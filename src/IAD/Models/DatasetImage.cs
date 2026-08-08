using System;

namespace IAD.Models
{
    public sealed class DatasetImage
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string FileName { get; set; }
        public string RelativePath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Status { get; set; }
        public string ProductDefinitionVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
