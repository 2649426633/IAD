namespace IAD.Models
{
    public sealed class RecipeRule
    {
        public long Id { get; set; }
        public long RecipeId { get; set; }
        public long? CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string RoiName { get; set; }
        public double MinConfidence { get; set; }
        public double MinArea { get; set; }
        public double MinWidth { get; set; }
        public double MinHeight { get; set; }
        public int MaxAllowedCount { get; set; }
        public string Decision { get; set; }
        public bool IsEnabled { get; set; }
    }
}
