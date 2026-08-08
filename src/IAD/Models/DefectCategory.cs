using System;

namespace IAD.Models
{
    public sealed class DefectCategory
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
