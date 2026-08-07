using System.Drawing;
using System.Windows.Forms;
using IAD.UI;

namespace IAD.Pages
{
    public class DashboardPage : UserControl
    {
        public DashboardPage()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Page;
            Padding = new Padding(14, 14, 4, 10);
            Build();
        }

        private void Build()
        {
            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 205F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 155F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.Controls.Add(BuildTop(), 0, 0);
            root.Controls.Add(BuildStats(), 0, 1);
            root.Controls.Add(BuildBottom(), 0, 2);
            Controls.Add(root);
        }

        private Control BuildTop()
        {
            TableLayoutPanel top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = UiTheme.Page };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310F));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            string[,] overview = {
                { "项目名称：", "单项目产线A" }, { "产品类型：", "电子组件" }, { "产线名称：", "产线A" },
                { "当前阶段：", "在线检测" }, { "项目负责人：", "张工" }, { "创建时间：", "2025-03-01 09:30:00" }, { "备注：", "-" }
            };
            top.Controls.Add(UiFactory.Card("项目概况", UiFactory.KeyValues(overview, 42)), 0, 0);
            top.Controls.Add(UiFactory.Card("产线流程进度", BuildProcess()), 1, 0);
            return top;
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
                Padding = new Padding(10, 18, 0, 10)
            };
            string[] names = { "产品模板建立", "标注", "少样本扩标", "数据质检", "训练", "验收", "发布", "检测" };
            string[] states = { "已完成", "已完成", "已完成", "已完成", "进行中", "待开始", "待开始", "待开始" };
            for (int i = 0; i < names.Length; i++)
            {
                Panel step = new Panel { Width = 82, Height = 115, BackColor = i == 4 ? Color.FromArgb(242, 242, 242) : UiTheme.Surface, Margin = new Padding(0) };
                step.Paint += delegate(object sender, PaintEventArgs e)
                {
                    Panel p = (Panel)sender;
                    using (Pen pen = new Pen(Color.FromArgb(165, 165, 165))) e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, 72);
                };
                Label n = UiFactory.Label((i + 1).ToString(), 10.5F, false, ContentAlignment.TopCenter); n.Height = 28; n.Dock = DockStyle.Top;
                Label name = UiFactory.Label(names[i], 8.4F, i == 4, ContentAlignment.MiddleCenter); name.Height = 44; name.Dock = DockStyle.Top;
                Label state = UiFactory.Label(states[i], 8.2F, i == 4, ContentAlignment.BottomCenter); state.Dock = DockStyle.Fill; state.ForeColor = UiTheme.Muted;
                step.Controls.Add(state); step.Controls.Add(name); step.Controls.Add(n);
                flow.Controls.Add(step);
                if (i < names.Length - 1)
                {
                    Label arrow = UiFactory.Label("→", 13F, false, ContentAlignment.MiddleCenter); arrow.Width = 18; arrow.Height = 82; arrow.Margin = Padding.Empty;
                    flow.Controls.Add(arrow);
                }
            }
            return flow;
        }

        private Control BuildStats()
        {
            TableLayoutPanel stats = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1, BackColor = UiTheme.Page, Margin = Padding.Empty };
            for (int i = 0; i < 5; i++) stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            stats.Controls.Add(UiFactory.Stat("图片总数", "128,560", "较昨日  +2,340  |  +1.85%"), 0, 0);
            stats.Controls.Add(UiFactory.Stat("已标注图片", "98,732", "较昨日  +1,890  |  +1.95%"), 1, 0);
            stats.Controls.Add(UiFactory.Stat("缺陷类别数", "42", "较昨日  +0  |  0.00%"), 2, 0);
            stats.Controls.Add(UiFactory.Stat("缺陷实例数", "256,731", "较昨日  +4,210  |  +1.67%"), 3, 0);
            stats.Controls.Add(UiFactory.Stat("待确认候选数", "5,362", "较昨日  +320  |  +6.34%"), 4, 0);
            return stats;
        }

        private Control BuildBottom()
        {
            TableLayoutPanel bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = UiTheme.Page, Margin = Padding.Empty };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            string[][] training = {
                new[] { "TRN-20250516-001", "Model_A_1.3.2", "V2.1.0", "完成", "0.956", "0.872", "2025-05-16" },
                new[] { "TRN-20250515-002", "Model_A_1.2.2", "V2.0.9", "完成", "0.948", "0.861", "2025-05-15" },
                new[] { "TRN-20250514-001", "Model_A_1.2.1", "V2.0.8", "完成", "0.939", "0.842", "2025-05-14" },
                new[] { "TRN-20250513-001", "Model_A_1.2.0", "V2.0.7", "完成", "0.932", "0.831", "2025-05-13" }
            };
            string[][] inspection = {
                new[] { "IMG-143210", "20/3/1", "工位5", "12,412", "236", "1.90%", "14:32:12" },
                new[] { "IMG-141305", "21/2/1", "工位3", "12,080", "218", "1.80%", "14:13:08" },
                new[] { "IMG-140210", "22/1/1", "工位2", "11,948", "205", "1.71%", "14:03:05" },
                new[] { "IMG-142856", "24/0/0", "工位0", "12,603", "249", "1.98%", "14:28:59" }
            };
            bottom.Controls.Add(UiFactory.Card("最近训练", UiFactory.Grid(new[] { "训练批次", "模型", "数据集", "状态", "F1", "召回率", "日期" }, training)), 0, 0);
            bottom.Controls.Add(UiFactory.Card("最近检测", UiFactory.Grid(new[] { "检测批次", "产品批次", "工位", "检测数", "NG数", "NG率", "时间" }, inspection)), 1, 0);
            FlowLayoutPanel pending = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = UiTheme.Surface, Padding = new Padding(10, 6, 10, 4) };
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
