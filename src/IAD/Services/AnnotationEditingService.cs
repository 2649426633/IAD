using System;
using IAD.Models;
using IAD.Repositories;

namespace IAD.Services
{
    public sealed class AnnotationEditingService
    {
        private readonly IDatasetRepository datasets;
        private readonly IDefectCategoryRepository categories;

        internal AnnotationEditingService(IDatasetRepository datasets, IDefectCategoryRepository categories)
        {
            this.datasets = datasets ?? throw new ArgumentNullException("datasets");
            this.categories = categories ?? throw new ArgumentNullException("categories");
        }

        public DatasetAnnotation Update(DatasetAnnotation annotation)
        {
            if (annotation == null) throw new ArgumentNullException("annotation");
            if (annotation.Id <= 0) throw new ArgumentException("标注 Id 无效。", "annotation");

            DatasetImage image = datasets.GetImageById(annotation.DatasetImageId);
            if (image == null)
                throw new InvalidOperationException("标注所属图片不存在。Id=" + annotation.DatasetImageId);

            ApplyCategorySnapshot(annotation, image.ProductId);
            ValidateAnnotation(annotation, image);
            annotation.UpdatedAtUtc = DateTime.UtcNow;
            datasets.UpdateAnnotation(annotation);
            datasets.UpdateImageStatus(image.Id, "待审核", annotation.UpdatedAtUtc);
            return annotation;
        }

        public DatasetAnnotation Recreate(DatasetAnnotation snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");

            DatasetImage image = datasets.GetImageById(snapshot.DatasetImageId);
            if (image == null)
                throw new InvalidOperationException("标注所属图片不存在。Id=" + snapshot.DatasetImageId);

            DatasetAnnotation annotation = Clone(snapshot);
            annotation.Id = 0;
            ApplyCategorySnapshot(annotation, image.ProductId);
            ValidateAnnotation(annotation, image);
            DateTime now = DateTime.UtcNow;
            annotation.CreatedAtUtc = now;
            annotation.UpdatedAtUtc = now;
            annotation.Id = datasets.InsertAnnotation(annotation);
            datasets.UpdateImageStatus(image.Id, "已标注", now);
            return annotation;
        }

        private void ApplyCategorySnapshot(DatasetAnnotation annotation, long productId)
        {
            if (!annotation.CategoryId.HasValue) return;

            DefectCategory category = categories.GetById(annotation.CategoryId.Value);
            if (category == null || category.ProductId != productId)
                throw new InvalidOperationException("标注关联的瑕疵类别不存在或不属于当前产品。Id=" + annotation.CategoryId.Value);

            annotation.CategoryCode = category.CategoryCode;
            annotation.CategoryName = category.CategoryName;
        }

        private static void ValidateAnnotation(DatasetAnnotation annotation, DatasetImage image)
        {
            if (string.IsNullOrWhiteSpace(annotation.AnnotationType))
                throw new ArgumentException("标注类型不能为空。", "annotation");
            if (annotation.BrushWidth < 1 || float.IsNaN(annotation.BrushWidth) || float.IsInfinity(annotation.BrushWidth))
                throw new ArgumentException("画笔宽度必须大于等于 1。", "annotation");
            if (double.IsNaN(annotation.Confidence) || double.IsInfinity(annotation.Confidence) ||
                annotation.Confidence < 0 || annotation.Confidence > 1)
                throw new ArgumentException("置信度必须在 0 到 1 之间。", "annotation");

            annotation.AnnotationType = annotation.AnnotationType.Trim();
            annotation.GeometryData = AnnotationGeometry.ValidateAndNormalize(
                annotation.AnnotationType,
                annotation.GeometryData,
                image.Width,
                image.Height);
        }

        private static DatasetAnnotation Clone(DatasetAnnotation source)
        {
            return new DatasetAnnotation
            {
                Id = source.Id,
                DatasetImageId = source.DatasetImageId,
                CategoryId = source.CategoryId,
                CategoryCode = source.CategoryCode,
                CategoryName = source.CategoryName,
                AnnotationType = source.AnnotationType,
                GeometryData = source.GeometryData,
                BrushWidth = source.BrushWidth,
                Confidence = source.Confidence,
                IsVisible = source.IsVisible,
                CreatedAtUtc = source.CreatedAtUtc,
                UpdatedAtUtc = source.UpdatedAtUtc
            };
        }
    }
}
