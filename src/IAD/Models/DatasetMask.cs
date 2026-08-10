using System;

namespace IAD.Models
{
    public sealed class DatasetMask
    {
        public long Id { get; set; }
        public long DatasetImageId { get; set; }
        public long? CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string RelativePath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Revision { get; set; }
        public long PixelCount { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
