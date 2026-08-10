using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using IAD.Infrastructure.Storage;
using IAD.Models;

namespace IAD.Services
{
    public sealed class OfflineInspectionService
    {
        private readonly ProductService products;
        private readonly RecipeService recipes;
        private readonly InferenceModelService models;
        private readonly ResultService results;
        private readonly OnnxInferenceEngine engine;
        private readonly RecipeRuleEngine ruleEngine;

        internal OfflineInspectionService(ProductService products, RecipeService recipes, InferenceModelService models, ResultService results)
        {
            this.products = products ?? throw new ArgumentNullException("products");
            this.recipes = recipes ?? throw new ArgumentNullException("recipes");
            this.models = models ?? throw new ArgumentNullException("models");
            this.results = results ?? throw new ArgumentNullException("results");
            engine = new OnnxInferenceEngine(models);
            ruleEngine = new RecipeRuleEngine();
        }

        public InspectionResult Inspect(long productId, string imagePath, string batchCode, string operatorName, CancellationToken cancellationToken)
        {
            DateTime started = DateTime.UtcNow;
            InspectionRecipe recipe = null;
            InferenceModel model = null;
            string archived = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath)) throw new FileNotFoundException("待检测图片不存在。", imagePath);
                recipe = recipes.GetActiveRecipe(productId);
                if (recipe == null) throw new InvalidOperationException("当前产品没有已启用的 Recipe。请先在规则与 Recipe 页面保存并启用 Recipe。");
                if (!recipe.ModelId.HasValue) throw new InvalidOperationException("当前 Recipe 尚未绑定 ONNX 模型。");
                model = models.GetModel(recipe.ModelId.Value);
                if (model == null || model.ProductId != productId) throw new InvalidOperationException("Recipe 绑定的模型不存在或不属于当前产品。");
                string modelPath = models.ResolveModelPath(model);
                if (!File.Exists(modelPath)) throw new FileNotFoundException("Recipe 绑定的模型文件不存在。", modelPath);

                archived = ArchiveOriginal(imagePath);
                cancellationToken.ThrowIfCancellationRequested();
                using (Bitmap bitmap = LoadBitmap(imagePath))
                {
                    Stopwatch watch = Stopwatch.StartNew();
                    IList<InferencePrediction> predictions = engine.Run(model, bitmap);
                    watch.Stop();
                    IList<DefectInstance> defects = MapDefects(productId, predictions);
                    string overall = ruleEngine.Evaluate(defects, recipe.Rules);
                    string annotated = SaveAnnotated(bitmap, archived, defects, overall);
                    InspectionResult result = CreateBaseResult(productId, imagePath, archived, recipe, model, batchCode, operatorName, started);
                    result.OverallResult = overall;
                    result.InferenceMilliseconds = watch.ElapsedMilliseconds;
                    result.AnnotatedImagePath = annotated;
                    foreach (DefectInstance defect in defects) result.Defects.Add(defect);
                    result.FinishedAtUtc = DateTime.UtcNow;
                    results.SaveInspectionResult(result);
                    return result;
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                if (archived == null && !string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                {
                    try { archived = ArchiveOriginal(imagePath); } catch { }
                }
                InspectionResult error = CreateBaseResult(productId, imagePath, archived, recipe, model, batchCode, operatorName, started);
                error.OverallResult = "ERROR";
                error.ErrorMessage = ex.Message;
                error.FinishedAtUtc = DateTime.UtcNow;
                error.InferenceMilliseconds = Math.Max(0, (long)(error.FinishedAtUtc - started).TotalMilliseconds);
                results.SaveInspectionResult(error);
                return error;
            }
        }

        private IList<DefectInstance> MapDefects(long productId, IList<InferencePrediction> predictions)
        {
            IList<DefectCategory> categories = products.GetDefectCategories(productId);
            List<DefectInstance> defects = new List<DefectInstance>();
            foreach (InferencePrediction prediction in predictions)
            {
                DefectCategory category = categories.FirstOrDefault(c =>
                    string.Equals(c.CategoryCode, prediction.Label, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.CategoryName, prediction.Label, StringComparison.OrdinalIgnoreCase));
                defects.Add(new DefectInstance
                {
                    CategoryId=category == null ? (long?)null : category.Id,
                    CategoryCode=category == null ? prediction.Label : category.CategoryCode,
                    CategoryName=category == null ? prediction.Label : category.CategoryName,
                    RoiName="全图", Confidence=prediction.Confidence, X=prediction.X, Y=prediction.Y,
                    Width=prediction.Width, Height=prediction.Height,
                    Area=prediction.Width * prediction.Height
                });
            }
            return defects;
        }

        private static InspectionResult CreateBaseResult(long productId, string originalPath, string archivedPath, InspectionRecipe recipe, InferenceModel model, string batchCode, string operatorName, DateTime started)
        {
            return new InspectionResult
            {
                ProductId=productId, RecipeId=recipe == null ? (long?)null : recipe.Id, ModelId=model == null ? (long?)null : model.Id,
                BatchCode=string.IsNullOrWhiteSpace(batchCode) ? DateTime.Now.ToString("yyyyMMdd-HHmmss") : batchCode.Trim(),
                SourceImagePath=archivedPath ?? originalPath, OriginalImagePath=originalPath, ArchivedImagePath=archivedPath,
                ModelVersion=model == null ? null : model.Version, RuleVersion=recipe == null ? null : recipe.RuleVersion,
                OperatorName=string.IsNullOrWhiteSpace(operatorName) ? "system" : operatorName.Trim(), StartedAtUtc=started
            };
        }

        private static Bitmap LoadBitmap(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (Image image = Image.FromStream(stream)) return new Bitmap(image);
        }

        private static string ArchiveOriginal(string sourcePath)
        {
            string relativeFolder = Path.Combine("Results", DateTime.Now.ToString("yyyyMMdd"));
            string fullFolder = Path.Combine(ProjectStoragePaths.RootPath, relativeFolder);
            Directory.CreateDirectory(fullFolder);
            string extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrWhiteSpace(extension)) extension = ".png";
            string fileName = DateTime.Now.ToString("HHmmssfff") + "_" + Guid.NewGuid().ToString("N") + extension.ToLowerInvariant();
            File.Copy(sourcePath, Path.Combine(fullFolder, fileName), false);
            return Path.Combine(relativeFolder, fileName);
        }

        private static string SaveAnnotated(Bitmap source, string archivedPath, IList<DefectInstance> defects, string overall)
        {
            string relativeFolder = string.IsNullOrWhiteSpace(archivedPath) ? Path.Combine("Results", DateTime.Now.ToString("yyyyMMdd")) : Path.GetDirectoryName(archivedPath);
            string fileName = Path.GetFileNameWithoutExtension(archivedPath ?? Guid.NewGuid().ToString("N")) + ".annotated.png";
            string relative = Path.Combine(relativeFolder, fileName);
            string full = Path.Combine(ProjectStoragePaths.RootPath, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            using (Bitmap annotated = new Bitmap(source))
            using (Graphics graphics = Graphics.FromImage(annotated))
            using (Pen pen = new Pen(overall == "NG" ? Color.Red : Color.LimeGreen, Math.Max(2, source.Width / 500f)))
            using (Font font = new Font("Microsoft YaHei UI", Math.Max(10, source.Width / 80f), FontStyle.Bold))
            {
                foreach (DefectInstance defect in defects)
                {
                    RectangleF box = new RectangleF((float)defect.X, (float)defect.Y, Math.Max(1, (float)(defect.Width)), Math.Max(1, (float)(defect.Height)));
                    graphics.DrawRectangle(pen, box.X, box.Y, box.Width, box.Height);
                    string label = (defect.CategoryName ?? defect.CategoryCode) + " " + defect.Confidence.ToString("P1") + " " + defect.Result;
                    SizeF size = graphics.MeasureString(label, font);
                    float textY = Math.Max(0, box.Y - size.Height);
                    using (Brush background = new SolidBrush(Color.FromArgb(190, pen.Color))) graphics.FillRectangle(background, box.X, textY, size.Width, size.Height);
                    graphics.DrawString(label, font, Brushes.White, box.X, textY);
                }
                annotated.Save(full, ImageFormat.Png);
            }
            return relative;
        }
    }
}
