using System;

namespace IAD.Models
{
    public sealed class RoiDefinition
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string RoiName { get; set; }
        public string RoiType { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double AngleDeg { get; set; }
        public string GeometryJson { get; set; }
        public int SortIndex { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
