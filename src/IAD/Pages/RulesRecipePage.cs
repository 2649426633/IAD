using System.Drawing;
using System.Windows.Forms;
using IAD.UI;

namespace IAD.Pages
{
    public class RulesRecipePage : UserControl
    {
        public RulesRecipePage()
        {
            Dock = DockStyle.Fill; BackColor = UiTheme.Page; Padding = new Padding(14, 14, 4, 10); Build();
        }

        private void Build()
        {
            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = UiTheme.Page };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 290F)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 165F));
            string[][] rules = {
                new[] { "划痕", "全局", "≥0.90", "≥8.00", "≥3.00", "≥0.20", "≤0", "NG" }, new[] { "刮伤", "全局", "≥0.85", "≥5.00", "≥2.00", "≥0.20", "≤1", "NG" },
                new[] { "凹坑", "全局", "≥0.90", "≥4.00", "≥1.50", "≥0.20", "≤2", "NG" }, new[] { "脏污", "全局", "≥0.80", "≥12.00", "≥4.00", "≥0.20", "≤2", "NG" },
                new[] { "气泡", "全局", "≥0.85", "≥2.00", "≥1.00", "≥0.20", "≤5", "NG" }, new[] { "缺角", "全局", "≥0.90", "≥6.00", "≥2.00", "≥0.20", "≤1", "NG" }
            };
            TableLayoutPanel top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = UiTheme.Page };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F)); top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            top.Controls.Add(UiFactory.Card("质量规则编辑（按缺陷类别）", UiFactory.Grid(new[] { "缺陷", "区域", "Confidence", "Area(mm²)", "Length(mm)", "Width(mm)", "Count", "结果" }, rules)), 0, 0);
            Panel zones = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(205, 205, 205), Margin = new Padding(12) };
            zones.Paint += delegate(object sender, PaintEventArgs e)
            {
                Rectangle all = new Rectangle(35, 25, zones.Width - 70, zones.Height - 60); using (Pen p = new Pen(Color.Black, 2)) e.Graphics.DrawRectangle(p, all);
                Rectangle critical = new Rectangle(all.X + 55, all.Y + 30, all.Width / 2, all.Height - 60); using (Pen p = new Pen(Color.FromArgb(70, 70, 70), 3)) e.Graphics.DrawRectangle(p, critical);
                Rectangle normal = new Rectangle(all.Right - all.Width / 4 - 20, all.Y + 30, all.Width / 5, all.Height - 60); using (Pen p = new Pen(Color.FromArgb(125, 125, 125), 2)) e.Graphics.DrawRectangle(p, normal);
                e.Graphics.DrawString("Critical Zone", UiTheme.Font(9F, true), Brushes.Black, critical.X + 8, critical.Y + 8); e.Graphics.DrawString("Normal Zone", UiTheme.Font(9F, true), Brushes.Black, normal.X + 5, normal.Y + 8); e.Graphics.DrawString("Ignore Zone: 边缘区域", UiTheme.Font(8F, false), Brushes.Black, all.X + 5, all.Bottom + 8);
            };
            top.Controls.Add(UiFactory.Card("检测区域定义", zones), 1, 0); root.Controls.Add(top, 0, 0);
            TableLayoutPanel mid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = UiTheme.Page };
            mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F)); mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F)); mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            mid.Controls.Add(UiFactory.Card("分级阈值配置（Critical Zone）", UiFactory.Grid(new[] { "缺陷", "Confidence", "Area", "Length", "Width", "Count" }, new[] { new[] { "划痕", "≥0.95", "≥10", "≥4", "≥0.20", "≤0" }, new[] { "刮伤", "≥0.90", "≥6", "≥2.5", "≥0.20", "≤1" }, new[] { "凹坑", "≥0.95", "≥5", "≥2", "≥0.20", "≤2" }, new[] { "脏污", "≥0.85", "≥15", "≥5", "≥0.20", "≤2" } })), 0, 0);
            mid.Controls.Add(UiFactory.Card("检测配方（Recipe）组成", UiFactory.KeyValues(new[,] { { "DatasetVersion：", "DSV_2025-05-10_v1.2.0" }, { "LocalizationTemplate：", "LT_2025-05-06_v2.0.1" }, { "ModelVersion：", "MV_2025-05-14_v1.3.0" }, { "RuleVersion：", "RV_2025-05-16_v1.1.0" }, { "CalibrationVersion：", "CV_2025-05-01_v1.0.2" }, { "ThresholdVersion：", "TV_2025-05-16_v1.0.0" }, { "Recipe名称：", "Recipe_2025-05-16_001" } }, 42)), 1, 0);
            mid.Controls.Add(UiFactory.Card("Inspection Recipe 版本", UiFactory.Grid(new[] { "版本", "状态", "创建时间", "创建人" }, new[] { new[] { "1.0.0", "当前", "05-16 14:35", "Admin" }, new[] { "0.9.2", "已发布", "05-14 10:21", "Admin" }, new[] { "0.9.1", "已发布", "05-12 16:42", "Admin" }, new[] { "0.9.0", "已发布", "05-10 11:07", "Admin" } })), 2, 0); root.Controls.Add(mid, 0, 1);
            TableLayoutPanel bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = UiTheme.Page };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F)); bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            bottom.Controls.Add(UiFactory.Card("接受策略（Acceptance Strategy）", UiFactory.KeyValues(new[,] { { "当前策略：", "均衡" }, { "目标漏检率(FNR)：", "≤ 2.00%" }, { "目标误检率(FPR)：", "≤ 5.00%" }, { "代价权重：", "漏检 3 : 误检 1" } }, 38)), 0, 0);
            bottom.Controls.Add(UiFactory.Card("策略预估（验证集）", UiFactory.KeyValues(new[,] { { "预计FNR：", "1.68% / 达标" }, { "预计FPR：", "3.72% / 达标" }, { "综合代价：", "0.86 / 较优" }, { "评估样本：", "18,732" } }, 40)), 1, 0); root.Controls.Add(bottom, 0, 2); Controls.Add(root);
        }
    }
}
