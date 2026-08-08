using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using IAD.UI;

namespace IAD.Pages
{
    public partial class TemplateRecognitionPage
    {
        private IContainer components = null;
        private DataGridView candidateGrid;
        private Panel queryCanvas;
        private Panel heatmapCanvas;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(8F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(247, 247, 247);
            this.Name = "TemplateRecognitionPage";
            this.Size = new Size(1400, 820);
            this.ResumeLayout(false);
        }

        private void BuildView()
        {
            this.SuspendLayout();
            this.Controls.Clear();
            this.Dock = DockStyle.Fill;
            this.BackColor = UiTheme.Page;
            this.Padding = new Padding(14, 10, 4, 10);

            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 105F));

            root.Controls.Add(UiFactory.Card("Few-shot 控制", UiFactory.KeyValues(new[,] {
                { "瑕疵类别：", "划痕 Scratches" }, { "正样本：", "32" }, { "Hard Negative：", "128" }, { "相似度阈值：", "0.72" }, { "Top-K：", "10" }
            }, 36)), 0, 0);
            root.Controls.Add(BuildBody(), 0, 1);
            root.Controls.Add(UiFactory.Card("学习循环进度  |  本轮：已确认126 / 已拒绝34 / 待处理28", BuildLoop()), 0, 2);

            this.Controls.Add(root);
            this.ResumeLayout(true);
        }

        private Control BuildBody()
        {
            TableLayoutPanel body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 270F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F));

            body.Controls.Add(BuildSamplePanel(), 0, 0);
            body.Controls.Add(BuildCenterPanel(), 1, 0);
            body.Controls.Add(BuildCandidatePanel(), 2, 0);
            return body;
        }

        private Control BuildSamplePanel()
        {
            TableLayoutPanel left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = UiTheme.Page
            };
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));

            string[][] samples = {
                new[] { "P-0001", "0.96" }, new[] { "P-0002", "0.94" }, new[] { "P-0003", "0.93" }, new[] { "P-0004", "0.91" }, new[] { "P-0005", "0.90" }
            };
            string[][] hardNegatives = {
                new[] { "HN-0128", "IMG_142310", "0.68" }, new[] { "HN-0127", "IMG_143105", "0.66" }, new[] { "HN-0126", "IMG_142856", "0.65" }, new[] { "HN-0125", "IMG_142725", "0.64" }
            };
            left.Controls.Add(UiFactory.Card("原型样本（正样本）", UiFactory.Grid(new[] { "ID", "相似度" }, samples)), 0, 0);
            left.Controls.Add(UiFactory.Card("Hard Negative", UiFactory.Grid(new[] { "ID", "来源", "相似度" }, hardNegatives)), 0, 1);
            return left;
        }

        private Control BuildCenterPanel()
        {
            TableLayoutPanel center = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = UiTheme.Page
            };
            center.RowStyles.Add(new RowStyle(SizeType.Absolute, 235F));
            center.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            TableLayoutPanel analysis = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = UiTheme.Page
            };
            analysis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            analysis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            queryCanvas = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(170, 170, 170), Margin = new Padding(10) };
            queryCanvas.Paint += delegate(object sender, PaintEventArgs e)
            {
                Panel p = sender as Panel;
                if (p == null) return;
                using (Pen pen = new Pen(Color.Black, 2)) e.Graphics.DrawLine(pen, 50, Math.Max(30, p.Height - 45), Math.Max(60, p.Width - 55), 45);
                e.Graphics.DrawString("查询瑕疵", UiTheme.Font(9F, true), Brushes.Black, 12, 12);
            };

            heatmapCanvas = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(140, 140, 140), Margin = new Padding(10) };
            heatmapCanvas.Paint += delegate(object sender, PaintEventArgs e)
            {
                Panel p = sender as Panel;
                if (p == null) return;
                using (Brush b = new SolidBrush(Color.FromArgb(80, 80, 80))) e.Graphics.FillEllipse(b, p.Width / 3, p.Height / 4, Math.Max(40, p.Width / 3), Math.Max(40, p.Height / 2));
                e.Graphics.DrawString("相似度热力图", UiTheme.Font(9F, true), Brushes.Black, 12, 12);
            };

            analysis.Controls.Add(UiFactory.Card("查询瑕疵（待标注）", queryCanvas), 0, 0);
            analysis.Controls.Add(UiFactory.Card("相似度热力图", heatmapCanvas), 1, 0);
            center.Controls.Add(analysis, 0, 0);

            candidateGrid = UiFactory.Grid(new[] { "排名", "相似度", "来源图像", "状态" }, CandidateRows());
            center.Controls.Add(UiFactory.Card("候选区域（Top-K）", candidateGrid), 0, 1);
            return center;
        }

        private Control BuildCandidatePanel()
        {
            TableLayoutPanel right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = UiTheme.Page
            };
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            right.Controls.Add(UiFactory.Card("候选列表", UiFactory.Grid(new[] { "排名", "相似度", "来源", "状态" }, CandidateRows())), 0, 0);
            FlowLayoutPanel actions = UiFactory.Toolbar("AI Mask精修", "人工确认", "人工拒绝", "加入Hard Negative");
            actions.FlowDirection = FlowDirection.TopDown;
            actions.WrapContents = false;
            actions.Padding = new Padding(12, 8, 0, 0);
            right.Controls.Add(UiFactory.Card("候选处理", actions), 0, 1);
            return right;
        }

        private static string[][] CandidateRows()
        {
            return new[] {
                new[] { "1", "0.93", "IMG_142310", "待确认" }, new[] { "2", "0.91", "IMG_143105", "待确认" },
                new[] { "3", "0.89", "IMG_142856", "待确认" }, new[] { "4", "0.87", "IMG_142725", "待确认" },
                new[] { "5", "0.86", "IMG_143015", "待确认" }, new[] { "6", "0.84", "IMG_142910", "待确认" }
            };
        }

        private Control BuildLoop()
        {
            FlowLayoutPanel loop = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = UiTheme.Surface,
                Padding = new Padding(18, 18, 0, 0)
            };
            string[] steps = { "1 人工标注", "→", "2 相似搜索", "→", "3 候选生成", "→", "4 Mask Refinement", "→", "5 人工确认" };
            for (int i = 0; i < steps.Length; i++)
            {
                Label label = UiFactory.Label(steps[i], 9F, i % 2 == 0, ContentAlignment.MiddleCenter);
                label.Width = i % 2 == 0 ? 120 : 28;
                label.Height = 45;
                loop.Controls.Add(label);
            }
            return loop;
        }
    }
}
