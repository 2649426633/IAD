using System;

namespace IAD.Models
{
    public sealed class DatasetAnnotation
    {
        public long Id { get; set; }
        public long DatasetImageId { get; set; }
        public long? CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string AnnotationType { get; set; }
        public string GeometryData { get; set; }
        public float BrushWidth { get; set; }
        public double Confidence { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
