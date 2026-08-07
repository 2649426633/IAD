using System.Drawing;

namespace IAD.UI
{
    internal static class UiTheme
    {
        public static readonly Color Page = Color.FromArgb(247, 247, 247);
        public static readonly Color Surface = Color.White;
        public static readonly Color Header = Color.FromArgb(244, 244, 244);
        public static readonly Color Active = Color.FromArgb(214, 214, 214);
        public static readonly Color Border = Color.FromArgb(207, 207, 207);
        public static readonly Color SoftBorder = Color.FromArgb(229, 229, 229);
        public static readonly Color Text = Color.FromArgb(25, 25, 25);
        public static readonly Color Muted = Color.FromArgb(95, 95, 95);
        public static readonly Color Canvas = Color.FromArgb(35, 35, 35);

        public static Font Font(float size, bool bold)
        {
            return new Font("Microsoft YaHei UI", size, bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point);
        }
    }
}
