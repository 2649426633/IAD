using System.Drawing;

namespace IAD.Pages
{
    public partial class DatasetAnnotationPage
    {
        // DatasetAnnotationPage 的多个 partial 文件共享同一主题入口。
        // 所有值仍由全局 IAD.UI.UiTheme 提供，避免在 Canvas 文件中重复定义颜色和字体。
        private static class UiTheme
        {
            public static Color Page { get { return IAD.UI.UiTheme.Page; } }
            public static Color Surface { get { return IAD.UI.UiTheme.Surface; } }
            public static Color Header { get { return IAD.UI.UiTheme.Header; } }
            public static Color Active { get { return IAD.UI.UiTheme.Active; } }
            public static Color Border { get { return IAD.UI.UiTheme.Border; } }
            public static Color SoftBorder { get { return IAD.UI.UiTheme.SoftBorder; } }
            public static Color Text { get { return IAD.UI.UiTheme.Text; } }
            public static Color Muted { get { return IAD.UI.UiTheme.Muted; } }
            public static Color Canvas { get { return IAD.UI.UiTheme.Canvas; } }

            public static Font Font(float size, bool bold)
            {
                return IAD.UI.UiTheme.Font(size, bold);
            }
        }
    }
}
