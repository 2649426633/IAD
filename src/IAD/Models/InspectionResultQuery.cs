using System;

namespace IAD.Models
{
    public sealed class InspectionResultQuery
    {
        public long? ProductId { get; set; }
        public long? RecipeId { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public string OverallResult { get; set; }
        public string CategoryCode { get; set; }
        public string Keyword { get; set; }
        public int Limit { get; set; }
    }
}
