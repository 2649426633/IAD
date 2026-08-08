using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using IAD.UI;

namespace IAD.Pages
{
    public partial class DatasetAnnotationPage
    {
        private IContainer components = null;
        private Panel annotationCanvas;
        private DataGridView imageGrid;
        private DataGridView classGrid;

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
            this.Name = "DatasetAnnotationPage";
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
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 105F));
            root.Controls.Add(UiFactory.Toolbar("导入图片", "Rectangle", "Polygon", "Brush", "Eraser", "Mask修改", "自动修正", "版本管理"), 0, 0);
            root.Controls.Add(BuildMainBody(), 0, 1);
            root.Controls.Add(UiFactory.Card("标注队列 / 缩略图", BuildFilmstrip()), 0, 2);

            this.Controls.Add(root);
            this.ResumeLayout(true);
        }

        private Control BuildMainBody()
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
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 290F));

            string[][] images =
            {
                new[] { "IMG_142310.png", "已标注" },
                new[] { "IMG_143105.png", "已标注" },
                new[] { "IMG_143701.png", "部分标注" },
                new[] { "IMG_144203.png", "未标注" },
                new[] { "IMG_144856.png", "已标注" },
                new[] { "IMG_142756.png", "已标注" },
                new[] { "IMG_143012.png", "未标注" }
            };
            imageGrid = UiFactory.Grid(new[] { "文件", "状态" }, images);
            body.Controls.Add(UiFactory.Card("数据集图片", imageGrid), 0, 0);

            annotationCanvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(62, 62, 62),
                Margin = new Padding(8)
            };
            annotationCanvas.Paint += PaintAnnotationCanvas;
            body.Controls.Add(UiFactory.Card("标注画布  |  2048 × 1536  |  Fit", annotationCanvas), 1, 0);

            body.Controls.Add(BuildRightPanel(), 2, 0);
            return body;
        }

        private void PaintAnnotationCanvas(object sender, PaintEventArgs e)
        {
            Panel canvas = sender as Panel;
            if (canvas == null) return;
            int w = Math.Max(80, (canvas.Width - 80) / 3);
            int h = Math.Max(80, (canvas.Height - 70) / 2);
            for (int r = 0; r < 2; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Rectangle box = new Rectangle(25 + c * (w + 15), 25 + r * (h + 15), w, h);
                    using (Brush b = new SolidBrush(Color.FromArgb(155, 155, 155)))
                    {
                        e.Graphics.FillEllipse(b, box.X + 20, box.Y + 10, Math.Max(20, box.Width - 40), Math.Max(20, box.Height - 20));
                    }
                    using (Pen p = new Pen(Color.White, 1.5F))
                    {
                        e.Graphics.DrawRectangle(p, box);
                    }
                    e.Graphics.DrawString((c + r * 3 + 1).ToString("00") + "  标注区域", UiTheme.Font(8F, true), Brushes.White, box.X + 6, box.Y + 6);
                }
            }
        }

        private Control BuildRightPanel()
        {
            TableLayoutPanel right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            string[][] classes =
            {
                new[] { "1", "划痕", "Rectangle" },
                new[] { "2", "缺料", "Polygon" },
                new[] { "3", "裂纹", "Polygon" },
                new[] { "4", "脏污", "Brush/Mask" }
            };
            classGrid = UiFactory.Grid(new[] { "ID", "类别", "类型" }, classes);
            right.Controls.Add(UiFactory.Card("瑕疵类别", classGrid), 0, 0);
            right.Controls.Add(UiFactory.Card("当前类别", UiFactory.KeyValues(new[,] { { "类别：", "划痕 (ID:1)" }, { "线宽：", "2" }, { "置信度阈值：", "0.50" } }, 45)), 0, 1);
            right.Controls.Add(UiFactory.Card("图层与标注属性", UiFactory.KeyValues(new[,] { { "划痕：", "4 / 可见" }, { "缺料：", "1 / 可见" }, { "裂纹：", "1 / 可见" }, { "脏污：", "1 / 可见" } }, 45)), 0, 2);
            right.Controls.Add(UiFactory.Card("标注质量", UiFactory.KeyValues(new[,] { { "总标注：", "7" }, { "边界完整性：", "0.92" }, { "综合评分：", "0.94" }, { "建议：", "通过" } }, 45)), 0, 3);
            return right;
        }

        private Control BuildFilmstrip()
        {
            FlowLayoutPanel film = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                BackColor = UiTheme.Surface,
                Padding = new Padding(10, 8, 0, 6)
            };
            for (int i = 1; i <= 10; i++)
            {
                Panel thumb = new Panel
                {
                    Width = 92,
                    Height = 72,
                    BackColor = Color.FromArgb(220, 220, 220),
                    Margin = new Padding(0, 0, 8, 0)
                };
                Label label = UiFactory.Label(i.ToString("0000") + "\n" + (i % 4 == 0 ? "未标注" : "已标注"), 8F, false, ContentAlignment.MiddleCenter);
                thumb.Controls.Add(label);
                film.Controls.Add(thumb);
            }
            return film;
        }
    }
}
