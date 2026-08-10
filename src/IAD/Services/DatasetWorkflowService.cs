using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using IAD.Infrastructure.Storage;
using IAD.Models;
using IAD.Repositories;

namespace IAD.Services
{
    public sealed class DatasetWorkflowService
    {
        private readonly IProductRepository products;
        private readonly IDefectCategoryRepository categories;
        private readonly IDatasetRepository datasets;
        private readonly IDatasetMaskRepository masks;
        private readonly DatasetService datasetService;
        private readonly DatasetMaskService maskService;

        internal DatasetWorkflowService(
            IProductRepository products,
            IDefectCategoryRepository categories,
            IDatasetRepository datasets,
            IDatasetMaskRepository masks,
            DatasetService datasetService,
            DatasetMaskService maskService)
        {
            this.products = products ?? throw new ArgumentNullException("products");
            this.categories = categories ?? throw new ArgumentNullException("categories");
            this.datasets = datasets ?? throw new ArgumentNullException("datasets");
            this.masks = masks ?? throw new ArgumentNullException("masks");
            this.datasetService = datasetService ?? throw new ArgumentNullException("datasetService");
            this.maskService = maskService ?? throw new ArgumentNullException("maskService");
        }

        public DatasetImageQuality EvaluateImage(long imageId)
        {
            DatasetImage image = RequireImage(imageId);
            IList<DatasetAnnotation> annotations = datasets.GetAnnotationsByImage(imageId);
            IList<DatasetMask> imageMasks = masks.GetByImage(imageId);
            DatasetImageQuality result = new DatasetImageQuality
            {
                ImageId = image.Id,
                FileName = image.FileName,
                VectorAnnotationCount = annotations.Count,
                MaskCount = imageMasks.Count
            };

            int checkedBoundaries = 0;
            int validBoundaries = 0;
            HashSet<string> uniqueAnnotations = new HashSet<string>(StringComparer.Ordinal);
            foreach (DatasetAnnotation annotation in annotations)
            {
                checkedBoundaries++;
                try
                {
                    IList<PointF> points = AnnotationGeometry.Parse(annotation.GeometryData);
                    bool inBounds = IsGeometryInside(annotation, points, image.Width, image.Height);
                    if (inBounds) validBoundaries++;
                    else AddIssue(result, "Error", "OUT_OF_BOUNDS", "标注 #" + annotation.Id + " 超出图像边界。");

                    double area = EstimateArea(annotation, points);
                    if (area <= 0.5D)
                        AddIssue(result, "Error", "EMPTY_GEOMETRY", "标注 #" + annotation.Id + " 没有有效面积或长度。");

                    if (annotation.CategoryId.HasValue)
                    {
                        DefectCategory category = categories.GetById(annotation.CategoryId.Value);
                        if (category == null)
                            AddIssue(result, "Warning", "MISSING_CATEGORY", "标注 #" + annotation.Id + " 关联的类别已不存在。");
                        else if (category.MinArea > 0 && area < category.MinArea)
                            AddIssue(result, "Warning", "BELOW_MIN_AREA", "标注 #" + annotation.Id + " 小于类别最小面积 " + category.MinArea.ToString("0.##", CultureInfo.InvariantCulture) + "。");
                    }

                    string signature = (annotation.CategoryId ?? 0) + "|" + annotation.AnnotationType + "|" + annotation.GeometryData;
                    if (!uniqueAnnotations.Add(signature))
                        AddIssue(result, "Warning", "DUPLICATE_ANNOTATION", "检测到完全重复的矢量标注 #" + annotation.Id + "。");
                }
                catch (Exception ex)
                {
                    AddIssue(result, "Error", "INVALID_GEOMETRY", "标注 #" + annotation.Id + " 几何数据无效：" + ex.Message);
                }
            }

            foreach (DatasetMask mask in imageMasks)
            {
                checkedBoundaries++;
                bool valid = mask.Width == image.Width && mask.Height == image.Height && mask.PixelCount > 0;
                if (mask.Width != image.Width || mask.Height != image.Height)
                    AddIssue(result, "Error", "MASK_SIZE", "类别“" + mask.CategoryName + "”的 Mask 尺寸与原图不一致。");
                if (mask.PixelCount <= 0)
                    AddIssue(result, "Error", "EMPTY_MASK", "类别“" + mask.CategoryName + "”的 Mask 没有前景像素。");
                string path;
                try { path = ResolveWorkspacePath(mask.RelativePath); }
                catch (Exception ex)
                {
                    AddIssue(result, "Error", "MASK_PATH", "Mask 路径无效：" + ex.Message);
                    continue;
                }
                if (!File.Exists(path))
                {
                    AddIssue(result, "Error", "MASK_MISSING", "类别“" + mask.CategoryName + "”的 Mask PNG 不存在。");
                    valid = false;
                }
                if (valid) validBoundaries++;
            }

            bool hasLabels = annotations.Count > 0 || imageMasks.Count > 0;
            if (string.Equals(image.ReviewStatus, DatasetReviewStatus.Normal, StringComparison.OrdinalIgnoreCase) && hasLabels)
                AddIssue(result, "Error", "NORMAL_HAS_LABEL", "图片已标记为正常样本，但仍存在缺陷标注。");
            if (!hasLabels && !string.Equals(image.ReviewStatus, DatasetReviewStatus.Normal, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(image.ReviewStatus, DatasetReviewStatus.Ignored, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(image.ReviewStatus, DatasetReviewStatus.Rejected, StringComparison.OrdinalIgnoreCase))
                AddIssue(result, "Error", "UNCONFIRMED_EMPTY", "图片没有标注，也没有被确认成正常样本。");

            result.BoundaryScore = checkedBoundaries == 0
                ? (string.Equals(image.ReviewStatus, DatasetReviewStatus.Normal, StringComparison.OrdinalIgnoreCase) ? 1D : 0D)
                : validBoundaries / (double)checkedBoundaries;
            int errors = CountSeverity(result, "Error");
            int warnings = CountSeverity(result, "Warning");
            result.QualityScore = Math.Max(0D, Math.Min(1D, 1D - errors * 0.25D - warnings * 0.08D));
            result.CanApprove = errors == 0 && (hasLabels || string.Equals(image.ReviewStatus, DatasetReviewStatus.Normal, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        public DatasetQualityReport EvaluateProduct(long productId)
        {
            RequireProduct(productId);
            DatasetQualityReport report = new DatasetQualityReport();
            foreach (DatasetImage image in datasets.GetImagesByProduct(productId))
            {
                DatasetImageQuality quality = EvaluateImage(image.Id);
                if (!IsPublicationReadyStatus(image.ReviewStatus))
                    AddIssue(quality, "Error", "REVIEW_NOT_READY", "图片尚未审核通过、确认正常或设为忽略。");
                report.Images.Add(quality);
                report.ImageCount++;
                int errors = CountSeverity(quality, "Error");
                int warnings = CountSeverity(quality, "Warning");
                if (errors > 0) report.ErrorCount++;
                else if (warnings > 0) report.WarningCount++;
                else report.PassedCount++;
            }
            return report;
        }

        public void SetReviewStatus(long productId, IEnumerable<long> imageIds, string reviewStatus, string comment, string reviewer)
        {
            RequireProduct(productId);
            if (!DatasetReviewStatus.IsValid(reviewStatus))
                throw new ArgumentException("无效的审核状态：" + reviewStatus, "reviewStatus");

            bool changed = false;
            foreach (long imageId in imageIds ?? new long[0])
            {
                DatasetImage image = RequireImage(imageId);
                if (image.ProductId != productId) throw new InvalidOperationException("图片不属于当前产品。Id=" + imageId);
                if (string.Equals(reviewStatus, DatasetReviewStatus.Normal, StringComparison.OrdinalIgnoreCase))
                {
                    if (datasets.GetAnnotationsByImage(imageId).Count > 0 || masks.GetByImage(imageId).Count > 0)
                        throw new InvalidOperationException("“" + image.FileName + "”仍有缺陷标注，不能标记为正常样本。");
                }
                if (string.Equals(reviewStatus, DatasetReviewStatus.Approved, StringComparison.OrdinalIgnoreCase))
                {
                    DatasetImageQuality quality = EvaluateImage(imageId);
                    if (!quality.CanApprove)
                        throw new InvalidOperationException("“" + image.FileName + "”未通过质量门禁：" + FirstIssue(quality));
                }

                image.ReviewStatus = reviewStatus;
                image.ReviewComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
                image.ReviewedBy = string.Equals(reviewStatus, DatasetReviewStatus.Pending, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : (string.IsNullOrWhiteSpace(reviewer) ? Environment.UserName : reviewer.Trim());
                image.ReviewedAtUtc = string.Equals(reviewStatus, DatasetReviewStatus.Pending, StringComparison.OrdinalIgnoreCase)
                    ? (DateTime?)null
                    : DateTime.UtcNow;
                image.Status = DisplayReviewStatus(reviewStatus, datasets.GetAnnotationsByImage(imageId).Count > 0 || masks.GetByImage(imageId).Count > 0);
                image.UpdatedAtUtc = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(image.DatasetSplit)) image.DatasetSplit = DatasetSplit.Unassigned;
                datasets.UpdateImageWorkflow(image);
                changed = true;
            }
            if (changed) ProductDataRevisionTracker.MarkChanged(productId);
        }

        public void SetSplit(long productId, IEnumerable<long> imageIds, string split)
        {
            RequireProduct(productId);
            if (!DatasetSplit.IsValid(split)) throw new ArgumentException("无效的数据集划分：" + split, "split");
            bool changed = false;
            foreach (long imageId in imageIds ?? new long[0])
            {
                DatasetImage image = RequireImage(imageId);
                if (image.ProductId != productId) throw new InvalidOperationException("图片不属于当前产品。Id=" + imageId);
                image.DatasetSplit = split;
                image.UpdatedAtUtc = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(image.ReviewStatus)) image.ReviewStatus = DatasetReviewStatus.Pending;
                datasets.UpdateImageWorkflow(image);
                changed = true;
            }
            if (changed) ProductDataRevisionTracker.MarkChanged(productId);
        }

        public void AssignSplits(long productId, int trainPercent, int validationPercent, int seed)
        {
            RequireProduct(productId);
            if (trainPercent < 0 || validationPercent < 0 || trainPercent + validationPercent > 100)
                throw new ArgumentException("训练集和验证集比例无效。");

            List<DatasetImage> eligible = new List<DatasetImage>();
            foreach (DatasetImage image in datasets.GetImagesByProduct(productId))
            {
                if (string.Equals(image.ReviewStatus, DatasetReviewStatus.Ignored, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(image.ReviewStatus, DatasetReviewStatus.Rejected, StringComparison.OrdinalIgnoreCase)) continue;
                eligible.Add(image);
            }
            eligible.Sort(delegate(DatasetImage left, DatasetImage right)
            {
                return string.CompareOrdinal(StableSortKey(left, seed), StableSortKey(right, seed));
            });

            int trainCount = (int)Math.Round(eligible.Count * trainPercent / 100D, MidpointRounding.AwayFromZero);
            int validationCount = (int)Math.Round(eligible.Count * validationPercent / 100D, MidpointRounding.AwayFromZero);
            if (trainCount + validationCount > eligible.Count) validationCount = eligible.Count - trainCount;
            for (int i = 0; i < eligible.Count; i++)
            {
                DatasetImage image = eligible[i];
                image.DatasetSplit = i < trainCount
                    ? DatasetSplit.Train
                    : (i < trainCount + validationCount ? DatasetSplit.Validation : DatasetSplit.Test);
                image.UpdatedAtUtc = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(image.ReviewStatus)) image.ReviewStatus = DatasetReviewStatus.Pending;
                datasets.UpdateImageWorkflow(image);
            }
            if (eligible.Count > 0) ProductDataRevisionTracker.MarkChanged(productId);
        }

        public DatasetImportResult ImportCoco(long productId, string annotationFile)
        {
            RequireProduct(productId);
            if (string.IsNullOrWhiteSpace(annotationFile) || !File.Exists(annotationFile))
                throw new FileNotFoundException("COCO 标注文件不存在。", annotationFile);

            object rootObject = new JavaScriptSerializer { MaxJsonLength = int.MaxValue }.DeserializeObject(
                File.ReadAllText(annotationFile, Encoding.UTF8));
            IDictionary<string, object> root = AsObject(rootObject, "COCO 根对象");
            IList<object> categoryObjects = GetArray(root, "categories");
            IList<object> imageObjects = GetArray(root, "images");
            IList<object> annotationObjects = GetArray(root, "annotations");

            Dictionary<long, DefectCategory> categoryMap = new Dictionary<long, DefectCategory>();
            foreach (object item in categoryObjects)
            {
                IDictionary<string, object> value = AsObject(item, "COCO category");
                long externalId = GetInt64(value, "id", true);
                string externalName = GetString(value, "name");
                categoryMap[externalId] = EnsureImportCategory(productId, externalId, externalName);
            }

            datasetService.BackfillContentHashes(productId);
            DatasetImportResult result = new DatasetImportResult();
            Dictionary<long, DatasetImage> imageMap = new Dictionary<long, DatasetImage>();
            Dictionary<long, string> desiredReviews = new Dictionary<long, string>();
            string annotationDirectory = Path.GetDirectoryName(Path.GetFullPath(annotationFile));

            foreach (object item in imageObjects)
            {
                IDictionary<string, object> value = AsObject(item, "COCO image");
                long externalId = GetInt64(value, "id", true);
                string fileName = GetString(value, "file_name");
                string sourcePath = ResolveImportedImagePath(annotationDirectory, fileName);
                DatasetImageImportResult imported = datasetService.ImportImageChecked(productId, sourcePath, false);
                imageMap[externalId] = imported.Image;
                if (imported.IsDuplicate) result.DuplicateImageCount++; else result.ImageCount++;

                string split = NormalizeImportedSplit(GetString(value, "split"), fileName);
                SetImportedSplit(imported.Image.Id, split);
                string review = GetString(value, "review_status");
                if (DatasetReviewStatus.IsValid(review)) desiredReviews[imported.Image.Id] = review;
            }

            foreach (object item in annotationObjects)
            {
                IDictionary<string, object> value = AsObject(item, "COCO annotation");
                long externalImageId = GetInt64(value, "image_id", true);
                long externalCategoryId = GetInt64(value, "category_id", true);
                DatasetImage image;
                DefectCategory category;
                if (!imageMap.TryGetValue(externalImageId, out image))
                {
                    result.Warnings.Add("跳过找不到图片的 COCO annotation，image_id=" + externalImageId + "。");
                    continue;
                }
                if (!categoryMap.TryGetValue(externalCategoryId, out category))
                {
                    result.Warnings.Add("跳过找不到类别的 COCO annotation，category_id=" + externalCategoryId + "。");
                    continue;
                }

                string annotationType;
                string geometry;
                if (!TryReadCocoGeometry(value, image, out annotationType, out geometry))
                {
                    result.Warnings.Add("跳过没有有效 bbox 或 polygon 的 COCO annotation。");
                    continue;
                }
                if (AddImportedAnnotation(image, category, annotationType, geometry)) result.AnnotationCount++;
            }

            DirectoryInfo annotationParent = Directory.GetParent(annotationDirectory);
            string datasetRoot = string.Equals(Path.GetFileName(annotationDirectory), "annotations", StringComparison.OrdinalIgnoreCase) && annotationParent != null
                ? annotationParent.FullName
                : annotationDirectory;
            ImportMaskFolder(productId, Path.Combine(datasetRoot, "masks"), result);
            ApplyImportedReviews(productId, desiredReviews, result);
            return result;
        }

        public DatasetImportResult ImportYolo(long productId, string datasetDirectory)
        {
            RequireProduct(productId);
            if (string.IsNullOrWhiteSpace(datasetDirectory) || !Directory.Exists(datasetDirectory))
                throw new DirectoryNotFoundException("YOLO 数据集目录不存在：" + datasetDirectory);

            string root = Path.GetFullPath(datasetDirectory);
            string imagesRoot = Directory.Exists(Path.Combine(root, "images")) ? Path.Combine(root, "images") : root;
            Dictionary<int, DefectCategory> categoryMap = LoadYoloCategories(productId, root);
            if (categoryMap.Count == 0)
                throw new InvalidOperationException("没有从 classes.txt 或 dataset.yaml 读取到 YOLO 类别。");

            datasetService.BackfillContentHashes(productId);
            DatasetImportResult result = new DatasetImportResult();
            List<string> imageFiles = CollectTrainingImages(imagesRoot);
            foreach (string imageFile in imageFiles)
            {
                DatasetImageImportResult imported = datasetService.ImportImageChecked(productId, imageFile, false);
                if (imported.IsDuplicate) result.DuplicateImageCount++; else result.ImageCount++;
                string relative = MakeRelativePath(imagesRoot, imageFile);
                string split = NormalizeImportedSplit(null, relative);
                SetImportedSplit(imported.Image.Id, split);

                string labelPath = Path.Combine(root, "labels", Path.ChangeExtension(relative, ".txt"));
                if (!File.Exists(labelPath))
                {
                    result.Warnings.Add("图片没有对应 YOLO 标签，保持待审核：" + Path.GetFileName(imageFile));
                    continue;
                }
                foreach (string rawLine in File.ReadAllLines(labelPath, Encoding.UTF8))
                {
                    string line = rawLine == null ? string.Empty : rawLine.Trim();
                    if (line.Length == 0) continue;
                    string[] tokens = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    int categoryIndex;
                    if (tokens.Length < 5 || !int.TryParse(tokens[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out categoryIndex))
                    {
                        result.Warnings.Add("跳过无效 YOLO 标签：" + Path.GetFileName(labelPath) + " — " + line);
                        continue;
                    }
                    DefectCategory category;
                    if (!categoryMap.TryGetValue(categoryIndex, out category))
                    {
                        result.Warnings.Add("YOLO 类别索引不存在：" + categoryIndex + "。");
                        continue;
                    }
                    string annotationType;
                    string geometry;
                    if (!TryReadYoloGeometry(tokens, imported.Image, out annotationType, out geometry))
                    {
                        result.Warnings.Add("跳过坐标无效的 YOLO 标签：" + Path.GetFileName(labelPath) + " — " + line);
                        continue;
                    }
                    if (AddImportedAnnotation(imported.Image, category, annotationType, geometry)) result.AnnotationCount++;
                }
            }
            ImportMaskFolder(productId, Path.Combine(root, "masks"), result);
            return result;
        }

        public DatasetExportResult ExportCurrent(long productId, DatasetExportOptions options)
        {
            Product product = RequireProduct(productId);
            IList<DatasetImage> sourceImages = datasets.GetImagesByProduct(productId);
            List<ExportImage> images = new List<ExportImage>();
            List<ExportAnnotation> annotations = new List<ExportAnnotation>();
            List<ExportMask> exportMasks = new List<ExportMask>();

            foreach (DatasetImage image in sourceImages)
            {
                if (!ShouldExport(image.ReviewStatus, options.ApprovedOnly)) continue;
                if (options.RequireQualityGate)
                {
                    DatasetImageQuality quality = EvaluateImage(image.Id);
                    if (!quality.CanApprove)
                        throw new InvalidOperationException("质量门禁未通过：“" + image.FileName + "” — " + FirstIssue(quality));
                    if (!IsPublicationReadyStatus(image.ReviewStatus))
                        throw new InvalidOperationException("审核状态未完成：“" + image.FileName + "”仍为待审核或已驳回。");
                }
                images.Add(ToExportImage(image));
                foreach (DatasetAnnotation annotation in datasets.GetAnnotationsByImage(image.Id))
                    annotations.Add(ToExportAnnotation(annotation));
                foreach (DatasetMask mask in masks.GetByImage(image.Id))
                    exportMasks.Add(ToExportMask(mask));
            }
            return Export(product, null, images, annotations, exportMasks, categories.GetByProduct(productId), options);
        }

        public DatasetExportResult ExportVersion(long productId, long versionId, DatasetExportOptions options)
        {
            Product product = RequireProduct(productId);
            DatasetVersion version = null;
            foreach (DatasetVersion item in datasets.GetVersions(productId))
                if (item.Id == versionId) { version = item; break; }
            if (version == null) throw new InvalidOperationException("数据集版本不存在或不属于当前产品。Id=" + versionId);

            List<ExportImage> images = new List<ExportImage>();
            foreach (DatasetVersionImage image in datasets.GetVersionImages(versionId))
            {
                if (!ShouldExport(image.ReviewStatus, options.ApprovedOnly)) continue;
                images.Add(new ExportImage
                {
                    Id = image.SourceImageId,
                    FileName = image.FileName,
                    RelativePath = image.RelativePath,
                    Width = image.Width,
                    Height = image.Height,
                    Split = image.DatasetSplit,
                    ReviewStatus = image.ReviewStatus
                });
            }
            HashSet<long> exportedImageIds = new HashSet<long>();
            foreach (ExportImage image in images) exportedImageIds.Add(image.Id);

            List<ExportAnnotation> annotations = new List<ExportAnnotation>();
            foreach (DatasetVersionAnnotation annotation in datasets.GetVersionAnnotations(versionId))
            {
                if (!exportedImageIds.Contains(annotation.SourceImageId)) continue;
                annotations.Add(new ExportAnnotation
                {
                    Id = annotation.SourceAnnotationId,
                    ImageId = annotation.SourceImageId,
                    CategoryCode = annotation.CategoryCode,
                    CategoryName = annotation.CategoryName,
                    AnnotationType = annotation.AnnotationType,
                    GeometryData = annotation.GeometryData,
                    BrushWidth = annotation.BrushWidth
                });
            }
            List<ExportMask> exportMasks = new List<ExportMask>();
            foreach (DatasetVersionMask mask in datasets.GetVersionMasks(versionId))
            {
                if (!exportedImageIds.Contains(mask.SourceImageId)) continue;
                exportMasks.Add(new ExportMask
                {
                    Id = mask.SourceMaskId,
                    ImageId = mask.SourceImageId,
                    CategoryCode = mask.CategoryCode,
                    CategoryName = mask.CategoryName,
                    RelativePath = mask.RelativePath,
                    Width = mask.Width,
                    Height = mask.Height,
                    PixelCount = mask.PixelCount
                });
            }
            if (options.RequireQualityGate)
                ValidateVersionExport(images, annotations, exportMasks);
            return Export(product, version.VersionCode, images, annotations, exportMasks, categories.GetByProduct(productId), options);
        }

        public DatasetVersionComparison CompareVersions(long productId, long leftVersionId, long rightVersionId)
        {
            RequireProduct(productId);
            DatasetVersion left = FindVersion(productId, leftVersionId);
            DatasetVersion right = FindVersion(productId, rightVersionId);
            IList<DatasetVersionImage> leftImages = datasets.GetVersionImages(leftVersionId);
            IList<DatasetVersionImage> rightImages = datasets.GetVersionImages(rightVersionId);

            Dictionary<long, string> leftImageKeys = BuildVersionImageKeys(leftImages);
            Dictionary<long, string> rightImageKeys = BuildVersionImageKeys(rightImages);
            HashSet<string> leftImageSet = new HashSet<string>(leftImageKeys.Values, StringComparer.Ordinal);
            HashSet<string> rightImageSet = new HashSet<string>(rightImageKeys.Values, StringComparer.Ordinal);

            DatasetVersionComparison result = new DatasetVersionComparison
            {
                LeftVersionCode = left.VersionCode,
                RightVersionCode = right.VersionCode,
                AddedImages = CountExcept(rightImageSet, leftImageSet),
                RemovedImages = CountExcept(leftImageSet, rightImageSet)
            };

            HashSet<string> leftAnnotations = BuildAnnotationSignatures(datasets.GetVersionAnnotations(leftVersionId), leftImageKeys);
            HashSet<string> rightAnnotations = BuildAnnotationSignatures(datasets.GetVersionAnnotations(rightVersionId), rightImageKeys);
            result.AddedAnnotations = CountExcept(rightAnnotations, leftAnnotations);
            result.RemovedAnnotations = CountExcept(leftAnnotations, rightAnnotations);

            HashSet<string> leftMasks = BuildMaskSignatures(datasets.GetVersionMasks(leftVersionId), leftImageKeys);
            HashSet<string> rightMasks = BuildMaskSignatures(datasets.GetVersionMasks(rightVersionId), rightImageKeys);
            result.AddedMasks = CountExcept(rightMasks, leftMasks);
            result.RemovedMasks = CountExcept(leftMasks, rightMasks);

            Dictionary<string, DatasetVersionImage> leftByKey = IndexVersionImages(leftImages, leftImageKeys);
            Dictionary<string, DatasetVersionImage> rightByKey = IndexVersionImages(rightImages, rightImageKeys);
            foreach (KeyValuePair<string, DatasetVersionImage> pair in leftByKey)
            {
                DatasetVersionImage rightImage;
                if (!rightByKey.TryGetValue(pair.Key, out rightImage)) continue;
                if (!string.Equals(pair.Value.DatasetSplit, rightImage.DatasetSplit, StringComparison.OrdinalIgnoreCase)) result.SplitChanges++;
                if (!string.Equals(pair.Value.ReviewStatus, rightImage.ReviewStatus, StringComparison.OrdinalIgnoreCase)) result.ReviewChanges++;
            }
            return result;
        }

        private DatasetVersion FindVersion(long productId, long versionId)
        {
            foreach (DatasetVersion version in datasets.GetVersions(productId))
                if (version.Id == versionId) return version;
            throw new InvalidOperationException("数据集版本不存在或不属于当前产品。Id=" + versionId);
        }

        private static Dictionary<long, string> BuildVersionImageKeys(IList<DatasetVersionImage> images)
        {
            Dictionary<long, string> result = new Dictionary<long, string>();
            foreach (DatasetVersionImage image in images)
            {
                string key = !string.IsNullOrWhiteSpace(image.ContentHash)
                    ? "H|" + image.ContentHash.Trim().ToLowerInvariant()
                    : "F|" + (image.FileName ?? string.Empty).Trim().ToLowerInvariant() + "|" + image.Width + "x" + image.Height;
                result[image.SourceImageId] = key;
            }
            return result;
        }

        private static Dictionary<string, DatasetVersionImage> IndexVersionImages(
            IList<DatasetVersionImage> images,
            IDictionary<long, string> keys)
        {
            Dictionary<string, DatasetVersionImage> result = new Dictionary<string, DatasetVersionImage>(StringComparer.Ordinal);
            foreach (DatasetVersionImage image in images)
            {
                string key;
                if (keys.TryGetValue(image.SourceImageId, out key)) result[key] = image;
            }
            return result;
        }

        private static HashSet<string> BuildAnnotationSignatures(
            IList<DatasetVersionAnnotation> annotations,
            IDictionary<long, string> imageKeys)
        {
            HashSet<string> result = new HashSet<string>(StringComparer.Ordinal);
            foreach (DatasetVersionAnnotation annotation in annotations)
            {
                string imageKey;
                if (!imageKeys.TryGetValue(annotation.SourceImageId, out imageKey)) continue;
                result.Add(imageKey + "|" + CategoryKey(annotation.CategoryCode, annotation.CategoryName) + "|" +
                           annotation.AnnotationType + "|" + annotation.GeometryData + "|" + F(annotation.BrushWidth));
            }
            return result;
        }

        private static HashSet<string> BuildMaskSignatures(
            IList<DatasetVersionMask> versionMasks,
            IDictionary<long, string> imageKeys)
        {
            HashSet<string> result = new HashSet<string>(StringComparer.Ordinal);
            foreach (DatasetVersionMask mask in versionMasks)
            {
                string imageKey;
                if (!imageKeys.TryGetValue(mask.SourceImageId, out imageKey)) continue;
                result.Add(imageKey + "|" + CategoryKey(mask.CategoryCode, mask.CategoryName) + "|" + mask.PixelCount + "|" + mask.Revision);
            }
            return result;
        }

        private static int CountExcept(IEnumerable<string> source, ISet<string> other)
        {
            int count = 0;
            foreach (string value in source) if (!other.Contains(value)) count++;
            return count;
        }

        private DatasetExportResult Export(
            Product product,
            string versionCode,
            IList<ExportImage> images,
            IList<ExportAnnotation> annotations,
            IList<ExportMask> exportMasks,
            IList<DefectCategory> categoryItems,
            DatasetExportOptions options)
        {
            ValidateExportOptions(options);
            if (images.Count == 0) throw new InvalidOperationException("没有符合导出条件的图片。");

            string sourceName = string.IsNullOrWhiteSpace(versionCode) ? "working" : versionCode;
            string folderName = "IAD_" + MakeSafeName(product.ProductCode) + "_" + MakeSafeName(sourceName) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            string output = Path.Combine(options.DestinationDirectory, folderName);
            Directory.CreateDirectory(output);

            Dictionary<string, int> categoryIds = BuildCategoryMap(categoryItems, annotations, exportMasks);
            Dictionary<long, string> exportedNames = new Dictionary<long, string>();
            Dictionary<long, ExportImage> imageById = new Dictionary<long, ExportImage>();
            foreach (ExportImage image in images)
            {
                string split = NormalizeSplit(image.Split);
                string imageDirectory = Path.Combine(output, "images", SplitFolder(split));
                Directory.CreateDirectory(imageDirectory);
                string exportedName = image.Id.ToString("00000000", CultureInfo.InvariantCulture) + "_" + MakeSafeName(image.FileName);
                string sourcePath = ResolveWorkspacePath(image.RelativePath);
                if (!File.Exists(sourcePath)) throw new FileNotFoundException("数据集图片文件不存在。", sourcePath);
                File.Copy(sourcePath, Path.Combine(imageDirectory, exportedName), false);
                exportedNames[image.Id] = exportedName;
                imageById[image.Id] = image;
            }

            WriteClasses(output, categoryIds);
            if (options.ExportYolo) WriteYolo(output, images, annotations, categoryIds, exportedNames);
            if (options.ExportCoco) WriteCoco(output, product, versionCode, images, annotations, categoryIds, exportedNames);
            if (options.ExportMasks) WriteMasks(output, exportMasks, imageById, exportedNames);
            WriteManifest(output, product, versionCode, images, annotations, exportMasks, categoryIds, options);

            DatasetExportResult result = new DatasetExportResult
            {
                OutputDirectory = output,
                ImageCount = images.Count,
                AnnotationCount = annotations.Count,
                MaskCount = exportMasks.Count
            };
            foreach (ExportImage image in images)
                if (string.Equals(image.Split, DatasetSplit.Unassigned, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(image.Split))
                {
                    result.Warnings.Add("有未划分图片，导出时已归入 train。");
                    break;
                }
            if (options.ExportCoco && exportMasks.Count > 0)
                result.Warnings.Add("像素 Mask 以独立 PNG 导出；COCO JSON 当前记录矢量标注，未生成 Mask RLE。");
            return result;
        }

        private static void WriteYolo(string output, IList<ExportImage> images, IList<ExportAnnotation> annotations,
            IDictionary<string, int> categoryIds, IDictionary<long, string> exportedNames)
        {
            Dictionary<long, List<ExportAnnotation>> byImage = GroupAnnotations(annotations);
            foreach (ExportImage image in images)
            {
                string split = SplitFolder(NormalizeSplit(image.Split));
                string labelDirectory = Path.Combine(output, "labels", split);
                Directory.CreateDirectory(labelDirectory);
                string labelName = Path.GetFileNameWithoutExtension(exportedNames[image.Id]) + ".txt";
                StringBuilder text = new StringBuilder();
                List<ExportAnnotation> items;
                if (byImage.TryGetValue(image.Id, out items))
                {
                    foreach (ExportAnnotation annotation in items)
                    {
                        RectangleF box = AnnotationBounds(annotation);
                        int categoryId;
                        if (!categoryIds.TryGetValue(CategoryKey(annotation.CategoryCode, annotation.CategoryName), out categoryId)) continue;
                        double cx = (box.Left + box.Width / 2D) / image.Width;
                        double cy = (box.Top + box.Height / 2D) / image.Height;
                        double width = box.Width / image.Width;
                        double height = box.Height / image.Height;
                        text.Append(categoryId).Append(' ')
                            .Append(F(cx)).Append(' ').Append(F(cy)).Append(' ')
                            .Append(F(width)).Append(' ').Append(F(height)).AppendLine();
                    }
                }
                File.WriteAllText(Path.Combine(labelDirectory, labelName), text.ToString(), new UTF8Encoding(false));
            }

            WriteYoloYaml(output, categoryIds);

            string segmentationRoot = Path.Combine(output, "yolo-segmentation");
            Dictionary<long, List<ExportAnnotation>> segmentationByImage = GroupAnnotations(annotations);
            foreach (ExportImage image in images)
            {
                string split = SplitFolder(NormalizeSplit(image.Split));
                string imageDirectory = Path.Combine(segmentationRoot, "images", split);
                string labelDirectory = Path.Combine(segmentationRoot, "labels", split);
                Directory.CreateDirectory(imageDirectory);
                Directory.CreateDirectory(labelDirectory);
                string exportedName = exportedNames[image.Id];
                File.Copy(Path.Combine(output, "images", split, exportedName), Path.Combine(imageDirectory, exportedName), false);

                StringBuilder text = new StringBuilder();
                List<ExportAnnotation> items;
                if (segmentationByImage.TryGetValue(image.Id, out items))
                {
                    foreach (ExportAnnotation annotation in items)
                    {
                        int categoryId;
                        if (!categoryIds.TryGetValue(CategoryKey(annotation.CategoryCode, annotation.CategoryName), out categoryId)) continue;
                        IList<PointF> points;
                        if (string.Equals(annotation.AnnotationType, "Polygon", StringComparison.OrdinalIgnoreCase))
                        {
                            points = AnnotationGeometry.Parse(annotation.GeometryData);
                        }
                        else
                        {
                            RectangleF box = AnnotationBounds(annotation);
                            points = new[]
                            {
                                new PointF(box.Left, box.Top), new PointF(box.Right, box.Top),
                                new PointF(box.Right, box.Bottom), new PointF(box.Left, box.Bottom)
                            };
                        }
                        if (points.Count < 3) continue;
                        text.Append(categoryId);
                        foreach (PointF point in points)
                            text.Append(' ').Append(F(Math.Max(0D, Math.Min(1D, point.X / image.Width))))
                                .Append(' ').Append(F(Math.Max(0D, Math.Min(1D, point.Y / image.Height))));
                        text.AppendLine();
                    }
                }
                string labelName = Path.GetFileNameWithoutExtension(exportedName) + ".txt";
                File.WriteAllText(Path.Combine(labelDirectory, labelName), text.ToString(), new UTF8Encoding(false));
            }
            WriteClasses(segmentationRoot, categoryIds);
            WriteYoloYaml(segmentationRoot, categoryIds);
        }

        private static void WriteYoloYaml(string output, IDictionary<string, int> categoryIds)
        {
            StringBuilder yaml = new StringBuilder();
            yaml.AppendLine("path: .");
            yaml.AppendLine("train: images/train");
            yaml.AppendLine("val: images/val");
            yaml.AppendLine("test: images/test");
            yaml.AppendLine("names:");
            foreach (KeyValuePair<string, int> item in SortCategories(categoryIds))
                yaml.Append("  ").Append(item.Value).Append(": '").Append(YamlEscape(DisplayCategoryKey(item.Key))).AppendLine("'");
            File.WriteAllText(Path.Combine(output, "dataset.yaml"), yaml.ToString(), new UTF8Encoding(false));
        }

        private static void WriteCoco(string output, Product product, string versionCode, IList<ExportImage> images,
            IList<ExportAnnotation> annotations, IDictionary<string, int> categoryIds, IDictionary<long, string> exportedNames)
        {
            StringBuilder json = new StringBuilder();
            json.Append("{\n  \"info\": {\"description\": \"").Append(Json(product.ProductName))
                .Append("\", \"version\": \"").Append(Json(versionCode ?? "working")).Append("\"},\n");
            json.Append("  \"images\": [\n");
            for (int i = 0; i < images.Count; i++)
            {
                ExportImage image = images[i];
                json.Append("    {\"id\": ").Append(image.Id).Append(", \"file_name\": \"")
                    .Append(Json("images/" + SplitFolder(NormalizeSplit(image.Split)) + "/" + exportedNames[image.Id]))
                    .Append("\", \"width\": ").Append(image.Width).Append(", \"height\": ").Append(image.Height)
                    .Append(", \"split\": \"").Append(Json(NormalizeSplit(image.Split)))
                    .Append("\", \"review_status\": \"").Append(Json(image.ReviewStatus)).Append("\"}");
                json.AppendLine(i + 1 == images.Count ? string.Empty : ",");
            }
            json.Append("  ],\n  \"categories\": [\n");
            IList<KeyValuePair<string, int>> sorted = SortCategories(categoryIds);
            for (int i = 0; i < sorted.Count; i++)
            {
                json.Append("    {\"id\": ").Append(sorted[i].Value + 1).Append(", \"name\": \"")
                    .Append(Json(DisplayCategoryKey(sorted[i].Key))).Append("\"}");
                json.AppendLine(i + 1 == sorted.Count ? string.Empty : ",");
            }
            json.Append("  ],\n  \"annotations\": [\n");
            int written = 0;
            foreach (ExportAnnotation annotation in annotations)
            {
                ExportImage image;
                for (int i = 0; i < images.Count; i++)
                {
                    if (images[i].Id != annotation.ImageId) continue;
                    image = images[i];
                    goto FoundImage;
                }
                continue;
            FoundImage:
                int categoryId;
                if (!categoryIds.TryGetValue(CategoryKey(annotation.CategoryCode, annotation.CategoryName), out categoryId)) continue;
                if (written > 0) json.AppendLine(",");
                RectangleF box = AnnotationBounds(annotation);
                IList<PointF> points = AnnotationGeometry.Parse(annotation.GeometryData);
                json.Append("    {\"id\": ").Append(annotation.Id).Append(", \"image_id\": ").Append(annotation.ImageId)
                    .Append(", \"category_id\": ").Append(categoryId + 1)
                    .Append(", \"bbox\": [").Append(F(box.X)).Append(',').Append(F(box.Y)).Append(',').Append(F(box.Width)).Append(',').Append(F(box.Height)).Append(']')
                    .Append(", \"area\": ").Append(F(Math.Max(0D, AnnotationArea(annotation)))).Append(", \"iscrowd\": 0, \"segmentation\": ");
                if (string.Equals(annotation.AnnotationType, "Rectangle", StringComparison.OrdinalIgnoreCase) && points.Count >= 2)
                {
                    json.Append("[[").Append(F(box.Left)).Append(',').Append(F(box.Top)).Append(',')
                        .Append(F(box.Right)).Append(',').Append(F(box.Top)).Append(',')
                        .Append(F(box.Right)).Append(',').Append(F(box.Bottom)).Append(',')
                        .Append(F(box.Left)).Append(',').Append(F(box.Bottom)).Append("]]}");
                }
                else if (string.Equals(annotation.AnnotationType, "Polygon", StringComparison.OrdinalIgnoreCase) && points.Count >= 3)
                {
                    json.Append("[[");
                    for (int i = 0; i < points.Count; i++)
                    {
                        if (i > 0) json.Append(',');
                        json.Append(F(points[i].X)).Append(',').Append(F(points[i].Y));
                    }
                    json.Append("]]}");
                }
                else json.Append("[]}");
                written++;
            }
            json.AppendLine().Append("  ]\n}");
            string directory = Path.Combine(output, "annotations");
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "instances.json"), json.ToString(), new UTF8Encoding(false));
        }

        private static void WriteMasks(string output, IList<ExportMask> exportMasks,
            IDictionary<long, ExportImage> images, IDictionary<long, string> exportedNames)
        {
            foreach (ExportMask mask in exportMasks)
            {
                ExportImage image;
                if (!images.TryGetValue(mask.ImageId, out image)) continue;
                string category = MakeSafeName(string.IsNullOrWhiteSpace(mask.CategoryCode) ? mask.CategoryName : mask.CategoryCode);
                string directory = Path.Combine(output, "masks", SplitFolder(NormalizeSplit(image.Split)), category);
                Directory.CreateDirectory(directory);
                string source = ResolveWorkspacePath(mask.RelativePath);
                if (!File.Exists(source)) throw new FileNotFoundException("Mask PNG 不存在。", source);
                string targetName = Path.GetFileNameWithoutExtension(exportedNames[image.Id]) + ".png";
                File.Copy(source, Path.Combine(directory, targetName), false);
            }
        }

        private static void WriteClasses(string output, IDictionary<string, int> categoryIds)
        {
            StringBuilder classes = new StringBuilder();
            foreach (KeyValuePair<string, int> item in SortCategories(categoryIds))
                classes.Append(item.Value).Append('\t').AppendLine(DisplayCategoryKey(item.Key));
            File.WriteAllText(Path.Combine(output, "classes.txt"), classes.ToString(), new UTF8Encoding(false));
        }

        private static void WriteManifest(string output, Product product, string versionCode, IList<ExportImage> images,
            IList<ExportAnnotation> annotations, IList<ExportMask> exportMasks, IDictionary<string, int> categoryIds, DatasetExportOptions options)
        {
            int train = 0, validation = 0, test = 0;
            foreach (ExportImage image in images)
            {
                string split = NormalizeSplit(image.Split);
                if (split == DatasetSplit.Validation) validation++;
                else if (split == DatasetSplit.Test) test++;
                else train++;
            }
            StringBuilder text = new StringBuilder();
            text.AppendLine("IAD 数据集导出清单");
            text.AppendLine("产品：" + product.ProductCode + " · " + product.ProductName);
            text.AppendLine("版本：" + (versionCode ?? "当前工作集"));
            text.AppendLine("导出时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            text.AppendLine("图片：" + images.Count + "（train=" + train + ", val=" + validation + ", test=" + test + "）");
            text.AppendLine("矢量标注：" + annotations.Count);
            text.AppendLine("Mask：" + exportMasks.Count);
            text.AppendLine("类别：" + categoryIds.Count);
            text.AppendLine("格式：" + (options.ExportCoco ? "COCO " : string.Empty) + (options.ExportYolo ? "YOLO " : string.Empty) + (options.ExportMasks ? "Mask PNG" : string.Empty));
            File.WriteAllText(Path.Combine(output, "manifest.txt"), text.ToString(), new UTF8Encoding(false));
        }

        private static Dictionary<string, int> BuildCategoryMap(IList<DefectCategory> categoryItems,
            IList<ExportAnnotation> annotations, IList<ExportMask> exportMasks)
        {
            SortedDictionary<string, string> keys = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (DefectCategory category in categoryItems)
                if (category.IsEnabled) keys[CategoryKey(category.CategoryCode, category.CategoryName)] = string.Empty;
            foreach (ExportAnnotation annotation in annotations) keys[CategoryKey(annotation.CategoryCode, annotation.CategoryName)] = string.Empty;
            foreach (ExportMask mask in exportMasks) keys[CategoryKey(mask.CategoryCode, mask.CategoryName)] = string.Empty;
            Dictionary<string, int> result = new Dictionary<string, int>(StringComparer.Ordinal);
            int index = 0;
            foreach (string key in keys.Keys) result[key] = index++;
            return result;
        }

        private static Dictionary<long, List<ExportAnnotation>> GroupAnnotations(IList<ExportAnnotation> annotations)
        {
            Dictionary<long, List<ExportAnnotation>> result = new Dictionary<long, List<ExportAnnotation>>();
            foreach (ExportAnnotation annotation in annotations)
            {
                List<ExportAnnotation> items;
                if (!result.TryGetValue(annotation.ImageId, out items))
                {
                    items = new List<ExportAnnotation>();
                    result[annotation.ImageId] = items;
                }
                items.Add(annotation);
            }
            return result;
        }

        private static IList<KeyValuePair<string, int>> SortCategories(IDictionary<string, int> values)
        {
            List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>(values);
            result.Sort(delegate(KeyValuePair<string, int> left, KeyValuePair<string, int> right) { return left.Value.CompareTo(right.Value); });
            return result;
        }

        private static ExportImage ToExportImage(DatasetImage image)
        {
            return new ExportImage
            {
                Id = image.Id,
                FileName = image.FileName,
                RelativePath = image.RelativePath,
                Width = image.Width,
                Height = image.Height,
                Split = image.DatasetSplit,
                ReviewStatus = image.ReviewStatus
            };
        }

        private static ExportAnnotation ToExportAnnotation(DatasetAnnotation annotation)
        {
            return new ExportAnnotation
            {
                Id = annotation.Id,
                ImageId = annotation.DatasetImageId,
                CategoryCode = annotation.CategoryCode,
                CategoryName = annotation.CategoryName,
                AnnotationType = annotation.AnnotationType,
                GeometryData = annotation.GeometryData,
                BrushWidth = annotation.BrushWidth
            };
        }

        private static ExportMask ToExportMask(DatasetMask mask)
        {
            return new ExportMask
            {
                Id = mask.Id,
                ImageId = mask.DatasetImageId,
                CategoryCode = mask.CategoryCode,
                CategoryName = mask.CategoryName,
                RelativePath = mask.RelativePath,
                Width = mask.Width,
                Height = mask.Height,
                PixelCount = mask.PixelCount
            };
        }

        private static RectangleF AnnotationBounds(ExportAnnotation annotation)
        {
            IList<PointF> points = AnnotationGeometry.Parse(annotation.GeometryData);
            if (points.Count == 0) return RectangleF.Empty;
            float left = points[0].X, right = points[0].X, top = points[0].Y, bottom = points[0].Y;
            foreach (PointF point in points)
            {
                left = Math.Min(left, point.X); right = Math.Max(right, point.X);
                top = Math.Min(top, point.Y); bottom = Math.Max(bottom, point.Y);
            }
            if (string.Equals(annotation.AnnotationType, "Brush", StringComparison.OrdinalIgnoreCase))
            {
                float radius = Math.Max(0.5F, annotation.BrushWidth / 2F);
                left -= radius; right += radius; top -= radius; bottom += radius;
            }
            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        private static double AnnotationArea(ExportAnnotation annotation)
        {
            IList<PointF> points = AnnotationGeometry.Parse(annotation.GeometryData);
            DatasetAnnotation value = new DatasetAnnotation
            {
                AnnotationType = annotation.AnnotationType,
                BrushWidth = annotation.BrushWidth
            };
            return EstimateArea(value, points);
        }

        private static bool IsGeometryInside(DatasetAnnotation annotation, IList<PointF> points, int width, int height)
        {
            if (points.Count == 0) return false;
            float radius = string.Equals(annotation.AnnotationType, "Brush", StringComparison.OrdinalIgnoreCase)
                ? Math.Max(0.5F, annotation.BrushWidth / 2F)
                : 0F;
            foreach (PointF point in points)
                if (point.X - radius < 0 || point.Y - radius < 0 || point.X + radius > width || point.Y + radius > height) return false;
            return true;
        }

        private static double EstimateArea(DatasetAnnotation annotation, IList<PointF> points)
        {
            if (string.Equals(annotation.AnnotationType, "Rectangle", StringComparison.OrdinalIgnoreCase) && points.Count >= 2)
                return Math.Abs((points[1].X - points[0].X) * (points[1].Y - points[0].Y));
            if (string.Equals(annotation.AnnotationType, "Polygon", StringComparison.OrdinalIgnoreCase) && points.Count >= 3)
            {
                double area = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    PointF a = points[i];
                    PointF b = points[(i + 1) % points.Count];
                    area += a.X * b.Y - b.X * a.Y;
                }
                return Math.Abs(area) / 2D;
            }
            if (string.Equals(annotation.AnnotationType, "Brush", StringComparison.OrdinalIgnoreCase) && points.Count >= 2)
            {
                double length = 0;
                for (int i = 1; i < points.Count; i++)
                {
                    double dx = points[i].X - points[i - 1].X;
                    double dy = points[i].Y - points[i - 1].Y;
                    length += Math.Sqrt(dx * dx + dy * dy);
                }
                return length * annotation.BrushWidth;
            }
            return 0D;
        }

        private static int CountSeverity(DatasetImageQuality quality, string severity)
        {
            int count = 0;
            foreach (DatasetQualityIssue issue in quality.Issues)
                if (string.Equals(issue.Severity, severity, StringComparison.OrdinalIgnoreCase)) count++;
            return count;
        }

        private static void AddIssue(DatasetImageQuality result, string severity, string code, string message)
        {
            result.Issues.Add(new DatasetQualityIssue { Severity = severity, Code = code, Message = message });
        }

        private static string FirstIssue(DatasetImageQuality quality)
        {
            return quality.Issues.Count == 0 ? "未知质量问题" : quality.Issues[0].Message;
        }

        private static string DisplayReviewStatus(string status, bool hasLabels)
        {
            if (string.Equals(status, DatasetReviewStatus.Normal, StringComparison.OrdinalIgnoreCase)) return "正常样本";
            if (string.Equals(status, DatasetReviewStatus.Approved, StringComparison.OrdinalIgnoreCase)) return "已通过";
            if (string.Equals(status, DatasetReviewStatus.Rejected, StringComparison.OrdinalIgnoreCase)) return "已驳回";
            if (string.Equals(status, DatasetReviewStatus.Ignored, StringComparison.OrdinalIgnoreCase)) return "已忽略";
            return hasLabels ? "待审核" : "未标注";
        }

        private static bool ShouldExport(string reviewStatus, bool approvedOnly)
        {
            if (string.Equals(reviewStatus, DatasetReviewStatus.Ignored, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(reviewStatus, DatasetReviewStatus.Rejected, StringComparison.OrdinalIgnoreCase)) return false;
            if (!approvedOnly) return true;
            return string.Equals(reviewStatus, DatasetReviewStatus.Approved, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(reviewStatus, DatasetReviewStatus.Normal, StringComparison.OrdinalIgnoreCase);
        }

        private DefectCategory EnsureImportCategory(long productId, long externalId, string externalName)
        {
            string displayName = string.IsNullOrWhiteSpace(externalName)
                ? "导入类别 " + externalId.ToString(CultureInfo.InvariantCulture)
                : externalName.Trim().Trim('\'', '"');
            IList<DefectCategory> existing = categories.GetByProduct(productId);
            foreach (DefectCategory category in existing)
            {
                string combined = string.IsNullOrWhiteSpace(category.CategoryCode)
                    ? category.CategoryName
                    : category.CategoryCode + "_" + category.CategoryName;
                if (string.Equals(category.CategoryCode, displayName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(category.CategoryName, displayName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(combined, displayName, StringComparison.OrdinalIgnoreCase)) return category;
            }

            string code = null;
            string name = displayName;
            int separator = displayName.IndexOf('_');
            if (separator > 0 && separator + 1 < displayName.Length)
            {
                code = displayName.Substring(0, separator).Trim();
                name = displayName.Substring(separator + 1).Trim();
            }
            if (string.IsNullOrWhiteSpace(code))
            {
                long safeId = externalId == long.MinValue ? long.MaxValue : Math.Abs(externalId);
                code = "IMP-" + safeId.ToString("000", CultureInfo.InvariantCulture);
            }
            code = MakeSafeCategoryCode(code);

            string baseCode = code;
            int suffix = 2;
            bool conflict;
            do
            {
                conflict = false;
                foreach (DefectCategory category in existing)
                {
                    if (!string.Equals(category.CategoryCode, code, StringComparison.OrdinalIgnoreCase)) continue;
                    conflict = true;
                    code = baseCode + "-" + suffix++;
                    break;
                }
            } while (conflict);

            DateTime now = DateTime.UtcNow;
            DefectCategory created = new DefectCategory
            {
                ProductId = productId,
                CategoryCode = code,
                CategoryName = string.IsNullOrWhiteSpace(name) ? displayName : name,
                DefectType = "Imported",
                DetectionStrategy = "DatasetImport",
                DefaultThreshold = 0.5D,
                MinArea = 0D,
                MinLength = 0D,
                DisplayOrder = existing.Count + 1,
                IsEnabled = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            created.Id = categories.Insert(created);
            return created;
        }

        private static string MakeSafeCategoryCode(string value)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in value ?? string.Empty)
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_') result.Append(c);
                else if (!char.IsWhiteSpace(c)) result.Append('-');
            }
            return result.Length == 0 ? "IMPORTED" : result.ToString();
        }

        private Dictionary<int, DefectCategory> LoadYoloCategories(long productId, string root)
        {
            SortedDictionary<int, string> names = new SortedDictionary<int, string>();
            string classesPath = Path.Combine(root, "classes.txt");
            if (File.Exists(classesPath))
            {
                int sequential = 0;
                foreach (string raw in File.ReadAllLines(classesPath, Encoding.UTF8))
                {
                    string line = raw == null ? string.Empty : raw.Trim();
                    if (line.Length == 0) continue;
                    int index = sequential;
                    string name = line;
                    int tab = line.IndexOf('\t');
                    if (tab > 0)
                    {
                        int parsed;
                        if (int.TryParse(line.Substring(0, tab).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed)) index = parsed;
                        name = line.Substring(tab + 1).Trim();
                    }
                    names[index] = name;
                    sequential = Math.Max(sequential + 1, index + 1);
                }
            }
            if (names.Count == 0)
            {
                string yamlPath = Path.Combine(root, "dataset.yaml");
                if (File.Exists(yamlPath))
                {
                    bool inNames = false;
                    foreach (string raw in File.ReadAllLines(yamlPath, Encoding.UTF8))
                    {
                        string line = raw == null ? string.Empty : raw.Trim();
                        if (string.Equals(line, "names:", StringComparison.OrdinalIgnoreCase)) { inNames = true; continue; }
                        if (!inNames || line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
                        int colon = line.IndexOf(':');
                        int index;
                        if (colon <= 0 || !int.TryParse(line.Substring(0, colon).Trim(), out index)) continue;
                        names[index] = line.Substring(colon + 1).Trim().Trim('\'', '"');
                    }
                }
            }
            Dictionary<int, DefectCategory> result = new Dictionary<int, DefectCategory>();
            foreach (KeyValuePair<int, string> pair in names)
                result[pair.Key] = EnsureImportCategory(productId, pair.Key, pair.Value);
            return result;
        }

        private static bool TryReadCocoGeometry(
            IDictionary<string, object> value,
            DatasetImage image,
            out string annotationType,
            out string geometry)
        {
            annotationType = null;
            geometry = null;
            object segmentationValue;
            IList<object> segments;
            if (value.TryGetValue("segmentation", out segmentationValue) && TryAsArray(segmentationValue, out segments))
            {
                foreach (object segment in segments)
                {
                    IList<object> coordinates;
                    if (!TryAsArray(segment, out coordinates) || coordinates.Count < 6 || coordinates.Count % 2 != 0) continue;
                    List<PointF> points = new List<PointF>();
                    for (int i = 0; i < coordinates.Count; i += 2)
                    {
                        double x;
                        double y;
                        if (!TryNumber(coordinates[i], out x) || !TryNumber(coordinates[i + 1], out y)) { points.Clear(); break; }
                        points.Add(new PointF(ClampCoordinate(x, image.Width), ClampCoordinate(y, image.Height)));
                    }
                    if (points.Count < 3) continue;
                    annotationType = "Polygon";
                    geometry = AnnotationGeometry.Serialize(points);
                    return true;
                }
            }

            object bboxValue;
            IList<object> bbox;
            if (!value.TryGetValue("bbox", out bboxValue) || !TryAsArray(bboxValue, out bbox) || bbox.Count < 4) return false;
            double left;
            double top;
            double width;
            double height;
            if (!TryNumber(bbox[0], out left) || !TryNumber(bbox[1], out top) ||
                !TryNumber(bbox[2], out width) || !TryNumber(bbox[3], out height) || width <= 0 || height <= 0) return false;
            float x1 = ClampCoordinate(left, image.Width);
            float y1 = ClampCoordinate(top, image.Height);
            float x2 = ClampCoordinate(left + width, image.Width);
            float y2 = ClampCoordinate(top + height, image.Height);
            if (x2 - x1 < 1F || y2 - y1 < 1F) return false;
            annotationType = "Rectangle";
            geometry = AnnotationGeometry.Serialize(new[] { new PointF(x1, y1), new PointF(x2, y2) });
            return true;
        }

        private static bool TryReadYoloGeometry(
            string[] tokens,
            DatasetImage image,
            out string annotationType,
            out string geometry)
        {
            annotationType = null;
            geometry = null;
            if (tokens.Length >= 7 && tokens.Length % 2 == 1)
            {
                List<PointF> points = new List<PointF>();
                for (int i = 1; i + 1 < tokens.Length; i += 2)
                {
                    double x;
                    double y;
                    if (!double.TryParse(tokens[i], NumberStyles.Float, CultureInfo.InvariantCulture, out x) ||
                        !double.TryParse(tokens[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out y)) return false;
                    points.Add(new PointF(ClampCoordinate(x * image.Width, image.Width), ClampCoordinate(y * image.Height, image.Height)));
                }
                if (points.Count < 3) return false;
                annotationType = "Polygon";
                geometry = AnnotationGeometry.Serialize(points);
                return true;
            }
            if (tokens.Length != 5) return false;
            double cx;
            double cy;
            double width;
            double height;
            if (!double.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out cx) ||
                !double.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out cy) ||
                !double.TryParse(tokens[3], NumberStyles.Float, CultureInfo.InvariantCulture, out width) ||
                !double.TryParse(tokens[4], NumberStyles.Float, CultureInfo.InvariantCulture, out height) || width <= 0 || height <= 0) return false;
            float x1 = ClampCoordinate((cx - width / 2D) * image.Width, image.Width);
            float y1 = ClampCoordinate((cy - height / 2D) * image.Height, image.Height);
            float x2 = ClampCoordinate((cx + width / 2D) * image.Width, image.Width);
            float y2 = ClampCoordinate((cy + height / 2D) * image.Height, image.Height);
            if (x2 - x1 < 1F || y2 - y1 < 1F) return false;
            annotationType = "Rectangle";
            geometry = AnnotationGeometry.Serialize(new[] { new PointF(x1, y1), new PointF(x2, y2) });
            return true;
        }

        private bool AddImportedAnnotation(DatasetImage image, DefectCategory category, string annotationType, string geometry)
        {
            string normalized = AnnotationGeometry.ValidateAndNormalize(annotationType, geometry, image.Width, image.Height);
            foreach (DatasetAnnotation existing in datasets.GetAnnotationsByImage(image.Id))
            {
                if (existing.CategoryId == category.Id &&
                    string.Equals(existing.AnnotationType, annotationType, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(existing.GeometryData, normalized, StringComparison.Ordinal)) return false;
            }
            datasetService.CreateAnnotation(image.Id, category.Id, annotationType, normalized, 1F, 1D);
            return true;
        }

        private void SetImportedSplit(long imageId, string split)
        {
            DatasetImage image = RequireImage(imageId);
            image.DatasetSplit = split;
            image.UpdatedAtUtc = DateTime.UtcNow;
            datasets.UpdateImageWorkflow(image);
        }

        private void ApplyImportedReviews(long productId, IDictionary<long, string> desiredReviews, DatasetImportResult result)
        {
            foreach (KeyValuePair<long, string> pair in desiredReviews)
            {
                if (string.Equals(pair.Value, DatasetReviewStatus.Pending, StringComparison.OrdinalIgnoreCase)) continue;
                try { SetReviewStatus(productId, new[] { pair.Key }, pair.Value, "由训练数据导入", Environment.UserName); }
                catch (Exception ex) { result.Warnings.Add("审核状态未自动恢复：" + ex.Message); }
            }
        }

        private void ImportMaskFolder(long productId, string masksRoot, DatasetImportResult result)
        {
            if (!Directory.Exists(masksRoot)) return;
            Dictionary<string, DatasetImage> imagesByBaseName = new Dictionary<string, DatasetImage>(StringComparer.OrdinalIgnoreCase);
            foreach (DatasetImage image in datasets.GetImagesByProduct(productId))
            {
                string key = Path.GetFileNameWithoutExtension(image.FileName ?? string.Empty);
                if (!imagesByBaseName.ContainsKey(key)) imagesByBaseName[key] = image;
            }
            foreach (string maskPath in Directory.GetFiles(masksRoot, "*.png", SearchOption.AllDirectories))
            {
                string imageKey = Path.GetFileNameWithoutExtension(maskPath);
                DatasetImage image;
                if (!imagesByBaseName.TryGetValue(imageKey, out image))
                {
                    result.Warnings.Add("找不到 Mask 对应的图片：" + Path.GetFileName(maskPath));
                    continue;
                }
                string categoryName = new DirectoryInfo(Path.GetDirectoryName(maskPath)).Name;
                DefectCategory category = EnsureImportCategory(productId, StableExternalId(categoryName), categoryName);
                try
                {
                    using (Bitmap bitmap = new Bitmap(maskPath))
                        maskService.SaveMask(image.Id, category.Id, bitmap);
                    result.MaskCount++;
                }
                catch (Exception ex)
                {
                    result.Warnings.Add("Mask 导入失败：" + Path.GetFileName(maskPath) + " — " + ex.Message);
                }
            }
        }

        private static long StableExternalId(string value)
        {
            unchecked
            {
                long hash = 17;
                foreach (char c in value ?? string.Empty) hash = hash * 31 + char.ToUpperInvariant(c);
                return hash == long.MinValue ? long.MaxValue : Math.Abs(hash);
            }
        }

        private static List<string> CollectTrainingImages(string root)
        {
            HashSet<string> extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff" };
            List<string> result = new List<string>();
            foreach (string file in Directory.GetFiles(root, "*.*", SearchOption.AllDirectories))
                if (extensions.Contains(Path.GetExtension(file))) result.Add(file);
            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        private static string MakeRelativePath(string root, string path)
        {
            string normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string normalizedPath = Path.GetFullPath(path);
            if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("文件不在数据集图片目录中：" + path);
            return normalizedPath.Substring(normalizedRoot.Length);
        }

        private static string ResolveImportedImagePath(string annotationDirectory, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new InvalidDataException("COCO image.file_name 不能为空。");
            string normalized = fileName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            if (Path.IsPathRooted(normalized) && File.Exists(normalized)) return Path.GetFullPath(normalized);
            string direct = Path.GetFullPath(Path.Combine(annotationDirectory, normalized));
            if (File.Exists(direct)) return direct;
            DirectoryInfo parent = Directory.GetParent(annotationDirectory);
            if (parent != null)
            {
                string fromParent = Path.GetFullPath(Path.Combine(parent.FullName, normalized));
                if (File.Exists(fromParent)) return fromParent;
            }
            throw new FileNotFoundException("COCO 图片文件不存在。", normalized);
        }

        private static string NormalizeImportedSplit(string explicitSplit, string path)
        {
            string value = (explicitSplit ?? string.Empty).Trim();
            if (string.Equals(value, "val", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "validation", StringComparison.OrdinalIgnoreCase)) return DatasetSplit.Validation;
            if (string.Equals(value, "test", StringComparison.OrdinalIgnoreCase)) return DatasetSplit.Test;
            if (string.Equals(value, "train", StringComparison.OrdinalIgnoreCase)) return DatasetSplit.Train;
            string normalized = (path ?? string.Empty).Replace('\\', '/').ToLowerInvariant();
            if (normalized.Contains("/val/") || normalized.StartsWith("val/")) return DatasetSplit.Validation;
            if (normalized.Contains("/validation/") || normalized.StartsWith("validation/")) return DatasetSplit.Validation;
            if (normalized.Contains("/test/") || normalized.StartsWith("test/")) return DatasetSplit.Test;
            if (normalized.Contains("/train/") || normalized.StartsWith("train/")) return DatasetSplit.Train;
            return DatasetSplit.Unassigned;
        }

        private static float ClampCoordinate(double value, int maximum)
        {
            return (float)Math.Max(0D, Math.Min(maximum, value));
        }

        private static IDictionary<string, object> AsObject(object value, string description)
        {
            IDictionary<string, object> result = value as IDictionary<string, object>;
            if (result == null) throw new InvalidDataException(description + "不是 JSON 对象。");
            return result;
        }

        private static IList<object> GetArray(IDictionary<string, object> value, string key)
        {
            object raw;
            IList<object> result;
            if (!value.TryGetValue(key, out raw) || !TryAsArray(raw, out result))
                throw new InvalidDataException("COCO 缺少数组字段：" + key);
            return result;
        }

        private static bool TryAsArray(object value, out IList<object> result)
        {
            object[] array = value as object[];
            if (array != null) { result = array; return true; }
            ArrayList list = value as ArrayList;
            if (list != null) { result = list.ToArray(); return true; }
            IEnumerable enumerable = value as IEnumerable;
            if (enumerable != null && !(value is string) && !(value is IDictionary))
            {
                List<object> items = new List<object>();
                foreach (object item in enumerable) items.Add(item);
                result = items;
                return true;
            }
            result = null;
            return false;
        }

        private static string GetString(IDictionary<string, object> value, string key)
        {
            object raw;
            return value.TryGetValue(key, out raw) && raw != null ? Convert.ToString(raw, CultureInfo.InvariantCulture) : null;
        }

        private static long GetInt64(IDictionary<string, object> value, string key, bool required)
        {
            object raw;
            double number;
            if (value.TryGetValue(key, out raw) && TryNumber(raw, out number)) return Convert.ToInt64(number);
            if (required) throw new InvalidDataException("JSON 字段不是有效整数：" + key);
            return 0;
        }

        private static bool TryNumber(object value, out double result)
        {
            if (value == null) { result = 0D; return false; }
            try
            {
                result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return !double.IsNaN(result) && !double.IsInfinity(result);
            }
            catch
            {
                result = 0D;
                return false;
            }
        }

        private static bool IsPublicationReadyStatus(string reviewStatus)
        {
            return string.Equals(reviewStatus, DatasetReviewStatus.Approved, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(reviewStatus, DatasetReviewStatus.Normal, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(reviewStatus, DatasetReviewStatus.Ignored, StringComparison.OrdinalIgnoreCase);
        }

        private static void ValidateVersionExport(
            IList<ExportImage> images,
            IList<ExportAnnotation> annotations,
            IList<ExportMask> exportMasks)
        {
            Dictionary<long, int> labelCounts = new Dictionary<long, int>();
            foreach (ExportAnnotation annotation in annotations)
            {
                int count;
                labelCounts.TryGetValue(annotation.ImageId, out count);
                labelCounts[annotation.ImageId] = count + 1;
            }
            foreach (ExportMask mask in exportMasks)
            {
                int count;
                labelCounts.TryGetValue(mask.ImageId, out count);
                labelCounts[mask.ImageId] = count + 1;
                if (mask.PixelCount <= 0)
                    throw new InvalidOperationException("历史版本中存在空 Mask，不能通过质量门禁。");
                string maskPath = ResolveWorkspacePath(mask.RelativePath);
                if (!File.Exists(maskPath))
                    throw new FileNotFoundException("历史版本的 Mask PNG 不存在。", maskPath);
            }
            foreach (ExportImage image in images)
            {
                if (!IsPublicationReadyStatus(image.ReviewStatus))
                    throw new InvalidOperationException("历史版本包含未完成审核的图片：“" + image.FileName + "”。");
                int count;
                labelCounts.TryGetValue(image.Id, out count);
                bool normal = string.Equals(image.ReviewStatus, DatasetReviewStatus.Normal, StringComparison.OrdinalIgnoreCase);
                if (normal && count > 0)
                    throw new InvalidOperationException("历史版本中的正常样本仍包含缺陷标注：“" + image.FileName + "”。");
                if (!normal && count == 0)
                    throw new InvalidOperationException("历史版本中的已通过图片没有任何标注：“" + image.FileName + "”。");
            }
        }

        private Product RequireProduct(long productId)
        {
            Product product = products.GetById(productId);
            if (product == null) throw new InvalidOperationException("产品不存在。Id=" + productId);
            return product;
        }

        private DatasetImage RequireImage(long imageId)
        {
            DatasetImage image = datasets.GetImageById(imageId);
            if (image == null) throw new InvalidOperationException("数据集图片不存在。Id=" + imageId);
            if (string.IsNullOrWhiteSpace(image.ReviewStatus)) image.ReviewStatus = DatasetReviewStatus.Pending;
            if (string.IsNullOrWhiteSpace(image.DatasetSplit)) image.DatasetSplit = DatasetSplit.Unassigned;
            return image;
        }

        private static string StableSortKey(DatasetImage image, int seed)
        {
            string value = seed.ToString(CultureInfo.InvariantCulture) + "|" + (image.ContentHash ?? image.FileName ?? image.Id.ToString(CultureInfo.InvariantCulture));
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", string.Empty);
        }

        private static string ResolveWorkspacePath(string relativePath)
        {
            string root = Path.GetFullPath(ProjectStoragePaths.RootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string fullPath = Path.GetFullPath(Path.Combine(ProjectStoragePaths.RootPath, relativePath ?? string.Empty));
            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("路径超出当前 Workspace。");
            return fullPath;
        }

        private static void ValidateExportOptions(DatasetExportOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (string.IsNullOrWhiteSpace(options.DestinationDirectory)) throw new ArgumentException("请选择导出目录。");
            if (!options.ExportCoco && !options.ExportYolo && !options.ExportMasks) throw new ArgumentException("请至少选择一种导出格式。");
            Directory.CreateDirectory(options.DestinationDirectory);
        }

        private static string CategoryKey(string code, string name)
        {
            return (code ?? string.Empty).Trim() + "|" + (name ?? "未命名类别").Trim();
        }

        private static string DisplayCategoryKey(string key)
        {
            int separator = key.IndexOf('|');
            if (separator < 0) return key;
            string code = key.Substring(0, separator);
            string name = key.Substring(separator + 1);
            return string.IsNullOrWhiteSpace(code) ? name : code + "_" + name;
        }

        private static string NormalizeSplit(string split)
        {
            if (string.Equals(split, DatasetSplit.Validation, StringComparison.OrdinalIgnoreCase)) return DatasetSplit.Validation;
            if (string.Equals(split, DatasetSplit.Test, StringComparison.OrdinalIgnoreCase)) return DatasetSplit.Test;
            return DatasetSplit.Train;
        }

        private static string SplitFolder(string split)
        {
            if (string.Equals(split, DatasetSplit.Validation, StringComparison.OrdinalIgnoreCase)) return "val";
            if (string.Equals(split, DatasetSplit.Test, StringComparison.OrdinalIgnoreCase)) return "test";
            return "train";
        }

        private static string MakeSafeName(string value)
        {
            string result = string.IsNullOrWhiteSpace(value) ? "item" : value.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars()) result = result.Replace(invalid, '_');
            return result;
        }

        private static string F(double value)
        {
            return value.ToString("0.######", CultureInfo.InvariantCulture);
        }

        private static string Json(string value)
        {
            if (value == null) return string.Empty;
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private static string YamlEscape(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        private sealed class ExportImage
        {
            public long Id;
            public string FileName;
            public string RelativePath;
            public int Width;
            public int Height;
            public string Split;
            public string ReviewStatus;
        }

        private sealed class ExportAnnotation
        {
            public long Id;
            public long ImageId;
            public string CategoryCode;
            public string CategoryName;
            public string AnnotationType;
            public string GeometryData;
            public float BrushWidth;
        }

        private sealed class ExportMask
        {
            public long Id;
            public long ImageId;
            public string CategoryCode;
            public string CategoryName;
            public string RelativePath;
            public int Width;
            public int Height;
            public long PixelCount;
        }
    }
}
