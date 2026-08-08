using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using IAD.Infrastructure.Storage;
using IAD.Models;
using IAD.Repositories;

namespace IAD.Services
{
    public sealed class DatasetService
    {
        private readonly IProductRepository products;
        private readonly IDefectCategoryRepository categories;
        private readonly IDatasetRepository datasets;

        public DatasetService(IProductRepository products, IDefectCategoryRepository categories, IDatasetRepository datasets)
        {
            this.products = products ?? throw new ArgumentNullException("products");
            this.categories = categories ?? throw new ArgumentNullException("categories");
            this.datasets = datasets ?? throw new ArgumentNullException("datasets");
        }

        public IList<DatasetImage> GetImages(long productId)
        {
            EnsureProductExists(productId);
            return datasets.GetImagesByProduct(productId);
        }

        public DatasetImage ImportImage(long productId, string sourcePath)
        {
            Product product = EnsureProductExists(productId);
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                throw new FileNotFoundException("待导入图片不存在。", sourcePath);

            int width;
            int height;
            using (Image source = Image.FromFile(sourcePath))
            {
                width = source.Width;
                height = source.Height;
            }
            if (width <= 0 || height <= 0)
                throw new InvalidDataException("图片尺寸无效：" + sourcePath);

            string productFolder = MakeSafeFileName(product.ProductCode);
            string targetDirectory = Path.Combine(ProjectStoragePaths.ImagesPath, productFolder);
            Directory.CreateDirectory(targetDirectory);
            string targetName = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture) + "_" +
                                Guid.NewGuid().ToString("N").Substring(0, 8) + Path.GetExtension(sourcePath).ToLowerInvariant();
            string targetPath = Path.Combine(targetDirectory, targetName);
            File.Copy(sourcePath, targetPath, false);

            try
            {
                DateTime now = DateTime.UtcNow;
                DatasetImage image = new DatasetImage
                {
                    ProductId = productId,
                    FileName = Path.GetFileName(sourcePath),
                    RelativePath = Path.Combine("Images", productFolder, targetName),
                    Width = width,
                    Height = height,
                    Status = "未标注",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };
                image.Id = datasets.InsertImage(image);
                return image;
            }
            catch
            {
                if (File.Exists(targetPath)) File.Delete(targetPath);
                throw;
            }
        }

        public string GetImagePath(DatasetImage image)
        {
            if (image == null) throw new ArgumentNullException("image");
            string root = Path.GetFullPath(ProjectStoragePaths.RootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string fullPath = Path.GetFullPath(Path.Combine(ProjectStoragePaths.RootPath, image.RelativePath ?? string.Empty));
            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("数据集图片路径超出当前 Workspace。");
            return fullPath;
        }

        public IList<DatasetAnnotation> GetAnnotations(long imageId)
        {
            DatasetImage image = EnsureImageExists(imageId);
            return datasets.GetAnnotationsByImage(image.Id);
        }

        public DatasetAnnotation CreateAnnotation(long imageId, long categoryId, string annotationType, string geometryData, float brushWidth, double confidence)
        {
            DatasetImage image = EnsureImageExists(imageId);
            DefectCategory category = categories.GetById(categoryId);
            if (category == null || category.ProductId != image.ProductId || !category.IsEnabled)
                throw new InvalidOperationException("当前瑕疵类别不存在或未启用。");
            if (brushWidth < 1 || float.IsNaN(brushWidth) || float.IsInfinity(brushWidth))
                throw new ArgumentException("画笔宽度必须大于等于 1。", "brushWidth");
            if (double.IsNaN(confidence) || double.IsInfinity(confidence) || confidence < 0 || confidence > 1)
                throw new ArgumentException("置信度必须在 0 到 1 之间。", "confidence");
            if (string.IsNullOrWhiteSpace(annotationType))
                throw new ArgumentException("标注类型不能为空。", "annotationType");

            string normalizedType = annotationType.Trim();
            string normalizedGeometry = AnnotationGeometry.ValidateAndNormalize(normalizedType, geometryData, image.Width, image.Height);

            DateTime now = DateTime.UtcNow;
            DatasetAnnotation annotation = new DatasetAnnotation
            {
                DatasetImageId = image.Id,
                CategoryId = category.Id,
                CategoryCode = category.CategoryCode,
                CategoryName = category.CategoryName,
                AnnotationType = normalizedType,
                GeometryData = normalizedGeometry,
                BrushWidth = brushWidth,
                Confidence = confidence,
                IsVisible = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            annotation.Id = datasets.InsertAnnotation(annotation);
            datasets.UpdateImageStatus(image.Id, "已标注", now);
            return annotation;
        }

        public void DeleteAnnotation(long imageId, long annotationId)
        {
            EnsureImageExists(imageId);
            datasets.DeleteAnnotation(annotationId, imageId);
            RefreshImageStatus(imageId);
        }

        public void SetCategoryVisibility(long imageId, long? categoryId, string categoryName, bool isVisible)
        {
            EnsureImageExists(imageId);
            IList<DatasetAnnotation> items = datasets.GetAnnotationsByImage(imageId);
            foreach (DatasetAnnotation item in items)
            {
                bool categoryMatches = categoryId.HasValue
                    ? item.CategoryId == categoryId
                    : !item.CategoryId.HasValue && string.Equals(item.CategoryName, categoryName, StringComparison.Ordinal);
                if (!categoryMatches || item.IsVisible == isVisible) continue;
                item.IsVisible = isVisible;
                item.UpdatedAtUtc = DateTime.UtcNow;
                datasets.UpdateAnnotation(item);
            }
        }

        public int RepairAnnotationBounds(long imageId)
        {
            DatasetImage image = EnsureImageExists(imageId);
            int changed = 0;
            foreach (DatasetAnnotation item in datasets.GetAnnotationsByImage(imageId))
            {
                string repaired = AnnotationGeometry.Clamp(item.GeometryData, image.Width, image.Height);
                repaired = AnnotationGeometry.ValidateAndNormalize(item.AnnotationType, repaired, image.Width, image.Height);
                if (string.Equals(repaired, item.GeometryData, StringComparison.Ordinal)) continue;
                item.GeometryData = repaired;
                item.UpdatedAtUtc = DateTime.UtcNow;
                datasets.UpdateAnnotation(item);
                changed++;
            }
            return changed;
        }

        public DatasetVersion GetLatestVersion(long productId)
        {
            EnsureProductExists(productId);
            return datasets.GetLatestVersion(productId);
        }

        public DatasetVersion CreateVersion(long productId, string notes)
        {
            EnsureProductExists(productId);
            int imageCount = datasets.CountImages(productId);
            if (imageCount == 0)
                throw new InvalidOperationException("请先导入至少一张数据集图片，再发布版本。");

            DatasetVersion latest = datasets.GetLatestVersion(productId);
            DatasetVersion version = new DatasetVersion
            {
                ProductId = productId,
                VersionCode = NextVersion(latest == null ? null : latest.VersionCode),
                ImageCount = imageCount,
                AnnotationCount = datasets.CountAnnotations(productId),
                Notes = notes,
                CreatedAtUtc = DateTime.UtcNow
            };
            version.Id = datasets.InsertVersion(version);
            return version;
        }

        private void RefreshImageStatus(long imageId)
        {
            string status = datasets.GetAnnotationsByImage(imageId).Count == 0 ? "未标注" : "已标注";
            datasets.UpdateImageStatus(imageId, status, DateTime.UtcNow);
        }

        private Product EnsureProductExists(long productId)
        {
            Product product = productId > 0 ? products.GetById(productId) : null;
            if (product == null) throw new InvalidOperationException("请先在“产品定义”中创建并保存产品。");
            return product;
        }

        private DatasetImage EnsureImageExists(long imageId)
        {
            DatasetImage image = imageId > 0 ? datasets.GetImageById(imageId) : null;
            if (image == null) throw new InvalidOperationException("数据集图片不存在。Id=" + imageId);
            return image;
        }

        private static string NextVersion(string value)
        {
            const string prefix = "DS-";
            if (!string.IsNullOrWhiteSpace(value) && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = value.Substring(prefix.Length).Split('.');
                int major;
                int minor;
                int patch;
                if (parts.Length == 3 && int.TryParse(parts[0], out major) && int.TryParse(parts[1], out minor) &&
                    int.TryParse(parts[2], out patch) && major >= 0 && minor >= 0 && patch >= 0 && patch < int.MaxValue)
                    return prefix + major + "." + minor + "." + (patch + 1);
            }
            return prefix + "1.0.0";
        }

        private static string MakeSafeFileName(string value)
        {
            string result = string.IsNullOrWhiteSpace(value) ? "Product" : value.Trim();
            foreach (char c in Path.GetInvalidFileNameChars()) result = result.Replace(c, '_');
            return result;
        }
    }
}
