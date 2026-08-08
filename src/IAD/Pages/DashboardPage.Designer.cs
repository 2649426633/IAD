using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using IAD.UI;

namespace IAD.Pages
{
    public partial class DashboardPage
    {
        private IContainer components = null;

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
            this.Name = "DashboardPage";
            this.Size = new Size(1400, 820);
            this.ResumeLayout(false);
        }

        private void BuildView()
        {
            this.SuspendLayout();
            this.Controls.Clear();
            this.Dock = DockStyle.Fill;
            this.BackColor = UiTheme.Page;
            this.Padding = new Padding(14, 14, 4, 10);

            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 168F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.Controls.Add(UiFactory.Card("产线流程进度", BuildProcess()), 0, 0);
            root.Controls.Add(BuildStats(), 0, 1);
            root.Controls.Add(BuildBottom(), 0, 2);
            this.Controls.Add(root);
            this.ResumeLayout(true);
        }

        private Control BuildProcess()
        {
            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false,
                BackColor = UiTheme.Surface,
                Padding = new Padding(12, 12, 0, 8),
                Margin = Padding.Empty
            };

            string[] names = { "产品模板建立", "标注", "少样本扩标", "数据质检", "训练", "验收", "发布", "检测" };
            string[] states = { "已完成", "已完成", "已完成", "已完成", "进行中", "待开始", "待开始", "待开始" };
            string[] dates = { "2025-03-01", "2025-03-02", "2025-03-03", "2025-03-04", "当前阶段", "", "", "" };

            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                bool active = index == 4;
                bool pending = index > 4;
                Panel step = new Panel
                {
                    Width = 112,
                    Height = 102,
                    BackColor = active ? Color.FromArgb(242, 242, 242) : UiTheme.Surface,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty
                };
                step.Paint += delegate(object sender, PaintEventArgs e)
                {
                    Panel p = sender as Panel;
                    if (p == null) return;
                    using (Pen pen = new Pen(active ? Color.FromArgb(90, 90, 90) : Color.FromArgb(172, 172, 172), active ? 1.5F : 1F))
                    {
                        if (pending) pen.DashStyle = DashStyle.Dash;
                        e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, 66);
                    }
                };

                Label number = UiFactory.Label((index + 1).ToString(), 10.5F, false, ContentAlignment.MiddleCenter);
                number.Height = 25;
                number.Dock = DockStyle.Top;
                number.ForeColor = pending ? UiTheme.Muted : UiTheme.Text;

                Label name = UiFactory.Label(names[index], 8.8F, active, ContentAlignment.MiddleCenter);
                name.Height = 41;
                name.Dock = DockStyle.Top;
                name.ForeColor = pending ? UiTheme.Muted : UiTheme.Text;

                TableLayoutPanel stateArea = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    BackColor = Color.Transparent,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty
                };
                stateArea.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
                stateArea.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));

                Label state = UiFactory.Label(states[index], 8.2F, active, ContentAlignment.BottomCenter);
                state.ForeColor = pending ? UiTheme.Muted : UiTheme.Text;
                Label date = UiFactory.Label(dates[index], 7.5F, false, ContentAlignment.TopCenter);
                date.ForeColor = UiTheme.Muted;

                stateArea.Controls.Add(state, 0, 0);
                stateArea.Controls.Add(date, 0, 1);
                step.Controls.Add(stateArea);
                step.Controls.Add(name);
                step.Controls.Add(number);
                flow.Controls.Add(step);

                if (index < names.Length - 1)
                {
                    Label arrow = UiFactory.Label("→", 14F, false, ContentAlignment.MiddleCenter);
                    arrow.Width = 28;
                    arrow.Height = 68;
                    arrow.Margin = Padding.Empty;
                    flow.Controls.Add(arrow);
                }
            }
            return flow;
        }

        private Control BuildStats()
        {
            TableLayoutPanel stats = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            stats.Controls.Add(UiFactory.Stat("图片总数", "128,560"), 0, 0);
            stats.Controls.Add(UiFactory.Stat("已标注图片", "98,732"), 1, 0);
            stats.Controls.Add(UiFactory.Stat("缺陷类别数", "42"), 2, 0);
            stats.Controls.Add(UiFactory.Stat("缺陷实例数", "256,731"), 3, 0);
            stats.Controls.Add(UiFactory.Stat("待确认候选数", "5,362"), 4, 0);
            return stats;
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
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));

            string[][] training =
            {
                new[] { "TRN-20250516-001", "Model_A_1.3.2", "V2.1.0", "完成", "0.956", "0.872", "2025-05-16" },
                new[] { "TRN-20250515-002", "Model_A_1.2.2", "V2.0.9", "完成", "0.948", "0.861", "2025-05-15" },
                new[] { "TRN-20250514-001", "Model_A_1.2.1", "V2.0.8", "完成", "0.939", "0.842", "2025-05-14" },
                new[] { "TRN-20250513-001", "Model_A_1.2.0", "V2.0.7", "完成", "0.932", "0.831", "2025-05-13" }
            };
            string[][] inspection =
            {
                new[] { "IMG-143210", "20/3/1", "工位5", "12,412", "236", "1.90%", "14:32:12" },
                new[] { "IMG-141305", "21/2/1", "工位3", "12,080", "218", "1.80%", "14:13:08" },
                new[] { "IMG-140210", "22/1/1", "工位2", "11,948", "205", "1.71%", "14:03:05" },
                new[] { "IMG-142856", "24/0/0", "工位0", "12,603", "249", "1.98%", "14:28:59" }
            };

            bottom.Controls.Add(UiFactory.Card("最近训练", UiFactory.Grid(new[] { "训练批次", "模型", "数据集", "状态", "F1", "召回率", "日期" }, training)), 0, 0);
            bottom.Controls.Add(UiFactory.Card("最近检测", UiFactory.Grid(new[] { "检测批次", "产品批次", "工位", "检测数", "NG数", "NG率", "时间" }, inspection)), 1, 0);

            FlowLayoutPanel pending = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = UiTheme.Surface,
                Padding = new Padding(10, 6, 10, 4)
            };
            pending.Controls.Add(UiFactory.PendingRow("待确认候选", "5,362"));
            pending.Controls.Add(UiFactory.PendingRow("待人工确认标注", "3,128"));
            pending.Controls.Add(UiFactory.PendingRow("待验收模型", "2"));
            pending.Controls.Add(UiFactory.PendingRow("待复核检测结果", "7,812"));
            pending.Controls.Add(UiFactory.PendingRow("待发布模型", "1"));
            pending.Controls.Add(UiFactory.PendingRow("异常检测告警", "3"));
            bottom.Controls.Add(UiFactory.Card("待处理事项", pending), 2, 0);
            return bottom;
        }
    }
}
