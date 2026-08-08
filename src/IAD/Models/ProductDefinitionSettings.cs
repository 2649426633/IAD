using System;

namespace IAD.Models
{
    public sealed class ProductDefinitionSettings
    {
        public long ProductId { get; set; }
        public string ImageSize { get; set; }
        public int ProductCount { get; set; }
        public string Pose { get; set; }
        public string AcquisitionCondition { get; set; }
        public string ReferenceImagePath { get; set; }
        public string TemplateType { get; set; }
        public string LocalizationMethod { get; set; }
        public string ModelType { get; set; }
        public double MinScore { get; set; }
        public string AngleRange { get; set; }
        public string ScaleRange { get; set; }
        public int MatchCount { get; set; }
        public double PixelX { get; set; }
        public double PixelY { get; set; }
        public string LengthUnit { get; set; }
        public string AreaUnit { get; set; }
        public string CalibrationVersion { get; set; }
        public string CalibrationState { get; set; }
        public string ProductDefinitionVersion { get; set; }
        public string TemplateVersion { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
