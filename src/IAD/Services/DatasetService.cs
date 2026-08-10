using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using IAD.Infrastructure.Storage;
using IAD.Models;
using IAD.Repositories;

namespace IAD.Services
{
    public sealed class DatasetService
    {
        private readonly IProductRepository products;
        private readonly IProductDefinitionSettingsRepository definitionSettings;
        private readonly IDefectCategoryRepository categories;
        private readonly IDatasetRepository datasets;
        private readonly IDatasetMaskRepository masks;

        public DatasetService(
            IProductRepository products,
            IProductDefinitionSettingsRepository definitionSettings,
            IDefectCategoryRepository categories,
            IDatasetRepository datasets,
            IDatasetMaskRepository masks)
        {
            this.products = products ?? throw new ArgumentNullException("products");
            this.definitionSettings = definitionSettings ?? throw new ArgumentNullException("definitionSettings");
            this.categories = categories ?? throw new ArgumentNullException("categories");
            this.datasets = datasets ?? throw new ArgumentNullException("datasets");
            this.masks = masks ?? throw new ArgumentNullException("masks");
        }

        public IList<DatasetImage> GetImages(long productId)
        {
            EnsureProductExists(productId);
            return datasets.GetImagesByProduct(productId);
        }

        public DatasetImage GetImage(long imageId)
        {
            return EnsureImageExists(imageId);
        }

        public IDictionary<long, int> GetClassCounts(long productId)
        {
            EnsureProductExists(productId);
            return datasets.GetClassCounts(productId);
        }

        public ProductDefinitionSettings GetSavedProductDefinition(long productId)
        {
            EnsureProductExists(productId);
            return definitionSettings.GetByProduct(productId);
        }

        public DatasetImage ImportImage(long productId, string sourcePath)
        {
            return ImportImageChecked(productId, sourcePath).Image;
        }

        public DatasetImageImportResult ImportImageChecked(long productId, string sourcePath, bool backfillExistingHashes = true)
        {
            Product product = EnsureProductExists(productId);
            ProductDefinitionSettings savedDefinition = EnsureSavedProductDefinition(productId);
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

            string contentHash = ComputeContentHash(sourcePath);
            if (backfillExistingHashes) BackfillContentHashes(productId);
            DatasetImage duplicate = datasets.GetImageByContentHash(productId, contentHash);
            if (duplicate != null)
            {
                return new DatasetImageImportResult
                {
                    Image = duplicate,
                    IsDuplicate = true
                };
            }

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
                    ReviewStatus = DatasetReviewStatus.Pending,
                    DatasetSplit = DatasetSplit.Unassigned,
                    ContentHash = contentHash,
                    ProductDefinitionVersion = savedDefinition.ProductDefinitionVersion,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };
                image.Id = datasets.InsertImage(image);
                ProductDataRevisionTracker.MarkChanged(productId);
                return new DatasetImageImportResult { Image = image, IsDuplicate = false };
            }
            catch
            {
                if (File.Exists(targetPath)) File.Delete(targetPath);
                throw;
            }
        }

        public void BackfillContentHashes(long productId)
        {
            EnsureProductExists(productId);
            IList<DatasetImage> images = datasets.GetImagesByProduct(productId);
            foreach (DatasetImage image in images)
            {
                if (!string.IsNullOrWhiteSpace(image.ContentHash)) continue;
                string path;
                try
                {
                    path = GetImagePath(image);
                    if (!File.Exists(path)) continue;
                    string hash = ComputeContentHash(path);
                    DatasetImage existing = datasets.GetImageByContentHash(productId, hash);
                    if (existing != null && existing.Id != image.Id) continue;
                    datasets.UpdateImageContentHash(image.Id, hash, DateTime.UtcNow);
                    image.ContentHash = hash;
                }
                catch
                {
                    // 单张历史文件损坏或被移走时不阻塞其他图片导入。
                }
            }
        }

        public static string ComputeContentHash(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
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

        public string DeleteImage(long productId, long imageId)
        {
            EnsureProductExists(productId);
            DatasetImage image = EnsureImageExists(imageId);
            if (image.ProductId != productId)
                throw new InvalidOperationException("所选图片不属于当前产品，已阻止删除。");

            string imagePath = GetImagePath(image);
            bool retainedByVersion = datasets.IsImageReferencedByVersion(image.Id);
            string stagedPath = null;
            Dictionary<string, string> stagedMaskPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!retainedByVersion && File.Exists(imagePath))
            {
                stagedPath = imagePath + ".deleting-" + Guid.NewGuid().ToString("N");
                File.Move(imagePath, stagedPath);
            }

            foreach (DatasetMask mask in masks.GetByImage(image.Id))
            {
                if (masks.IsRelativePathReferencedByVersion(mask.RelativePath)) continue;
                string maskPath = ResolveWorkspacePath(mask.RelativePath);
                if (!File.Exists(maskPath)) continue;
                string stagedMaskPath = maskPath + ".deleting-" + Guid.NewGuid().ToString("N");
                File.Move(maskPath, stagedMaskPath);
                stagedMaskPaths[maskPath] = stagedMaskPath;
            }

            try
            {
                datasets.DeleteImage(image.Id, productId);
                ProductDataRevisionTracker.MarkChanged(productId);
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(stagedPath) && File.Exists(stagedPath) && !File.Exists(imagePath))
                    File.Move(stagedPath, imagePath);
                foreach (KeyValuePair<string, string> pair in stagedMaskPaths)
                    if (File.Exists(pair.Value) && !File.Exists(pair.Key)) File.Move(pair.Value, pair.Key);
                throw;
            }

            if (retainedByVersion)
                return "图片已从当前数据集删除；原始文件因被已发布版本引用而保留。";

            if (!string.IsNullOrWhiteSpace(stagedPath) && File.Exists(stagedPath))
            {
                try
                {
                    File.Delete(stagedPath);
                }
                catch (Exception ex)
                {
                    return "图片记录和标注已删除，但存储副本清理失败：" + ex.Message;
                }
            }
            foreach (string stagedMaskPath in stagedMaskPaths.Values)
            {
                try { if (File.Exists(stagedMaskPath)) File.Delete(stagedMaskPath); }
                catch (Exception ex) { return "图片记录、矢量标注和 Mask 已删除，但 Mask 文件清理失败：" + ex.Message; }
            }
            return null;
        }

        private static string ResolveWorkspacePath(string relativePath)
        {
            string root = Path.GetFullPath(ProjectStoragePaths.RootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string fullPath = Path.GetFullPath(Path.Combine(ProjectStoragePaths.RootPath, relativePath ?? string.Empty));
            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("数据文件路径超出当前 Workspace。");
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
            if (string.IsNullOrWhiteSpace(image.ProductDefinitionVersion))
                throw new InvalidOperationException("该图片尚未绑定已保存的产品定义版本，请重新导入图片。");
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
            ProductDataRevisionTracker.MarkChanged(image.ProductId);
            return annotation;
        }

        public void DeleteAnnotation(long imageId, long annotationId)
        {
            DatasetImage image = EnsureImageExists(imageId);
            datasets.DeleteAnnotation(annotationId, imageId);
            RefreshImageStatus(imageId);
            ProductDataRevisionTracker.MarkChanged(image.ProductId);
        }

        public void SetCategoryVisibility(long imageId, long? categoryId, string categoryName, bool isVisible)
        {
            DatasetImage image = EnsureImageExists(imageId);
            IList<DatasetAnnotation> items = datasets.GetAnnotationsByImage(imageId);
            bool changed = false;
            foreach (DatasetAnnotation item in items)
            {
                bool categoryMatches = categoryId.HasValue
                    ? item.CategoryId == categoryId
                    : !item.CategoryId.HasValue && string.Equals(item.CategoryName, categoryName, StringComparison.Ordinal);
                if (!categoryMatches || item.IsVisible == isVisible) continue;
                item.IsVisible = isVisible;
                item.UpdatedAtUtc = DateTime.UtcNow;
                datasets.UpdateAnnotation(item);
                changed = true;
            }
            if (changed) ProductDataRevisionTracker.MarkChanged(image.ProductId);
        }

        public int RepairAnnotationBounds(long imageId)
        {
            DatasetImage image = EnsureImageExists(imageId);
            int changed = 0;
            List<string> failures = new List<string>();

            foreach (DatasetAnnotation item in datasets.GetAnnotationsByImage(imageId))
            {
                try
                {
                    string repaired = AnnotationGeometry.RepairToBounds(
                        item.AnnotationType,
                        item.GeometryData,
                        item.BrushWidth,
                        image.Width,
                        image.Height);

                    if (string.Equals(repaired, item.GeometryData, StringComparison.Ordinal)) continue;
                    item.GeometryData = repaired;
                    item.UpdatedAtUtc = DateTime.UtcNow;
                    datasets.UpdateAnnotation(item);
                    changed++;
                }
                catch (Exception ex)
                {
                    failures.Add(
                        "标注 #" + item.Id + "（" + (item.CategoryName ?? item.AnnotationType ?? "未知") + "）：" + ex.Message);
                }
            }

            if (changed > 0) ProductDataRevisionTracker.MarkChanged(image.ProductId);

            if (failures.Count > 0)
            {
                string message = "边界检查已处理完成，已自动修复 " + changed + " 个标注，但有 " + failures.Count + " 个标注无法自动修复。";
                int detailCount = Math.Min(5, failures.Count);
                for (int i = 0; i < detailCount; i++) message += "\r\n• " + failures[i];
                if (failures.Count > detailCount) message += "\r\n• 其余 " + (failures.Count - detailCount) + " 个未展开。";
                message += "\r\n请使用“编辑标注”人工调整这些异常标注。";
                throw new InvalidOperationException(message);
            }

            return changed;
        }

        public DatasetVersion GetLatestVersion(long productId)
        {
            EnsureProductExists(productId);
            return datasets.GetLatestVersion(productId);
        }

        public IList<DatasetVersion> GetVersions(long productId)
        {
            EnsureProductExists(productId);
            return datasets.GetVersions(productId);
        }

        public IList<DatasetVersionImage> GetVersionImages(long versionId)
        {
            return datasets.GetVersionImages(versionId);
        }

        public IList<DatasetVersionAnnotation> GetVersionAnnotations(long versionId)
        {
            return datasets.GetVersionAnnotations(versionId);
        }

        public IList<DatasetVersionMask> GetVersionMasks(long versionId)
        {
            return datasets.GetVersionMasks(versionId);
        }

        public void RestoreVersion(long productId, long versionId)
        {
            EnsureProductExists(productId);
            datasets.RestoreVersion(productId, versionId, DateTime.UtcNow);
            CleanupOrphanImageFiles();
            ProductDataRevisionTracker.MarkChanged(productId);
        }

        public void CleanupOrphanImageFiles()
        {
            if (!Directory.Exists(ProjectStoragePaths.ImagesPath)) return;
            HashSet<string> referenced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string relativePath in datasets.GetAllReferencedImagePaths())
            {
                try { referenced.Add(ResolveWorkspacePath(relativePath)); }
                catch { }
            }
            foreach (string path in Directory.GetFiles(ProjectStoragePaths.ImagesPath, "*.*", SearchOption.AllDirectories))
            {
                string fullPath = Path.GetFullPath(path);
                if (referenced.Contains(fullPath)) continue;
                try { File.Delete(fullPath); }
                catch { }
            }
        }

        public DatasetVersion CreateVersion(long productId, string notes)
        {
            EnsureProductExists(productId);
            ProductDefinitionSettings savedDefinition = EnsureSavedProductDefinition(productId);
            int imageCount = datasets.CountImages(productId);
            if (imageCount == 0)
                throw new InvalidOperationException("请先导入至少一张数据集图片，再发布版本。");

            DatasetVersion latest = datasets.GetLatestVersion(productId);
            DatasetVersion version = new DatasetVersion
            {
                ProductId = productId,
                VersionCode = NextVersion(latest == null ? null : latest.VersionCode),
                ProductDefinitionVersion = savedDefinition.ProductDefinitionVersion,
                ImageCount = imageCount,
                AnnotationCount = datasets.CountAnnotations(productId),
                MaskCount = datasets.CountMasks(productId),
                Notes = notes,
                CreatedAtUtc = DateTime.UtcNow
            };
            version.Id = datasets.InsertVersion(version);
            ProductDataRevisionTracker.MarkChanged(productId);
            return version;
        }

        private void RefreshImageStatus(long imageId)
        {
            bool hasAnnotations = datasets.GetAnnotationsByImage(imageId).Count > 0;
            bool hasMasks = masks.GetByImage(imageId).Count > 0;
            string status = hasAnnotations || hasMasks ? "已标注" : "未标注";
            datasets.UpdateImageStatus(imageId, status, DateTime.UtcNow);
        }

        private Product EnsureProductExists(long productId)
        {
            Product product = productId > 0 ? products.GetById(productId) : null;
            if (product == null) throw new InvalidOperationException("请先在“产品定义”中创建并保存产品。");
            return product;
        }

        private ProductDefinitionSettings EnsureSavedProductDefinition(long productId)
        {
            ProductDefinitionSettings savedDefinition = definitionSettings.GetByProduct(productId);
            if (savedDefinition == null || string.IsNullOrWhiteSpace(savedDefinition.ProductDefinitionVersion))
                throw new InvalidOperationException("请先在“产品定义”页面保存当前产品，再进入数据集标注。");
            return savedDefinition;
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
