using System;

namespace IAD.Models
{
    public sealed class InspectionRecipe
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string RecipeCode { get; set; }
        public string RecipeName { get; set; }
        public string DatasetVersion { get; set; }
        public string LocalizationTemplateVersion { get; set; }
        public string ModelVersion { get; set; }
        public string RuleVersion { get; set; }
        public string CalibrationVersion { get; set; }
        public string ThresholdVersion { get; set; }
        public long? ModelId { get; set; }
        public System.Collections.Generic.List<RecipeRule> Rules { get; private set; } = new System.Collections.Generic.List<RecipeRule>();
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
