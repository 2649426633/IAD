using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IAD
{
    internal static class DashboardUiBuilder
    {
        private static readonly Color PageBackground = Color.FromArgb(248, 248, 248);
        private static readonly Color PanelBackground = Color.White;
        private static readonly Color Border = Color.FromArgb(214, 214, 214);
        private static readonly Color SoftBorder = Color.FromArgb(230, 230, 230);
        private static readonly Color HeaderGray = Color.FromArgb(242, 242, 242);
        private static readonly Color ActiveGray = Color.FromArgb(210, 210, 210);
        private static readonly Color Dark = Color.FromArgb(28, 28, 28);
        private static readonly Color Muted = Color.FromArgb(92, 92, 92);

        public static void Build(Form form)
        {
            ConfigureWindow(form);
            form.SuspendLayout();
            form.Controls.Clear();

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.BackColor = PageBackground;
            root.Margin = Padding.Empty;
            root.Padding = Padding.Empty;
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            root.Controls.Add(BuildHeader(form), 0, 0);
            root.Controls.Add(BuildBody(), 0, 1);
            root.Controls.Add(BuildStatusBar(), 0, 2);

            form.Controls.Add(root);
            form.ResumeLayout(true);
        }

        private static void ConfigureWindow(Form form)
        {
            form.Text = "通用工业瑕疵质检系统";
            form.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            form.BackColor = PageBackground;
            form.FormBorderStyle = FormBorderStyle.None;
            form.StartPosition = FormStartPosition.Manual;
            form.Bounds = Screen.PrimaryScreen.Bounds;
            form.WindowState = FormWindowState.Maximized;
            form.MinimumSize = Screen.PrimaryScreen.Bounds.Size;
            form.MaximumSize = Screen.PrimaryScreen.Bounds.Size;
            form.KeyPreview = true;
            form.SizeChanged += delegate
            {
                if (form.WindowState != FormWindowState.Maximized)
                {
                    form.WindowState = FormWindowState.Maximized;
                }
            };
        }

        private static Control BuildHeader(Form form)
        {
            TableLayoutPanel header = new TableLayoutPanel();
            header.Dock = DockStyle.Fill;
            header.BackColor = Color.FromArgb(246, 246, 246);
            header.Margin = Padding.Empty;
            header.Padding = Padding.Empty;
            header.ColumnCount = 4;
            header.RowCount = 1;
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));

            Label title = new Label();
            title.Dock = DockStyle.Fill;
            title.Text = "通用工业瑕疵质检系统";
            title.Font = new Font("Microsoft YaHei UI", 17F, FontStyle.Bold);
            title.ForeColor = Dark;
            title.TextAlign = ContentAlignment.MiddleLeft;
            title.Padding = new Padding(22, 0, 0, 0);

            Label project = new Label();
            project.Dock = DockStyle.Fill;
            project.Text = "项目： 单项目产线A";
            project.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular);
            project.ForeColor = Dark;
            project.TextAlign = ContentAlignment.MiddleLeft;
            project.Padding = new Padding(8, 2, 0, 0);

            Button closeButton = new Button();
            closeButton.Dock = DockStyle.Fill;
            closeButton.Text = "×";
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.BackColor = Color.FromArgb(246, 246, 246);
            closeButton.ForeColor = Dark;
            closeButton.Font = new Font("Segoe UI", 20F, FontStyle.Regular);
            closeButton.TabStop = false;
            closeButton.Margin = Padding.Empty;
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 225, 225);
            closeButton.Click += delegate { form.Close(); };

            header.Controls.Add(title, 0, 0);
            header.Controls.Add(project, 1, 0);
            header.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }, 2, 0);
            header.Controls.Add(closeButton, 3, 0);
            header.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(Color.FromArgb(190, 190, 190)))
                {
                    e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
                }
            };

            return header;
        }

        private static Control BuildBody()
        {
            TableLayoutPanel body = new TableLayoutPanel();
            body.Dock = DockStyle.Fill;
            body.BackColor = PageBackground;
            body.Margin = Padding.Empty;
            body.Padding = Padding.Empty;
            body.ColumnCount = 2;
            body.RowCount = 1;
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 245F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            body.Controls.Add(BuildSidebar(), 0, 0);
            body.Controls.Add(BuildDashboard(), 1, 0);
            return body;
        }

        private static Control BuildSidebar()
        {
            Panel sidebar = new Panel();
            sidebar.Dock = DockStyle.Fill;
            sidebar.BackColor = Color.FromArgb(250, 250, 250);
            sidebar.Margin = Padding.Empty;
            sidebar.Padding = new Padding(8, 18, 8, 10);
            sidebar.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(Color.FromArgb(205, 205, 205)))
                {
                    e.Graphics.DrawLine(pen, sidebar.Width - 1, 0, sidebar.Width - 1, sidebar.Height);
                }
            };

            TableLayoutPanel menu = new TableLayoutPanel();
            menu.Dock = DockStyle.Top;
            menu.BackColor = Color.Transparent;
            menu.ColumnCount = 1;
            menu.RowCount = 9;
            menu.Height = 558;
            menu.Margin = Padding.Empty;
            menu.Padding = Padding.Empty;
            menu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 9; i++)
            {
                menu.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            }

            string[] items = new string[]
            {
                "工作台", "产品定义", "数据集标注", "瑕疵模板识别", "训练与模型",
                "规则与Recipe", "在线检测", "结果追溯", "系统设置"
            };

            for (int i = 0; i < items.Length; i++)
            {
                menu.Controls.Add(CreateNavigationButton(items[i], i == 0), 0, i);
            }

            sidebar.Controls.Add(menu);
            return sidebar;
        }

        private static Button CreateNavigationButton(string text, bool active)
        {
            Button button = new Button();
            button.Dock = DockStyle.Fill;
            button.Text = text;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Font = new Font("Microsoft YaHei UI", 12F, active ? FontStyle.Bold : FontStyle.Regular);
            button.ForeColor = Dark;
            button.BackColor = active ? ActiveGray : Color.Transparent;
            button.FlatStyle = FlatStyle.Flat;
            button.Margin = new Padding(0, 0, 0, 6);
            button.Padding = new Padding(20, 0, 0, 0);
            button.TabStop = false;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = active ? ActiveGray : Color.FromArgb(235, 235, 235);
            return button;
        }

        private static Control BuildDashboard()
        {
            TableLayoutPanel dashboard = new TableLayoutPanel();
            dashboard.Dock = DockStyle.Fill;
            dashboard.BackColor = PageBackground;
            dashboard.Margin = Padding.Empty;
            dashboard.Padding = new Padding(16, 16, 16, 12);
            dashboard.ColumnCount = 1;
            dashboard.RowCount = 3;
            dashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 46F));

            dashboard.Controls.Add(BuildTopArea(), 0, 0);
            dashboard.Controls.Add(BuildKpiArea(), 0, 1);
            dashboard.Controls.Add(BuildBottomArea(), 0, 2);
            return dashboard;
        }

        private static Control BuildTopArea()
        {
            TableLayoutPanel top = new TableLayoutPanel();
            top.Dock = DockStyle.Fill;
            top.BackColor = Color.Transparent;
            top.ColumnCount = 2;
            top.RowCount = 1;
            top.Margin = new Padding(0, 0, 0, 10);
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));
            top.Controls.Add(BuildProjectOverview(), 0, 0);
            top.Controls.Add(BuildProductionFlow(), 1, 0);
            return top;
        }

        private static Control BuildProjectOverview()
        {
            TableLayoutPanel content = new TableLayoutPanel();
            content.Dock = DockStyle.Fill;
            content.BackColor = PanelBackground;
            content.ColumnCount = 2;
            content.RowCount = 7;
            content.Padding = new Padding(12, 6, 12, 10);
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            for (int i = 0; i < 7; i++)
            {
                content.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28F));
            }

            string[,] data = new string[,]
            {
                { "项目名称：", "单项目产线A" },
                { "产品类型：", "电子组件" },
                { "产线名称：", "产线A" },
                { "当前阶段：", "在线检测" },
                { "项目负责人：", "张工" },
                { "创建时间：", "2025-03-01 09:30:00" },
                { "备注：", "-" }
            };

            for (int i = 0; i < data.GetLength(0); i++)
            {
                content.Controls.Add(CreateFieldLabel(data[i, 0], true), 0, i);
                content.Controls.Add(CreateFieldLabel(data[i, 1], false), 1, i);
            }
            return CreateSection("项目概况", content);
        }

        private static Control BuildProductionFlow()
        {
            TableLayoutPanel flow = new TableLayoutPanel();
            flow.Dock = DockStyle.Fill;
            flow.BackColor = PanelBackground;
            flow.ColumnCount = 15;
            flow.RowCount = 1;
            flow.Padding = new Padding(12, 12, 12, 10);
            for (int i = 0; i < 15; i++)
            {
                flow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, i % 2 == 0 ? 10.5F : 2.2857F));
            }

            string[] names = new string[] { "产品模板建立", "标注", "少样本扩标", "数据质检", "训练", "验收", "发布", "检测" };
            string[] statuses = new string[] { "已完成", "已完成", "已完成", "已完成", "进行中", "待开始", "待开始", "待开始" };
            string[] dates = new string[] { "2025-03-01", "2025-03-02", "2025-03-03", "2025-03-04", "当前阶段", "", "", "" };

            for (int i = 0; i < names.Length; i++)
            {
                flow.Controls.Add(CreateProcessStep(i + 1, names[i], statuses[i], dates[i], i > 4, i == 4), i * 2, 0);
                if (i < names.Length - 1)
                {
                    Label arrow = new Label();
                    arrow.Dock = DockStyle.Fill;
                    arrow.Text = "→";
                    arrow.Font = new Font("Segoe UI", 17F, FontStyle.Regular);
                    arrow.ForeColor = Dark;
                    arrow.TextAlign = ContentAlignment.MiddleCenter;
                    arrow.Margin = Padding.Empty;
                    flow.Controls.Add(arrow, i * 2 + 1, 0);
                }
            }
            return CreateSection("产线流程进度", flow);
        }

        private static Control CreateProcessStep(int number, string name, string status, string date, bool pending, bool active)
        {
            TableLayoutPanel wrapper = new TableLayoutPanel();
            wrapper.Dock = DockStyle.Fill;
            wrapper.BackColor = Color.Transparent;
            wrapper.ColumnCount = 1;
            wrapper.RowCount = 3;
            wrapper.Margin = new Padding(2, 0, 2, 0);
            wrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 64F));
            wrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 18F));
            wrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 18F));

            Panel box = new Panel();
            box.Dock = DockStyle.Fill;
            box.BackColor = active ? Color.FromArgb(244, 244, 244) : Color.White;
            box.Margin = new Padding(0, 0, 0, 5);
            box.Padding = new Padding(6);
            box.Paint += delegate(object sender, PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(active ? Color.FromArgb(80, 80, 80) : Color.FromArgb(175, 175, 175), active ? 1.8F : 1F))
                {
                    if (pending) pen.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, box.Width - 1), Math.Max(0, box.Height - 1));
                }
            };

            TableLayoutPanel boxLayout = new TableLayoutPanel();
            boxLayout.Dock = DockStyle.Fill;
            boxLayout.ColumnCount = 1;
            boxLayout.RowCount = 2;
            boxLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
            boxLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            boxLayout.Controls.Add(CreateCenteredLabel(number.ToString(), 11.5F, pending ? Muted : Dark, FontStyle.Regular), 0, 0);
            boxLayout.Controls.Add(CreateCenteredLabel(name, 9.5F, pending ? Muted : Dark, active ? FontStyle.Bold : FontStyle.Regular), 0, 1);
            box.Controls.Add(boxLayout);

            wrapper.Controls.Add(box, 0, 0);
            wrapper.Controls.Add(CreateCenteredLabel(status, 9.2F, pending ? Muted : Dark, active ? FontStyle.Bold : FontStyle.Regular), 0, 1);
            Label dateLabel = CreateCenteredLabel(date, 8.4F, Muted, FontStyle.Regular);
            dateLabel.TextAlign = ContentAlignment.TopCenter;
            wrapper.Controls.Add(dateLabel, 0, 2);
            return wrapper;
        }

        private static Label CreateCenteredLabel(string text, float size, Color color, FontStyle style)
        {
            Label label = new Label();
            label.Dock = DockStyle.Fill;
            label.Text = text;
            label.Font = new Font("Microsoft YaHei UI", size, style);
            label.ForeColor = color;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Margin = Padding.Empty;
            return label;
        }

        private static Control BuildKpiArea()
        {
            TableLayoutPanel area = new TableLayoutPanel();
            area.Dock = DockStyle.Fill;
            area.BackColor = Color.Transparent;
            area.ColumnCount = 5;
            area.RowCount = 1;
            area.Margin = new Padding(0, 0, 0, 10);
            for (int i = 0; i < 5; i++) area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            area.Controls.Add(CreateKpiCard("图片总数", "128,560", "较昨日   +2,340   |   +1.85%"), 0, 0);
            area.Controls.Add(CreateKpiCard("已标注图片", "98,732", "较昨日   +1,890   |   +1.95%"), 1, 0);
            area.Controls.Add(CreateKpiCard("缺陷类别数", "42", "较昨日   +0   |   0.00%"), 2, 0);
            area.Controls.Add(CreateKpiCard("缺陷实例数", "256,731", "较昨日   +4,210   |   +1.67%"), 3, 0);
            area.Controls.Add(CreateKpiCard("待确认候选数", "5,362", "较昨日   +320   |   +6.34%"), 4, 0);
            return area;
        }

        private static Control CreateKpiCard(string title, string value, string change)
        {
            Panel panel = CreateBorderPanel();
            panel.Margin = new Padding(0, 0, 10, 0);
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Color.White;
            layout.ColumnCount = 1;
            layout.RowCount = 4;
            layout.Padding = new Padding(10, 8, 10, 8);
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
            layout.Controls.Add(CreateCenteredLabel(title, 11F, Dark, FontStyle.Bold), 0, 0);
            layout.Controls.Add(CreateCenteredLabel(value, 18F, Dark, FontStyle.Regular), 0, 1);
            layout.Controls.Add(CreateCenteredLabel(change, 9F, Dark, FontStyle.Regular), 0, 2);
            Label cutoff = CreateCenteredLabel("数据截止： 2025-05-16 24:00", 8.4F, Muted, FontStyle.Regular);
            cutoff.TextAlign = ContentAlignment.BottomCenter;
            layout.Controls.Add(cutoff, 0, 3);
            panel.Controls.Add(layout);
            return panel;
        }

        private static Control BuildBottomArea()
        {
            TableLayoutPanel bottom = new TableLayoutPanel();
            bottom.Dock = DockStyle.Fill;
            bottom.BackColor = Color.Transparent;
            bottom.ColumnCount = 3;
            bottom.RowCount = 1;
            bottom.Margin = Padding.Empty;
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23F));
            bottom.Controls.Add(BuildRecentTraining(), 0, 0);
            bottom.Controls.Add(BuildRecentInspection(), 1, 0);
            bottom.Controls.Add(BuildPendingItems(), 2, 0);
            return bottom;
        }

        private static Control BuildRecentTraining()
        {
            DataGridView grid = CreateGrid(new string[] { "训练批次", "模型名称", "数据集版本", "状态", "F1", "缺陷召回率", "发布时间" });
            AddGridRow(grid, "TRN-20250516-001", "Model_A_1.3.2", "V2.1.0", "完成", "0.956", "0.872", "2025-05-16 14:32:10");
            AddGridRow(grid, "TRN-20250515-002", "Model_A_1.2.2", "V2.0.9", "完成", "0.948", "0.861", "2025-05-15 18:05:21");
            AddGridRow(grid, "TRN-20250514-001", "Model_A_1.2.1", "V2.0.8", "完成", "0.939", "0.842", "2025-05-14 16:41:33");
            AddGridRow(grid, "TRN-20250513-001", "Model_A_1.2.0", "V2.0.7", "完成", "0.932", "0.831", "2025-05-13 11:20:09");
            AddGridRow(grid, "TRN-20250512-001", "Model_A_1.1.9", "V2.0.6", "完成", "0.925", "0.823", "2025-05-12 09:54:17");
            return CreateTableSection("最近训练", grid);
        }

        private static Control BuildRecentInspection()
        {
            DataGridView grid = CreateGrid(new string[] { "检测批次", "产品批次", "相机/工位", "检测数", "NG数", "NG率", "检测时间" });
            AddGridRow(grid, "IMG-20250516-143210", "20 / 3 / 1", "工位5", "12,412", "236", "1.90%", "2025-05-16 14:32:12");
            AddGridRow(grid, "IMG-20250516-141305", "21 / 2 / 1", "工位3", "12,080", "218", "1.80%", "2025-05-16 14:13:08");
            AddGridRow(grid, "IMG-20250516-140210", "22 / 1 / 1", "工位2", "11,948", "205", "1.71%", "2025-05-16 14:03:05");
            AddGridRow(grid, "IMG-20250516-142856", "24 / 0 / 0", "工位0", "12,603", "249", "1.98%", "2025-05-16 14:28:59");
            AddGridRow(grid, "IMG-20250516-142752", "21 / 2 / 1", "工位4", "12,331", "231", "1.87%", "2025-05-16 14:27:54");
            return CreateTableSection("最近检测", grid);
        }

        private static Control CreateTableSection(string title, DataGridView grid)
        {
            TableLayoutPanel content = new TableLayoutPanel();
            content.Dock = DockStyle.Fill;
            content.BackColor = Color.White;
            content.ColumnCount = 1;
            content.RowCount = 2;
            content.Padding = new Padding(0, 0, 0, 8);
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            content.Controls.Add(grid, 0, 0);

            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.BackColor = Color.White;
            buttonPanel.Padding = new Padding(10, 7, 0, 0);
            Button more = CreateTextButton("查看更多...", 104);
            more.Dock = DockStyle.Left;
            buttonPanel.Controls.Add(more);
            content.Controls.Add(buttonPanel, 0, 1);
            return CreateSection(title, content);
        }

        private static Control BuildPendingItems()
        {
            TableLayoutPanel content = new TableLayoutPanel();
            content.Dock = DockStyle.Fill;
            content.BackColor = Color.White;
            content.ColumnCount = 1;
            content.RowCount = 7;
            content.Padding = new Padding(10, 8, 10, 10);
            for (int i = 0; i < 6; i++) content.RowStyles.Add(new RowStyle(SizeType.Percent, 15.2F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 8.8F));

            string[,] items = new string[,]
            {
                { "待确认候选", "5,362" },
                { "待人工确认标注", "3,128" },
                { "待验收模型", "2" },
                { "待复核检测结果", "7,812" },
                { "待发布模型", "1" },
                { "异常检测告警", "3" }
            };

            for (int i = 0; i < items.GetLength(0); i++) content.Controls.Add(CreatePendingRow(items[i, 0], items[i, 1]), 0, i);
            Panel buttonHolder = new Panel();
            buttonHolder.Dock = DockStyle.Fill;
            buttonHolder.BackColor = Color.White;
            buttonHolder.Padding = new Padding(0, 5, 0, 0);
            Button detail = CreateTextButton("查看详情", 96);
            detail.Dock = DockStyle.Left;
            buttonHolder.Controls.Add(detail);
            content.Controls.Add(buttonHolder, 0, 6);
            return CreateSection("待处理事项", content);
        }

        private static Control CreatePendingRow(string name, string count)
        {
            TableLayoutPanel row = new TableLayoutPanel();
            row.Dock = DockStyle.Fill;
            row.BackColor = Color.FromArgb(252, 252, 252);
            row.ColumnCount = 2;
            row.RowCount = 1;
            row.Margin = new Padding(0, 0, 0, 6);
            row.Padding = new Padding(10, 0, 10, 0);
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            row.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(SoftBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, row.Width - 1), Math.Max(0, row.Height - 1));
                }
            };

            Label left = new Label();
            left.Dock = DockStyle.Fill;
            left.Text = name;
            left.Font = new Font("Microsoft YaHei UI", 10.2F, FontStyle.Regular);
            left.ForeColor = Dark;
            left.TextAlign = ContentAlignment.MiddleLeft;

            Label right = new Label();
            right.Dock = DockStyle.Fill;
            right.Text = count;
            right.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Regular);
            right.ForeColor = Dark;
            right.TextAlign = ContentAlignment.MiddleRight;

            row.Controls.Add(left, 0, 0);
            row.Controls.Add(right, 1, 0);
            return row;
        }

        private static Control CreateSection(string title, Control content)
        {
            Panel panel = CreateBorderPanel();
            panel.Margin = new Padding(0, 0, 10, 0);
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Color.White;
            layout.ColumnCount = 1;
            layout.RowCount = 2;
            layout.Margin = Padding.Empty;
            layout.Padding = Padding.Empty;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Label titleLabel = new Label();
            titleLabel.Dock = DockStyle.Fill;
            titleLabel.Text = title;
            titleLabel.Font = new Font("Microsoft YaHei UI", 11.5F, FontStyle.Bold);
            titleLabel.ForeColor = Dark;
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            titleLabel.Padding = new Padding(14, 2, 0, 0);
            titleLabel.BackColor = Color.White;

            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(content, 0, 1);
            panel.Controls.Add(layout);
            return panel;
        }

        private static Panel CreateBorderPanel()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.White;
            panel.Margin = Padding.Empty;
            panel.Padding = new Padding(1);
            panel.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(Border))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, panel.Width - 1), Math.Max(0, panel.Height - 1));
                }
            };
            return panel;
        }

        private static Label CreateFieldLabel(string text, bool isName)
        {
            Label label = new Label();
            label.Dock = DockStyle.Fill;
            label.Text = text;
            label.Font = new Font("Microsoft YaHei UI", 9.6F, isName ? FontStyle.Regular : FontStyle.Bold);
            label.ForeColor = isName ? Muted : Dark;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Margin = Padding.Empty;
            return label;
        }

        private static DataGridView CreateGrid(string[] columns)
        {
            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToResizeColumns = false;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeight = 32;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.RowTemplate.Height = 31;
            grid.GridColor = SoftBorder;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.ScrollBars = ScrollBars.Vertical;

            grid.ColumnHeadersDefaultCellStyle.BackColor = HeaderGray;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Dark;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 8.6F, FontStyle.Regular);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Dark;
            grid.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 8.2F, FontStyle.Regular);
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 232, 232);
            grid.DefaultCellStyle.SelectionForeColor = Dark;

            for (int i = 0; i < columns.Length; i++)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = columns[i];
                column.Name = columns[i];
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                grid.Columns.Add(column);
            }
            return grid;
        }

        private static void AddGridRow(DataGridView grid, params object[] values)
        {
            grid.Rows.Add(values);
        }

        private static Button CreateTextButton(string text, int width)
        {
            Button button = new Button();
            button.Text = text;
            button.Width = width;
            button.Height = 30;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.White;
            button.ForeColor = Dark;
            button.Font = new Font("Microsoft YaHei UI", 9.2F, FontStyle.Regular);
            button.Cursor = Cursors.Hand;
            button.TabStop = false;
            button.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
            return button;
        }

        private static Control BuildStatusBar()
        {
            TableLayoutPanel bar = new TableLayoutPanel();
            bar.Dock = DockStyle.Fill;
            bar.BackColor = Color.FromArgb(245, 245, 245);
            bar.ColumnCount = 6;
            bar.RowCount = 1;
            bar.Padding = new Padding(18, 0, 0, 0);
            bar.Margin = Padding.Empty;
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 185F));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 185F));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            string[] values = new string[] { "CPU/GPU状态", "HALCON Runtime", "ONNX Runtime", "SQLite", "离线模式" };
            for (int i = 0; i < values.Length; i++)
            {
                Label label = new Label();
                label.Dock = DockStyle.Fill;
                label.Text = i < values.Length - 1 ? values[i] + "   |" : values[i];
                label.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
                label.ForeColor = Dark;
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.Margin = Padding.Empty;
                bar.Controls.Add(label, i, 0);
            }

            bar.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(Color.FromArgb(205, 205, 205)))
                {
                    e.Graphics.DrawLine(pen, 0, 0, bar.Width, 0);
                }
            };
            return bar;
        }
    }
}
