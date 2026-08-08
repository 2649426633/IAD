using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using IAD.UI;

namespace IAD.Pages
{
    public partial class OnlineInspectionPage
    {
        private IContainer components = null;
        private Panel inspectionCanvas;
        private DataGridView queueGrid;
        private DataGridView resultGrid;

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
            this.Name = "OnlineInspectionPage";
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
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 155F));

            FlowLayoutPanel toolbar = UiFactory.Toolbar("加载图片", "批量检测", "开始检测", "暂停", "导出结果");
            Label metrics = UiFactory.Label("当前速度  28件/分     今日检测  12,560件     NG率  2.34%", 10F, true, ContentAlignment.MiddleRight);
            metrics.Width = 460;
            metrics.Height = 35;
            toolbar.Controls.Add(metrics);
            root.Controls.Add(toolbar, 0, 0);
            root.Controls.Add(BuildBody(), 0, 1);
            root.Controls.Add(BuildBottom(), 0, 2);

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
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 235F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 305F));

            string[][] queue = {
                new[] { "IMG_142310.png", "检测中" }, new[] { "IMG_142311.png", "等待中" },
                new[] { "IMG_142312.png", "等待中" }, new[] { "IMG_142313.png", "等待中" },
                new[] { "IMG_142314.png", "等待中" }, new[] { "IMG_142315.png", "等待中" }
            };
            queueGrid = UiFactory.Grid(new[] { "文件", "状态" }, queue);
            body.Controls.Add(UiFactory.Card("图像队列 (24)", queueGrid), 0, 0);

            inspectionCanvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Canvas,
                Margin = new Padding(8)
            };
            inspectionCanvas.Paint += PaintInspectionCanvas;
            body.Controls.Add(UiFactory.Card("当前图像：IMG_20250516_142310.png  |  2448 × 2048  |  Fit", inspectionCanvas), 1, 0);

            body.Controls.Add(BuildRightPanel(), 2, 0);
            return body;
        }

        private void PaintInspectionCanvas(object sender, PaintEventArgs e)
        {
            Panel canvas = sender as Panel;
            if (canvas == null) return;

            int gap = 18;
            int w = Math.Max(70, (canvas.Width - gap * 5) / 4);
            int h = Math.Max(70, (canvas.Height - gap * 3) / 2);
            for (int i = 0; i < 8; i++)
            {
                int r = i / 4;
                int c = i % 4;
                Rectangle box = new Rectangle(gap + c * (w + gap), gap + r * (h + gap), w, h);
                string state = (i == 1 || i == 3 || i == 5) ? "NG" : (i == 6 ? "ERROR" : "OK");

                using (Pen p = new Pen(i == 6 ? Color.White : Color.FromArgb(160, 160, 160), i == 6 ? 2F : 1F))
                {
                    e.Graphics.DrawRectangle(p, box);
                }
                using (Brush b = new SolidBrush(Color.FromArgb(150, 150, 150)))
                {
                    e.Graphics.FillRectangle(b, box.X + box.Width / 4, box.Y + 15, Math.Max(20, box.Width / 2), Math.Max(20, box.Height - 30));
                }
                e.Graphics.DrawString("#" + (i + 1) + "  " + state, UiTheme.Font(8.5F, true), Brushes.White, box.X + 5, box.Y + 5);
                if (state == "NG")
                {
                    using (Pen p = new Pen(Color.White, 2F))
                    {
                        e.Graphics.DrawLine(p, box.X + 20, box.Bottom - 35, box.Right - 18, box.Y + 30);
                    }
                }
            }
        }

        private Control BuildRightPanel()
        {
            TableLayoutPanel right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));

            string[][] results = {
                new[] { "#1", "OK", "0" }, new[] { "#2", "NG", "1" }, new[] { "#3", "OK", "0" }, new[] { "#4", "NG", "2" },
                new[] { "#5", "OK", "0" }, new[] { "#6", "NG", "1" }, new[] { "#7", "ERROR", "-" }, new[] { "#8", "OK", "0" }
            };
            resultGrid = UiFactory.Grid(new[] { "产品", "状态", "缺陷数" }, results);
            right.Controls.Add(UiFactory.Card("产品结果 (8)", resultGrid), 0, 0);
            right.Controls.Add(UiFactory.Card("#2 缺陷详情", UiFactory.Grid(
                new[] { "类型", "置信度", "面积(px)", "位置" },
                new[] { new[] { "划痕", "0.96", "1824", "(1320,182)" } })), 0, 1);
            return right;
        }

        private Control BuildBottom()
        {
            TableLayoutPanel bottom = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));

            bottom.Controls.Add(UiFactory.Card("检测流程", UiFactory.KeyValues(new[,] {
                { "1 图像获取：", "OK" }, { "2 预处理：", "OK" }, { "3 产品定位：", "OK" }, { "4 缺陷检测：", "运行中" }, { "5 规则判定：", "等待中" }
            }, 44)), 0, 0);

            bottom.Controls.Add(UiFactory.Card("配置 / 推理后端", UiFactory.KeyValues(new[,] {
                { "当前Recipe：", "Recipe 1.0.0" }, { "模型版本：", "V1.2.3" }, { "后端：", "CUDA / TensorRT" }, { "设备：", "NVIDIA RTX 3060" }
            }, 43)), 1, 0);

            bottom.Controls.Add(UiFactory.Card("NG原因统计（今日）", UiFactory.Grid(
                new[] { "缺陷", "数量", "占比" },
                new[] {
                    new[] { "划痕", "186", "45.8%" }, new[] { "脏污", "128", "31.5%" }, new[] { "缺口", "73", "18.0%" }, new[] { "变形", "19", "4.7%" }
                })), 2, 0);
            return bottom;
        }
    }
}
