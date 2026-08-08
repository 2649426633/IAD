using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using IAD.UI;

namespace IAD.Pages
{
    public partial class TraceabilityPage
    {
        private IContainer components = null;
        private DataGridView recordGrid;
        private Panel previewCanvas;
        private TextBox auditLog;

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
            this.Name = "TraceabilityPage";
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
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 190F));

            root.Controls.Add(UiFactory.Card("筛选条件", UiFactory.KeyValues(new[,] {
                { "日期范围：", "2025-05-15 ~ 2025-05-16" }, { "状态：", "全部" }, { "类别：", "全部" },
                { "Recipe Version：", "全部" }, { "图片名 / 批次：", "可输入查询" }
            }, 27)), 0, 0);
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
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));

            string[][] records = {
                new[] { "1", "IMG_143212", "14:32:12", "B250516-001", "24", "20", "3", "1", "V2.1.0", "NG" },
                new[] { "2", "IMG_143018", "14:30:18", "B250516-001", "24", "21", "2", "1", "V2.1.0", "NG" },
                new[] { "3", "IMG_143005", "14:30:05", "B250516-001", "24", "22", "1", "1", "V2.1.0", "NG" },
                new[] { "4", "IMG_142859", "14:28:59", "B250516-001", "24", "24", "0", "0", "V2.1.0", "OK" }
            };
            recordGrid = UiFactory.Grid(new[] { "序号", "图片", "时间", "批次", "产品", "OK", "NG", "ERR", "Recipe", "状态" }, records);
            body.Controls.Add(UiFactory.Card("检测记录列表", recordGrid), 0, 0);

            previewCanvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(115, 115, 115),
                Margin = new Padding(10)
            };
            previewCanvas.Paint += PaintPreview;
            body.Controls.Add(UiFactory.Card("选中记录预览", previewCanvas), 1, 0);

            body.Controls.Add(UiFactory.Card("检测详情", UiFactory.KeyValues(new[,] {
                { "原图：", "IMG_20250516_143018.png" }, { "产品实例：", "21 / 24" }, { "Mask：", "mask_..._21.png" },
                { "缺陷实例：", "2 个" }, { "类别：", "划伤、缺口" }, { "概率：", "0.92、0.87" }, { "最终状态：", "NG" },
                { "Recipe：", "V2.1.0" }, { "模型版本：", "Model_A_2.1.2" }, { "操作员：", "operator_01" }
            }, 42)), 2, 0);
            return body;
        }

        private void PaintPreview(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null) return;
            int size = Math.Max(80, Math.Min(panel.Width, panel.Height) - 70);
            Rectangle r = new Rectangle((panel.Width - size) / 2, (panel.Height - size) / 2, size, size);
            using (Pen p = new Pen(Color.White, 2F)) e.Graphics.DrawEllipse(p, r);
            using (Pen p = new Pen(Color.Black, 3F))
            {
                e.Graphics.DrawRectangle(p, r.X + 25, r.Y + 30, Math.Min(95, Math.Max(30, r.Width / 3)), 60);
                e.Graphics.DrawRectangle(p, Math.Max(r.X + 30, r.Right - 125), r.Y + 100, Math.Min(90, Math.Max(30, r.Width / 3)), 65);
            }
            e.Graphics.DrawString("NG_01 划痕 0.92", UiTheme.Font(8F, true), Brushes.Black, r.X + 28, r.Y + 34);
            e.Graphics.DrawString("NG_02 缺口 0.87", UiTheme.Font(8F, true), Brushes.Black, Math.Max(r.X + 30, r.Right - 120), r.Y + 104);
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
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            bottom.Controls.Add(UiFactory.Card("当前图片缺陷列表", UiFactory.Grid(
                new[] { "序号", "类别", "位置", "尺寸", "概率", "规则", "状态" },
                new[] {
                    new[] { "1", "划伤", "842,312", "192×34", "0.92", "Rule_Scratch_v2.1", "NG" },
                    new[] { "2", "缺口", "1563,1024", "87×91", "0.87", "Rule_Notch_v1.3", "NG" }
                })), 0, 0);

            auditLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Font = UiTheme.Font(8.4F, false),
                Text = "14:30:18  检测完成  operator_01  结果：NG（2个缺陷）\r\n14:30:20  结果确认  operator_01  确认正确\r\n14:31:05  标记为需复检  qc_lead\r\n14:35:22  人工复检完成  qc_lead  结果：NG\r\n14:36:01  归档  system"
            };
            bottom.Controls.Add(UiFactory.Card("追溯日志 / 审计轨迹", auditLog), 1, 0);

            FlowLayoutPanel exports = UiFactory.Toolbar("导出详情PDF", "导出记录CSV", "导出缺陷ZIP", "导出批次CSV", "打印当前详情");
            exports.FlowDirection = FlowDirection.TopDown;
            exports.WrapContents = false;
            exports.Padding = new Padding(12, 8, 0, 0);
            bottom.Controls.Add(UiFactory.Card("导出与操作", exports), 2, 0);
            return bottom;
        }
    }
}
