using System;

namespace IAD.Models
{
    public sealed class DatasetVersion
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string VersionCode { get; set; }
        public string ProductDefinitionVersion { get; set; }
        public int ImageCount { get; set; }
        public int AnnotationCount { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
