using System;
using System.Collections.Generic;

namespace IAD.Models
{
    public sealed class InspectionResult
    {
        public InspectionResult()
        {
            Defects = new List<DefectInstance>();
        }

        public long Id { get; set; }
        public long ProductId { get; set; }
        public long? RecipeId { get; set; }
        public string BatchCode { get; set; }
        public string SourceImagePath { get; set; }
        public string OverallResult { get; set; }
        public double LocalizationScore { get; set; }
        public string ModelVersion { get; set; }
        public string RuleVersion { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime FinishedAtUtc { get; set; }
        public List<DefectInstance> Defects { get; private set; }
    }
}
