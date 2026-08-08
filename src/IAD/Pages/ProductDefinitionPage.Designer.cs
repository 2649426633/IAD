using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using IAD.UI;

namespace IAD.Pages
{
    public partial class ProductDefinitionPage
    {
        private IContainer components = null;

        private Button btnSave;
        private Button btnBuildTemplate;
        private Button btnTestLocation;
        private Button btnVersion;
        private Button btnImportReference;
        private Button btnRectangleRoi;
        private Button btnClearRoi;
        private Button btnFastMode;
        private Button btnPreciseMode;
        private Button btnAutoMode;
        private Panel templateCanvas;
        private DataGridView defectGrid;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(8F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(247, 247, 247);
            this.Name = "ProductDefinitionPage";
            this.Size = new Size(1400, 820);
            this.ResumeLayout(false);
        }

        /// <summary>
        /// 页面视觉结构。不要在此方法中写业务逻辑。
        /// 该方法由 ProductDefinitionPage 构造函数在 InitializeComponent 之后调用，
        /// 从而避免旧式 WinForms CodeDom Designer 解析复杂布局代码。
        /// </summary>
        private void BuildView()
        {
            this.SuspendLayout();
            this.Controls.Clear();
            this.Dock = DockStyle.Fill;
            this.BackColor = UiTheme.Page;
            this.Padding = new Padding(14, 12, 4, 10);

            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));

            root.Controls.Add(BuildCommandBar(), 0, 0);
            root.Controls.Add(BuildProductSummary(), 0, 1);
            root.Controls.Add(BuildTemplateAndParameters(), 0, 2);
            root.Controls.Add(BuildDefectCategories(), 0, 3);

            this.Controls.Add(root);
            this.ResumeLayout(true);
        }

        private Control BuildCommandBar()
        {
            TableLayoutPanel bar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390F));

            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = new Padding(0, 3, 0, 5)
            };

            btnSave = UiFactory.Button("保存产品定义", 112);
            btnBuildTemplate = UiFactory.Button("建立 / 更新模板", 128);
            btnTestLocation = UiFactory.Button("测试定位", 92);
            btnVersion = UiFactory.Button("版本记录", 92);
            actions.Controls.Add(btnSave);
            actions.Controls.Add(btnBuildTemplate);
            actions.Controls.Add(btnTestLocation);
            actions.Controls.Add(btnVersion);

            Label version = new Label
            {
                Dock = DockStyle.Fill,
                Text = "产品定义版本：PD-1.0.0    模板版本：LT-1.0.0    状态：已配置",
                Font = UiTheme.Font(8.6F, false),
                ForeColor = UiTheme.Muted,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 12, 0)
            };

            bar.Controls.Add(actions, 0, 0);
            bar.Controls.Add(version, 1, 0);
            return bar;
        }

        private Control BuildProductSummary()
        {
            TableLayoutPanel summary = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = UiTheme.Surface,
                Margin = Padding.Empty,
                Padding = new Padding(12, 8, 12, 8)
            };
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19F));
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));

            summary.Controls.Add(BuildSummaryItem("产品名称", "轴承盖板"), 0, 0);
            summary.Controls.Add(BuildSummaryItem("产品编号", "P-20250516-001"), 1, 0);
            summary.Controls.Add(BuildSummaryItem("图像尺寸", "2448 × 2048 px"), 2, 0);
            summary.Controls.Add(BuildSummaryItem("单图产品数", "1"), 3, 0);
            summary.Controls.Add(BuildSummaryItem("姿态", "允许旋转"), 4, 0);
            summary.Controls.Add(BuildSummaryItem("采集条件", "相机 / 光照 / 背景稳定"), 5, 0);

            return UiFactory.Card("产品基本信息", summary);
        }

        private Control BuildSummaryItem(string keyText, string valueText)
        {
            TableLayoutPanel item = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = UiTheme.Surface,
                Margin = new Padding(6, 0, 10, 0),
                Padding = new Padding(8, 3, 8, 3)
            };
            item.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            item.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Label key = UiFactory.Label(keyText, 8.4F, false, ContentAlignment.MiddleLeft);
            key.ForeColor = UiTheme.Muted;
            Label value = UiFactory.Label(valueText, 9.5F, true, ContentAlignment.MiddleLeft);
            value.AutoEllipsis = true;

            item.Controls.Add(key, 0, 0);
            item.Controls.Add(value, 0, 1);
            item.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(UiTheme.SoftBorder))
                {
                    e.Graphics.DrawLine(pen, item.Width - 1, 8, item.Width - 1, Math.Max(8, item.Height - 8));
                }
            };
            return item;
        }

        private Control BuildTemplateAndParameters()
        {
            TableLayoutPanel middle = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            middle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));
            middle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));

            middle.Controls.Add(BuildTemplateWorkspace(), 0, 0);
            middle.Controls.Add(BuildParameterStack(), 1, 0);
            return middle;
        }

        private Control BuildTemplateWorkspace()
        {
            TableLayoutPanel body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = UiTheme.Surface,
                Margin = Padding.Empty,
                Padding = new Padding(10, 2, 10, 8)
            };
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            FlowLayoutPanel tools = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = UiTheme.Surface,
                Margin = Padding.Empty,
                Padding = new Padding(0, 5, 0, 4)
            };

            btnImportReference = UiFactory.Button("导入基准图", 94);
            btnRectangleRoi = UiFactory.Button("Rectangle ROI", 110);
            btnClearRoi = UiFactory.Button("清除 ROI", 88);
            btnFastMode = UiFactory.Button("快速模式", 82);
            btnPreciseMode = UiFactory.Button("精细模式", 82);
            btnAutoMode = UiFactory.Button("自动模式", 82);

            tools.Controls.Add(btnImportReference);
            tools.Controls.Add(btnRectangleRoi);
            tools.Controls.Add(btnClearRoi);
            tools.Controls.Add(btnFastMode);
            tools.Controls.Add(btnPreciseMode);
            tools.Controls.Add(btnAutoMode);

            templateCanvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                Margin = new Padding(0, 2, 0, 4)
            };
            templateCanvas.Paint += PaintTemplateCanvas;

            TableLayoutPanel footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = UiTheme.Header,
                Margin = Padding.Empty,
                Padding = new Padding(10, 0, 10, 0)
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            footer.Controls.Add(BuildFooterText("基准图：reference_001.png"), 0, 0);
            footer.Controls.Add(BuildFooterText("ROI：已定义"), 1, 0);
            footer.Controls.Add(BuildFooterText("定位模板：Shape Model"), 2, 0);
            footer.Controls.Add(BuildFooterText("最近测试：Score 0.92"), 3, 0);

            body.Controls.Add(tools, 0, 0);
            body.Controls.Add(templateCanvas, 0, 1);
            body.Controls.Add(footer, 0, 2);
            return UiFactory.Card("产品模板与 ROI", body);
        }

        private void PaintTemplateCanvas(object sender, PaintEventArgs e)
        {
            Panel canvas = sender as Panel;
            if (canvas == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int w = Math.Max(420, canvas.Width);
            int h = Math.Max(260, canvas.Height);
            int productWidth = Math.Min(410, Math.Max(220, w - 150));
            int productHeight = Math.Min(240, Math.Max(150, h - 80));
            int x = (w - productWidth) / 2;
            int y = (h - productHeight) / 2;

            using (SolidBrush productBrush = new SolidBrush(Color.FromArgb(176, 176, 176)))
            using (Pen productPen = new Pen(Color.FromArgb(215, 215, 215), 2F))
            {
                Rectangle productRect = new Rectangle(x, y, productWidth, productHeight);
                e.Graphics.FillRectangle(productBrush, productRect);
                e.Graphics.DrawRectangle(productPen, productRect);

                Rectangle inner = new Rectangle(x + 42, y + 34, Math.Max(30, productWidth - 84), Math.Max(30, productHeight - 68));
                using (SolidBrush innerBrush = new SolidBrush(Color.FromArgb(112, 112, 112)))
                {
                    e.Graphics.FillRectangle(innerBrush, inner);
                }

                DrawHole(e.Graphics, x + 24, y + 22, 18);
                DrawHole(e.Graphics, x + productWidth - 42, y + 22, 18);
                DrawHole(e.Graphics, x + 24, y + productHeight - 40, 18);
                DrawHole(e.Graphics, x + productWidth - 42, y + productHeight - 40, 18);
            }

            Rectangle roi = new Rectangle(x + 24, y + 20, Math.Max(30, productWidth - 48), Math.Max(30, productHeight - 40));
            using (Pen roiPen = new Pen(Color.White, 2F))
            {
                roiPen.DashStyle = DashStyle.Dash;
                e.Graphics.DrawRectangle(roiPen, roi);
            }

            using (Font font = UiTheme.Font(9.3F, true))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString("Template ROI", font, brush, roi.X + 8, roi.Y + 7);
            }
            using (Font tipFont = UiTheme.Font(8.2F, false))
            using (SolidBrush tipBrush = new SolidBrush(Color.FromArgb(205, 205, 205)))
            {
                e.Graphics.DrawString("基准图预览 · 鼠标框选产品区域作为定位模板", tipFont, tipBrush, 14, 12);
            }
        }

        private static void DrawHole(Graphics g, int x, int y, int size)
        {
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(55, 55, 55)))
            {
                g.FillEllipse(brush, x, y, size, size);
            }
        }

        private Label BuildFooterText(string text)
        {
            Label label = UiFactory.Label(text, 7.8F, false, ContentAlignment.MiddleLeft);
            label.ForeColor = UiTheme.Muted;
            label.AutoEllipsis = true;
            return label;
        }

        private Control BuildParameterStack()
        {
            TableLayoutPanel stack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
            stack.Controls.Add(UiFactory.Card("定位参数", BuildLocalizationParameters()), 0, 0);
            stack.Controls.Add(UiFactory.Card("尺寸标定", BuildCalibrationParameters()), 0, 1);
            return stack;
        }

        private Control BuildLocalizationParameters()
        {
            string[,] data =
            {
                { "定位方式：", "HALCON Shape Matching" },
                { "模型类型：", "Shape Model" },
                { "最小匹配分数：", "0.60" },
                { "角度范围：", "-30° ~ 30°" },
                { "缩放范围：", "90% ~ 110%" },
                { "匹配个数：", "1" },
                { "最近结果：", "Row 1024.32 / Col 1226.78 / Angle -1.24° / Score 0.92" }
            };
            return UiFactory.KeyValues(data, 38);
        }

        private Control BuildCalibrationParameters()
        {
            string[,] data =
            {
                { "像素尺寸 X：", "6.5000 μm/px" },
                { "像素尺寸 Y：", "6.5000 μm/px" },
                { "长度单位：", "mm" },
                { "面积单位：", "mm²" },
                { "标定版本：", "CV-1.0.2" },
                { "状态：", "有效" }
            };
            return UiFactory.KeyValues(data, 42);
        }

        private Control BuildDefectCategories()
        {
            TableLayoutPanel body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = UiTheme.Surface,
                Margin = Padding.Empty,
                Padding = new Padding(10, 0, 10, 8)
            };
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel tools = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = UiTheme.Surface,
                Margin = Padding.Empty,
                Padding = new Padding(0, 5, 0, 5)
            };
            tools.Controls.Add(UiFactory.Button("新增类别", 86));
            tools.Controls.Add(UiFactory.Button("编辑类别", 86));
            tools.Controls.Add(UiFactory.Button("删除类别", 86));
            tools.Controls.Add(UiFactory.Button("启用 / 停用", 98));
            tools.Controls.Add(UiFactory.Button("导入配置", 86));
            tools.Controls.Add(UiFactory.Button("导出配置", 86));

            string[][] defects =
            {
                new[] { "1", "划痕", "表面缺陷", "Multi-label Segmentation", "0.80", "30", "15", "启用" },
                new[] { "2", "脏污", "表面缺陷", "Multi-label Segmentation", "0.75", "50", "-", "启用" },
                new[] { "3", "凹坑", "表面缺陷", "Multi-label Segmentation", "0.82", "40", "-", "启用" },
                new[] { "4", "孔洞", "结构缺陷", "Multi-label Segmentation", "0.90", "20", "-", "启用" },
                new[] { "5", "缺边", "结构缺陷", "Multi-label Segmentation", "0.90", "-", "25", "启用" },
                new[] { "6", "异物", "表面缺陷", "Multi-label Segmentation", "0.78", "20", "-", "启用" }
            };

            defectGrid = UiFactory.Grid(
                new[] { "序号", "缺陷名称", "缺陷类型", "检测策略", "默认阈值", "最小面积(px)", "最小长度(px)", "状态" },
                defects);

            body.Controls.Add(tools, 0, 0);
            body.Controls.Add(defectGrid, 0, 1);
            return UiFactory.Card("缺陷类别管理", body);
        }
    }
}
