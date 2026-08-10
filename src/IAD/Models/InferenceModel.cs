using System;

namespace IAD.Models
{
    public sealed class InferenceModel
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ModelCode { get; set; }
        public string ModelName { get; set; }
        public string Version { get; set; }
        public string ModelType { get; set; }
        public string RelativePath { get; set; }
        public string Sha256 { get; set; }
        public string InputName { get; set; }
        public string OutputName { get; set; }
        public int InputWidth { get; set; }
        public int InputHeight { get; set; }
        public string Labels { get; set; }
        public double ConfidenceThreshold { get; set; }
        public double NmsThreshold { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
