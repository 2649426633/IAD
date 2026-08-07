using System.Drawing;
using System.Windows.Forms;
using IAD.UI;

namespace IAD.Pages
{
    public class ProductDefinitionPage : UserControl
    {
        public ProductDefinitionPage()
        {
            Dock = DockStyle.Fill; BackColor = UiTheme.Page; Padding = new Padding(14, 14, 4, 10); Build();
        }

        private void Build()
        {
            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = UiTheme.Page };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 260F)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TableLayoutPanel top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = UiTheme.Page };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 355F)); top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            string[,] info = {
                { "产品名称：", "轴承盖板" }, { "产品编号：", "P-20250516-001" }, { "图像尺寸：", "2448 × 2048 px" },
                { "相机固定：", "是" }, { "光照稳定：", "是" }, { "背景稳定：", "是" }, { "多产品数量：", "1" }, { "是否旋转：", "允许" }
            };
            top.Controls.Add(UiFactory.Card("产品基本信息", UiFactory.KeyValues(info, 40)), 0, 0);
            string[][] defects = {
                new[] { "1", "划痕", "表面缺陷", "Multi-label Segmentation", "30", "15", "启用" },
                new[] { "2", "脏污", "表面缺陷", "Multi-label Segmentation", "50", "-", "启用" },
                new[] { "3", "凹坑", "表面缺陷", "Multi-label Segmentation", "40", "-", "启用" },
                new[] { "4", "孔洞", "结构缺陷", "Multi-label Segmentation", "20", "-", "启用" },
                new[] { "5", "缺边", "结构缺陷", "Multi-label Segmentation", "-", "25", "启用" },
                new[] { "6", "异物", "表面缺陷", "Multi-label Segmentation", "20", "-", "启用" }
            };
            top.Controls.Add(UiFactory.Card("缺陷类别管理", UiFactory.Grid(new[] { "序号", "缺陷名称", "类型", "检测策略", "最小面积(px)", "最小长度(px)", "状态" }, defects)), 1, 0);

            TableLayoutPanel lower = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = UiTheme.Page };
            lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47F)); lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F)); lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            Panel preview = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(230, 230, 230), Margin = new Padding(12) };
            preview.Paint += delegate(object sender, PaintEventArgs e)
            {
                Rectangle r = new Rectangle(50, 35, System.Math.Max(100, preview.Width - 100), System.Math.Max(100, preview.Height - 75));
                using (Pen p = new Pen(Color.FromArgb(90, 90, 90), 2)) e.Graphics.DrawRectangle(p, r);
                Rectangle roi = new Rectangle(r.X + 28, r.Y + 24, r.Width - 56, r.Height - 48);
                using (Pen p = new Pen(Color.FromArgb(20, 20, 20), 2)) e.Graphics.DrawRectangle(p, roi);
                e.Graphics.DrawString("HALCON 产品模板预览 / ROI", UiTheme.Font(10F, true), Brushes.Black, r.X + 15, r.Y + 12);
            };
            FlowLayoutPanel templateBody = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = UiTheme.Surface, Padding = new Padding(10) };
            preview.Width = 610; preview.Height = 280; templateBody.Controls.Add(preview);
            templateBody.Controls.Add(UiFactory.Toolbar("快速模式", "精细模式", "自动模式", "测试模板"));
            lower.Controls.Add(UiFactory.Card("产品模板设置", templateBody), 0, 0);
            string[,] localization = {
                { "定位方式：", "HALCON Shape Matching" }, { "模型类型：", "Shape Model" }, { "最小匹配分数：", "0.60" },
                { "金字塔层数：", "3" }, { "角度范围：", "-30° ~ 30°" }, { "缩放范围：", "90% ~ 110%" }, { "匹配个数：", "1" },
                { "最近结果：", "Row 1024.32 / Col 1226.78 / Angle -1.24° / Score 0.92" }
            };
            lower.Controls.Add(UiFactory.Card("定位配置（HALCON Shape Matching）", UiFactory.KeyValues(localization, 34)), 1, 0);
            string[,] calibration = {
                { "像素尺寸X：", "6.5000 μm/px" }, { "像素尺寸Y：", "6.5000 μm/px" }, { "面积单位：", "mm²" },
                { "换算系数：", "0.00004225" }, { "标定版本：", "CV-1.0.2" }, { "状态：", "有效" }
            };
            lower.Controls.Add(UiFactory.Card("标定设置", UiFactory.KeyValues(calibration, 48)), 2, 0);
            root.Controls.Add(top, 0, 0); root.Controls.Add(lower, 0, 1); Controls.Add(root);
        }
    }
}
