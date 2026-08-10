using System;
using System.Collections.Generic;

namespace IAD.Models
{
    public static class DatasetReviewStatus
    {
        public const string Pending = "Pending";
        public const string Normal = "Normal";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Ignored = "Ignored";

        public static bool IsValid(string value)
        {
            return string.Equals(value, Pending, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, Normal, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, Approved, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, Rejected, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, Ignored, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static class DatasetSplit
    {
        public const string Unassigned = "Unassigned";
        public const string Train = "Train";
        public const string Validation = "Validation";
        public const string Test = "Test";

        public static bool IsValid(string value)
        {
            return string.Equals(value, Unassigned, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, Train, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, Validation, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, Test, StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class DatasetImageImportResult
    {
        public DatasetImage Image { get; set; }
        public bool IsDuplicate { get; set; }
    }

    public sealed class DatasetQualityIssue
    {
        public string Severity { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }

    public sealed class DatasetImageQuality
    {
        public long ImageId { get; set; }
        public string FileName { get; set; }
        public int VectorAnnotationCount { get; set; }
        public int MaskCount { get; set; }
        public double BoundaryScore { get; set; }
        public double QualityScore { get; set; }
        public bool CanApprove { get; set; }
        public IList<DatasetQualityIssue> Issues { get; set; }

        public DatasetImageQuality()
        {
            Issues = new List<DatasetQualityIssue>();
        }
    }

    public sealed class DatasetQualityReport
    {
        public int ImageCount { get; set; }
        public int PassedCount { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public IList<DatasetImageQuality> Images { get; set; }

        public DatasetQualityReport()
        {
            Images = new List<DatasetImageQuality>();
        }
    }

    public sealed class DatasetVersionImage
    {
        public long VersionId { get; set; }
        public long SourceImageId { get; set; }
        public string FileName { get; set; }
        public string RelativePath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Status { get; set; }
        public string ReviewStatus { get; set; }
        public string DatasetSplit { get; set; }
        public string ContentHash { get; set; }
        public string ReviewComment { get; set; }
        public string ReviewedBy { get; set; }
        public DateTime? ReviewedAtUtc { get; set; }
        public string ProductDefinitionVersion { get; set; }
    }

    public sealed class DatasetImportResult
    {
        public int ImageCount { get; set; }
        public int DuplicateImageCount { get; set; }
        public int AnnotationCount { get; set; }
        public int MaskCount { get; set; }
        public IList<string> Warnings { get; set; }

        public DatasetImportResult()
        {
            Warnings = new List<string>();
        }
    }

    public sealed class DatasetVersionComparison
    {
        public string LeftVersionCode { get; set; }
        public string RightVersionCode { get; set; }
        public int AddedImages { get; set; }
        public int RemovedImages { get; set; }
        public int AddedAnnotations { get; set; }
        public int RemovedAnnotations { get; set; }
        public int AddedMasks { get; set; }
        public int RemovedMasks { get; set; }
        public int SplitChanges { get; set; }
        public int ReviewChanges { get; set; }
    }

    public sealed class DatasetVersionAnnotation
    {
        public long VersionId { get; set; }
        public long SourceAnnotationId { get; set; }
        public long SourceImageId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string AnnotationType { get; set; }
        public string GeometryData { get; set; }
        public float BrushWidth { get; set; }
        public double Confidence { get; set; }
        public bool IsVisible { get; set; }
    }

    public sealed class DatasetVersionMask
    {
        public long VersionId { get; set; }
        public long SourceMaskId { get; set; }
        public long SourceImageId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string RelativePath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Revision { get; set; }
        public long PixelCount { get; set; }
        public bool IsVisible { get; set; }
    }

    public sealed class DatasetExportOptions
    {
        public string DestinationDirectory { get; set; }
        public bool ExportCoco { get; set; }
        public bool ExportYolo { get; set; }
        public bool ExportMasks { get; set; }
        public bool ApprovedOnly { get; set; }
        public bool RequireQualityGate { get; set; }
    }

    public sealed class DatasetExportResult
    {
        public string OutputDirectory { get; set; }
        public int ImageCount { get; set; }
        public int AnnotationCount { get; set; }
        public int MaskCount { get; set; }
        public IList<string> Warnings { get; set; }

        public DatasetExportResult()
        {
            Warnings = new List<string>();
        }
    }
}
