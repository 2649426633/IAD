using System;

namespace IAD.Models
{
    public sealed class DefectRecognitionSettings
    {
        public long ProductId { get; set; }
        public long CategoryId { get; set; }
        public double SimilarityThreshold { get; set; }
        public int TopK { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public sealed class DefectRecognitionCandidate
    {
        public long Id { get; set; }
        public string RunCode { get; set; }
        public long ProductId { get; set; }
        public long CategoryId { get; set; }
        public long DatasetImageId { get; set; }
        public string SourceFileName { get; set; }
        public double Similarity { get; set; }
        public string GeometryData { get; set; }
        public string Status { get; set; }
        public long? ConfirmedAnnotationId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public sealed class DefectHardNegative
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long CategoryId { get; set; }
        public long DatasetImageId { get; set; }
        public string SourceFileName { get; set; }
        public string GeometryData { get; set; }
        public double Similarity { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public sealed class DefectPrototypeSample
    {
        public DatasetImage Image { get; set; }
        public DatasetAnnotation Annotation { get; set; }
    }

    public sealed class DefectRecognitionSummary
    {
        public int ConfirmedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingCount { get; set; }
        public int HardNegativeCount { get; set; }
    }
}
