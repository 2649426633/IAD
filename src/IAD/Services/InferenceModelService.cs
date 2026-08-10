using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using IAD.Infrastructure.Storage;
using IAD.Models;
using IAD.Repositories;
using Microsoft.ML.OnnxRuntime;

namespace IAD.Services
{
    public sealed class InferenceModelService
    {
        private static readonly HashSet<string> SupportedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Classification", "YoloV5", "YoloV8"
        };
        private readonly IProductRepository products;
        private readonly IDefectCategoryRepository categories;
        private readonly IInferenceModelRepository models;

        internal InferenceModelService(IProductRepository products, IDefectCategoryRepository categories, IInferenceModelRepository models)
        {
            this.products = products ?? throw new ArgumentNullException("products");
            this.categories = categories ?? throw new ArgumentNullException("categories");
            this.models = models ?? throw new ArgumentNullException("models");
        }

        public IList<InferenceModel> GetModels(long productId)
        {
            EnsureProduct(productId);
            return models.GetByProduct(productId);
        }

        public InferenceModel GetModel(long modelId) { return models.GetById(modelId); }
        public InferenceModel GetActiveModel(long productId) { EnsureProduct(productId); return models.GetActiveByProduct(productId); }

        public InferenceModel Import(string sourcePath, InferenceModel definition)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            EnsureProduct(definition.ProductId);
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath)) throw new FileNotFoundException("请选择有效的 ONNX 模型文件。", sourcePath);
            if (!string.Equals(Path.GetExtension(sourcePath), ".onnx", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("仅支持 .onnx 模型文件。", "sourcePath");

            definition.ModelCode = Required(definition.ModelCode, "模型编号");
            definition.ModelName = Required(definition.ModelName, "模型名称");
            definition.Version = Required(definition.Version, "模型版本");
            definition.ModelType = Required(definition.ModelType, "输出类型");
            if (!SupportedTypes.Contains(definition.ModelType)) throw new ArgumentException("输出类型仅支持 Classification、YoloV5 或 YoloV8。");
            ValidateThreshold(definition.ConfidenceThreshold, "置信度阈值");
            ValidateThreshold(definition.NmsThreshold, "NMS 阈值");
            string[] labels = OnnxInferenceEngine.ParseLabels(definition.Labels);
            if (labels.Length == 0) throw new ArgumentException("请按模型训练时的索引顺序填写类别编码。");
            IList<DefectCategory> productCategories = categories.GetByProduct(definition.ProductId);
            string[] unknownLabels = labels.Where(label => !IsNormalLabel(label) && !productCategories.Any(category =>
                string.Equals(category.CategoryCode, label, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(category.CategoryName, label, StringComparison.OrdinalIgnoreCase))).ToArray();
            if (unknownLabels.Length > 0) throw new ArgumentException("以下模型类别未在当前产品中定义：" + string.Join("、", unknownLabels));

            using (InferenceSession session = new InferenceSession(sourcePath))
            {
                KeyValuePair<string, NodeMetadata> input = session.InputMetadata.FirstOrDefault(p => p.Value.ElementType == typeof(float));
                if (string.IsNullOrEmpty(input.Key)) throw new InvalidOperationException("模型没有可用的 float 图像输入。当前版本支持 NCHW float 输入。");
                KeyValuePair<string, NodeMetadata> output = session.OutputMetadata.FirstOrDefault();
                if (string.IsNullOrEmpty(output.Key)) throw new InvalidOperationException("模型没有输出节点。");
                if (output.Value.ElementType != typeof(float)) throw new InvalidOperationException("模型输出必须是 float 张量。");
                int[] dimensions = input.Value.Dimensions;
                if (dimensions.Length != 4) throw new InvalidOperationException("模型输入必须是四维 NCHW 张量。");
                if (dimensions[1] > 0 && dimensions[1] != 3) throw new InvalidOperationException("模型输入通道数必须为 3（RGB）。");
                definition.InputName = input.Key;
                definition.OutputName = output.Key;
                if (definition.InputHeight <= 0) definition.InputHeight = dimensions[2] > 0 ? dimensions[2] : 640;
                if (definition.InputWidth <= 0) definition.InputWidth = dimensions[3] > 0 ? dimensions[3] : 640;
                if (string.Equals(definition.ModelType, "Classification", StringComparison.OrdinalIgnoreCase))
                {
                    int[] outputDimensions = output.Value.Dimensions;
                    int classCount = outputDimensions.Length == 0 ? -1 : outputDimensions[outputDimensions.Length - 1];
                    if (classCount > 0 && classCount != labels.Length)
                        throw new InvalidOperationException("分类模型输出类别数为 " + classCount + "，但类别顺序填写了 " + labels.Length + " 项。");
                }
            }

            DateTime now = DateTime.UtcNow;
            definition.CreatedAtUtc = now;
            definition.UpdatedAtUtc = now;
            definition.Sha256 = ComputeSha256(sourcePath);
            string folder = Path.Combine(ProjectStoragePaths.ModelsPath, definition.ProductId.ToString());
            Directory.CreateDirectory(folder);
            string fileName = Sanitize(definition.ModelCode) + "_" + Sanitize(definition.Version) + "_" + definition.Sha256.Substring(0, 12) + ".onnx";
            string destination = Path.Combine(folder, fileName);
            if (!File.Exists(destination)) File.Copy(sourcePath, destination, false);
            definition.RelativePath = Path.Combine("Models", definition.ProductId.ToString(), fileName);

            try
            {
                definition.Id = models.Insert(definition);
                if (definition.IsActive) models.Activate(definition.ProductId, definition.Id);
                InspectionConfigurationRevisionTracker.MarkChanged(definition.ProductId);
                return definition;
            }
            catch
            {
                if (!models.GetByProduct(definition.ProductId).Any(m => string.Equals(m.RelativePath, definition.RelativePath, StringComparison.OrdinalIgnoreCase)))
                {
                    try { File.Delete(destination); } catch { }
                }
                throw;
            }
        }

        public void Activate(long productId, long modelId)
        {
            EnsureProduct(productId);
            InferenceModel model = models.GetById(modelId);
            if (model == null || model.ProductId != productId) throw new InvalidOperationException("模型不存在或不属于当前产品。");
            models.Activate(productId, modelId);
            InspectionConfigurationRevisionTracker.MarkChanged(productId);
        }

        public void Delete(long productId, long modelId)
        {
            InferenceModel model = models.GetById(modelId);
            if (model == null || model.ProductId != productId) throw new InvalidOperationException("模型不存在或不属于当前产品。");
            models.Delete(productId, modelId);
            InspectionConfigurationRevisionTracker.MarkChanged(productId);
            string path = ResolveModelPath(model);
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        public string ResolveModelPath(InferenceModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.RelativePath)) throw new InvalidOperationException("模型文件路径为空。");
            string root = Path.GetFullPath(ProjectStoragePaths.RootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string full = Path.GetFullPath(Path.Combine(ProjectStoragePaths.RootPath, model.RelativePath));
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("模型路径超出项目工作区。");
            return full;
        }

        private void EnsureProduct(long productId)
        {
            if (productId <= 0 || products.GetById(productId) == null) throw new InvalidOperationException("产品不存在。Id=" + productId);
        }

        private static string Required(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + "不能为空。");
            return value.Trim();
        }

        private static void ValidateThreshold(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0 || value > 1) throw new ArgumentException(name + "必须在 0 到 1 之间。");
        }

        private static string ComputeSha256(string path)
        {
            using (SHA256 hash = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                StringBuilder value = new StringBuilder();
                foreach (byte item in hash.ComputeHash(stream)) value.Append(item.ToString("x2"));
                return value.ToString();
            }
        }

        private static string Sanitize(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return value.Replace(' ', '_');
        }

        private static bool IsNormalLabel(string label)
        {
            return string.Equals(label, "normal", StringComparison.OrdinalIgnoreCase) || string.Equals(label, "ok", StringComparison.OrdinalIgnoreCase) || label == "正常" || label == "良品";
        }
    }
}
