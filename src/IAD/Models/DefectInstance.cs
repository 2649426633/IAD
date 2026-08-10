namespace IAD.Models
{
    public sealed class DefectInstance
    {
        public long Id { get; set; }
        public long InspectionResultId { get; set; }
        public long? RoiId { get; set; }
        public long? CategoryId { get; set; }
        public string RoiName { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public double Confidence { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Area { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Result { get; set; }
        public string RuleDecision { get; set; }
    }
}
