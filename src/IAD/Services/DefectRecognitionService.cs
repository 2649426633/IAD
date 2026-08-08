using System;
using System.Collections.Generic;
using System.Drawing;
using IAD.Models;
using IAD.Repositories;

namespace IAD.Services
{
    public sealed class DefectRecognitionService
    {
        private readonly IProductRepository products;
        private readonly IDefectCategoryRepository categories;
        private readonly DatasetService datasets;
        private readonly IDefectRecognitionRepository recognition;

        internal DefectRecognitionService(
            IProductRepository products,
            IDefectCategoryRepository categories,
            DatasetService datasets,
            IDefectRecognitionRepository recognition)
        {
            this.products = products ?? throw new ArgumentNullException("products");
            this.categories = categories ?? throw new ArgumentNullException("categories");
            this.datasets = datasets ?? throw new ArgumentNullException("datasets");
            this.recognition = recognition ?? throw new ArgumentNullException("recognition");
        }

        public DefectRecognitionSettings GetSettings(long productId, long categoryId)
        {
            DefectCategory category = EnsureCategory(productId, categoryId);
            DefectRecognitionSettings settings = recognition.GetSettings(productId, categoryId);
            if (settings != null) return settings;
            return new DefectRecognitionSettings
            {
                ProductId = productId,
                CategoryId = categoryId,
                SimilarityThreshold = Math.Max(0.5D, Math.Min(0.95D, category.DefaultThreshold > 0 ? category.DefaultThreshold : 0.72D)),
                TopK = 10,
                UpdatedAtUtc = DateTime.UtcNow
            };
        }

        public DefectRecognitionSettings SaveSettings(DefectRecognitionSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            EnsureCategory(settings.ProductId, settings.CategoryId);
            if (double.IsNaN(settings.SimilarityThreshold) || double.IsInfinity(settings.SimilarityThreshold) ||
                settings.SimilarityThreshold < 0 || settings.SimilarityThreshold > 1)
                throw new ArgumentException("相似度阈值必须在 0 到 1 之间。", "settings");
            if (settings.TopK < 1 || settings.TopK > 100)
                throw new ArgumentException("Top-K 必须在 1 到 100 之间。", "settings");
            settings.UpdatedAtUtc = DateTime.UtcNow;
            recognition.UpsertSettings(settings);
            return settings;
        }

        public IList<DefectPrototypeSample> GetPositiveSamples(long productId, long categoryId)
        {
            EnsureCategory(productId, categoryId);
            List<DefectPrototypeSample> result = new List<DefectPrototypeSample>();
            foreach (DatasetImage image in datasets.GetImages(productId))
            {
                foreach (DatasetAnnotation annotation in datasets.GetAnnotations(image.Id))
                {
                    if (annotation.CategoryId != categoryId || !annotation.IsVisible) continue;
                    result.Add(new DefectPrototypeSample { Image = image, Annotation = annotation });
                }
            }
            return result;
        }

        public IList<DefectRecognitionCandidate> GenerateCandidates(DefectRecognitionSettings settings)
        {
            settings = SaveSettings(settings);
            IList<DatasetImage> images = datasets.GetImages(settings.ProductId);
            if (images.Count == 0)
                throw new InvalidOperationException("当前产品没有数据集图片，请先到“数据集标注”导入图片。");

            IList<DefectPrototypeSample> prototypes = GetPositiveSamples(settings.ProductId, settings.CategoryId);
            IList<DefectHardNegative> hardNegatives = recognition.GetHardNegatives(settings.ProductId, settings.CategoryId);
            IList<DefectRecognitionCandidate> generated = TemplateMatchingEngine.Generate(
                datasets, images, prototypes, hardNegatives, settings.ProductId, settings.CategoryId,
                settings.SimilarityThreshold, settings.TopK);

            string runCode = "REC-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            foreach (DefectRecognitionCandidate candidate in generated) candidate.RunCode = runCode;
            recognition.ReplacePendingCandidates(settings.ProductId, settings.CategoryId, runCode, generated);
            return recognition.GetLatestCandidates(settings.ProductId, settings.CategoryId);
        }

        public IList<DefectRecognitionCandidate> GetLatestCandidates(long productId, long categoryId)
        {
            EnsureCategory(productId, categoryId);
            return recognition.GetLatestCandidates(productId, categoryId);
        }

        public IList<DefectHardNegative> GetHardNegatives(long productId, long categoryId)
        {
            EnsureCategory(productId, categoryId);
            return recognition.GetHardNegatives(productId, categoryId);
        }

        public DefectRecognitionSummary GetSummary(long productId, long categoryId)
        {
            EnsureCategory(productId, categoryId);
            return recognition.GetSummary(productId, categoryId);
        }

        public DatasetAnnotation ConfirmCandidate(long productId, long categoryId, long candidateId)
        {
            DefectRecognitionCandidate candidate = EnsureCandidate(productId, categoryId, candidateId);
            if (string.Equals(candidate.Status, "已确认", StringComparison.Ordinal))
            {
                foreach (DatasetAnnotation existing in datasets.GetAnnotations(candidate.DatasetImageId))
                {
                    if (candidate.ConfirmedAnnotationId == existing.Id) return existing;
                }
            }
            if (!string.Equals(candidate.Status, "待确认", StringComparison.Ordinal))
                throw new InvalidOperationException("只有待确认候选可以生成为标注。当前状态：" + candidate.Status);

            foreach (DatasetAnnotation existing in datasets.GetAnnotations(candidate.DatasetImageId))
            {
                if (existing.CategoryId == categoryId && string.Equals(existing.GeometryData, candidate.GeometryData, StringComparison.Ordinal))
                {
                    candidate.Status = "已确认";
                    candidate.ConfirmedAnnotationId = existing.Id;
                    candidate.UpdatedAtUtc = DateTime.UtcNow;
                    recognition.UpdateCandidate(candidate);
                    return existing;
                }
            }

            DatasetAnnotation annotation = datasets.CreateAnnotation(
                candidate.DatasetImageId,
                categoryId,
                "Rectangle",
                candidate.GeometryData,
                2F,
                candidate.Similarity);
            candidate.Status = "已确认";
            candidate.ConfirmedAnnotationId = annotation.Id;
            candidate.UpdatedAtUtc = DateTime.UtcNow;
            recognition.UpdateCandidate(candidate);
            return annotation;
        }

        public DefectRecognitionCandidate RejectCandidate(long productId, long categoryId, long candidateId)
        {
            DefectRecognitionCandidate candidate = EnsureCandidate(productId, categoryId, candidateId);
            EnsurePending(candidate);
            candidate.Status = "已拒绝";
            candidate.UpdatedAtUtc = DateTime.UtcNow;
            recognition.UpdateCandidate(candidate);
            return candidate;
        }

        public DefectHardNegative AddHardNegative(long productId, long categoryId, long candidateId)
        {
            DefectRecognitionCandidate candidate = EnsureCandidate(productId, categoryId, candidateId);
            if (string.Equals(candidate.Status, "已确认", StringComparison.Ordinal))
                throw new InvalidOperationException("已确认候选已经成为正样本，不能再加入 Hard Negative。");

            DefectHardNegative item = new DefectHardNegative
            {
                ProductId = productId,
                CategoryId = categoryId,
                DatasetImageId = candidate.DatasetImageId,
                SourceFileName = candidate.SourceFileName,
                GeometryData = candidate.GeometryData,
                Similarity = candidate.Similarity,
                CreatedAtUtc = DateTime.UtcNow
            };
            item.Id = recognition.InsertHardNegative(item);
            candidate.Status = "Hard Negative";
            candidate.UpdatedAtUtc = DateTime.UtcNow;
            recognition.UpdateCandidate(candidate);
            return item;
        }

        public DefectRecognitionCandidate RefineCandidate(long productId, long categoryId, long candidateId)
        {
            DefectRecognitionCandidate candidate = EnsureCandidate(productId, categoryId, candidateId);
            EnsurePending(candidate);
            DatasetImage image = FindImage(productId, candidate.DatasetImageId);
            string refined = TemplateMatchingEngine.RefineBounds(datasets.GetImagePath(image), candidate.GeometryData);
            refined = AnnotationGeometry.ValidateAndNormalize("Rectangle", refined, image.Width, image.Height);
            candidate.GeometryData = refined;
            candidate.UpdatedAtUtc = DateTime.UtcNow;
            recognition.UpdateCandidate(candidate);
            return candidate;
        }

        public Bitmap BuildCandidatePreview(long productId, long categoryId, long candidateId, bool heatmap)
        {
            DefectRecognitionCandidate candidate = EnsureCandidate(productId, categoryId, candidateId);
            DatasetImage image = FindImage(productId, candidate.DatasetImageId);
            using (Image source = Image.FromFile(datasets.GetImagePath(image)))
            {
                Bitmap preview = new Bitmap(source);
                RectangleF bounds = TemplateMatchingEngine.ParseRectangle(candidate.GeometryData);
                using (Graphics graphics = Graphics.FromImage(preview))
                {
                    if (heatmap)
                    {
                        int alpha = (int)Math.Max(40, Math.Min(180, candidate.Similarity * 180D));
                        using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, 255, 80, 0)))
                            graphics.FillRectangle(brush, bounds);
                    }
                    using (Pen pen = new Pen(heatmap ? Color.Yellow : Color.Red, Math.Max(2F, preview.Width / 500F)))
                        graphics.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                }
                return preview;
            }
        }

        private DefectCategory EnsureCategory(long productId, long categoryId)
        {
            if (productId <= 0 || products.GetById(productId) == null)
                throw new InvalidOperationException("请先在“产品定义”中选择产品。");
            DefectCategory category = categoryId > 0 ? categories.GetById(categoryId) : null;
            if (category == null || category.ProductId != productId || !category.IsEnabled)
                throw new InvalidOperationException("瑕疵类别不存在、不属于当前产品或已停用。");
            return category;
        }

        private DefectRecognitionCandidate EnsureCandidate(long productId, long categoryId, long candidateId)
        {
            EnsureCategory(productId, categoryId);
            DefectRecognitionCandidate candidate = candidateId > 0 ? recognition.GetCandidateById(candidateId) : null;
            if (candidate == null || candidate.ProductId != productId || candidate.CategoryId != categoryId)
                throw new InvalidOperationException("识别候选不存在或不属于当前产品类别。Id=" + candidateId);
            return candidate;
        }

        private DatasetImage FindImage(long productId, long imageId)
        {
            foreach (DatasetImage image in datasets.GetImages(productId))
            {
                if (image.Id == imageId) return image;
            }
            throw new InvalidOperationException("候选来源图片不存在。Id=" + imageId);
        }

        private static void EnsurePending(DefectRecognitionCandidate candidate)
        {
            if (!string.Equals(candidate.Status, "待确认", StringComparison.Ordinal))
                throw new InvalidOperationException("只有待确认候选可以执行此操作。当前状态：" + candidate.Status);
        }
    }
}
