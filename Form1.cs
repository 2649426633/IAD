using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IAD
{
    public partial class Form1 : Form
    {
        private readonly Color _pageBackground = Color.FromArgb(248, 248, 248);
        private readonly Color _panelBackground = Color.White;
        private readonly Color _border = Color.FromArgb(214, 214, 214);
        private readonly Color _softBorder = Color.FromArgb(230, 230, 230);
        private readonly Color _headerGray = Color.FromArgb(242, 242, 242);
        private readonly Color _activeGray = Color.FromArgb(210, 210, 210);
        private readonly Color _dark = Color.FromArgb(28, 28, 28);
        private readonly Color _muted = Color.FromArgb(92, 92, 92);

        public Form1()
        {
            InitializeComponent();
            ConfigureWindow();
            BuildInterface();
        }

        private void ConfigureWindow()
        {
            Text = "通用工业瑕疵质检系统";
            Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = _pageBackground;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Bounds = Screen.PrimaryScreen.Bounds;
            WindowState = FormWindowState.Maximized;
            MinimumSize = Screen.PrimaryScreen.Bounds.Size;
            MaximumSize = Screen.PrimaryScreen.Bounds.Size;
            KeyPreview = true;

            SizeChanged += (sender, args) =>
            {
                if (WindowState != FormWindowState.Maximized)
                {
                    WindowState = FormWindowState.Maximized;
                }
            };
        }

        private void BuildInterface()
        {
            SuspendLayout();
            Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = _pageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                ColumnCount = 1,
                RowCount = 3
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildBody(), 0, 1);
            root.Controls.Add(BuildStatusBar(), 0, 2);

            Controls.Add(root);
            ResumeLayout(true);
        }

        private Control BuildHeader()
        {
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(246, 246, 246),
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                ColumnCount = 4,
                RowCount = 1
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));

            var title = new Label
            {
                Dock = DockStyle.Fill,
                Text = "通用工业瑕疵质检系统",
                Font = new Font("Microsoft YaHei UI", 17F, FontStyle.Bold),
                ForeColor = _dark,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(22, 0, 0, 0)
            };

            var project = new Label
            {
                Dock = DockStyle.Fill,
                Text = "项目： 单项目产线A",
                Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular),
                ForeColor = _dark,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 2, 0, 0)
            };

            var closeButton = new Button
            {
                Dock = DockStyle.Fill,
                Text = "×",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(246, 246, 246),
                ForeColor = _dark,
                Font = new Font("Segoe UI", 20F, FontStyle.Regular),
                TabStop = false,
                Margin = Padding.Empty
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 225, 225);
            closeButton.Click += (sender, args) => Close();

            header.Controls.Add(title, 0, 0);
            header.Controls.Add(project, 1, 0);
            header.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }, 2, 0);
            header.Controls.Add(closeButton, 3, 0);
            header.Paint += (sender, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(190, 190, 190)))
                {
                    e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
                }
            };

            return header;
        }

        private Control BuildBody()
        {
            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = _pageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                ColumnCount = 2,
                RowCount = 1
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 245F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            body.Controls.Add(BuildSidebar(), 0, 0);
            body.Controls.Add(BuildDashboard(), 1, 0);
            return body;
        }

        private Control BuildSidebar()
        {
            var sidebar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 250, 250),
                Margin = Padding.Empty,
                Padding = new Padding(8, 18, 8, 10)
            };
            sidebar.Paint += (sender, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(205, 205, 205)))
                {
                    e.Graphics.DrawLine(pen, sidebar.Width - 1, 0, sidebar.Width - 1, sidebar.Height);
                }
            };

            var menu = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 9,
                Height = 558,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            menu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 9; i++)
            {
                menu.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            }

            string[] items =
            {
                "工作台",
                "产品定义",
                "数据集标注",
                "瑕疵模板识别",
                "训练与模型",
                "规则与Recipe",
                "在线检测",
                "结果追溯",
                "系统设置"
            };

            for (int i = 0; i < items.Length; i++)
            {
                var button = CreateNavigationButton(items[i], i == 0);
                menu.Controls.Add(button, 0, i);
            }

            sidebar.Controls.Add(menu);
            return sidebar;
        }

        private Button CreateNavigationButton(string text, bool active)
        {
            var button = new Button
            {
                Dock = DockStyle.Fill,
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Microsoft YaHei UI", 12F, active ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = _dark,
                BackColor = active ? _activeGray : Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 6),
                Padding = new Padding(20, 0, 0, 0),
                TabStop = false
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = active ? _activeGray : Color.FromArgb(235, 235, 235);
            return button;
        }

        private Control BuildDashboard()
        {
            var dashboard = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = _pageBackground,
                Margin = Padding.Empty,
                Padding = new Padding(16, 16, 16, 12),
                ColumnCount = 1,
                RowCount = 3
            };
            dashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 46F));

            dashboard.Controls.Add(BuildTopArea(), 0, 0);
            dashboard.Controls.Add(BuildKpiArea(), 0, 1);
            dashboard.Controls.Add(BuildBottomArea(), 0, 2);
            return dashboard;
        }

        private Control BuildTopArea()
        {
            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 10)
            };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));

            top.Controls.Add(BuildProjectOverview(), 0, 0);
            top.Controls.Add(BuildProductionFlow(), 1, 0);
            return top;
        }

        private Control BuildProjectOverview()
        {
            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = _panelBackground,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(12, 6, 12, 10)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            for (int i = 0; i < 7; i++)
            {
                content.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28F));
            }

            string[,] data =
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

        private Control BuildProductionFlow()
        {
            var flow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = _panelBackground,
                ColumnCount = 15,
                RowCount = 1,
                Padding = new Padding(12, 12, 12, 10)
            };

            for (int i = 0; i < 15; i++)
            {
                flow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, i % 2 == 0 ? 10.5F : 2.2857F));
            }

            string[] names =
            {
                "产品模板建立", "标注", "少样本扩标", "数据质检",
                "训练", "验收", "发布", "检测"
            };
            string[] statuses =
            {
                "已完成", "已完成", "已完成", "已完成",
                "进行中", "待开始", "待开始", "待开始"
            };
            string[] dates =
            {
                "2025-03-01", "2025-03-02", "2025-03-03", "2025-03-04",
                "当前阶段", "", "", ""
            };

            for (int i = 0; i < names.Length; i++)
            {
                bool pending = i > 4;
                bool active = i == 4;
                flow.Controls.Add(CreateProcessStep(i + 1, names[i], statuses[i], dates[i], pending, active), i * 2, 0);
                if (i < names.Length - 1)
                {
                    flow.Controls.Add(new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = "→",
                        Font = new Font("Segoe UI", 17F, FontStyle.Regular),
                        ForeColor = _dark,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Margin = Padding.Empty
                    }, i * 2 + 1, 0);
                }
            }

            return CreateSection("产线流程进度", flow);
        }

        private Control CreateProcessStep(int number, string name, string status, string date, bool pending, bool active)
        {
            var wrapper = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 3,
                Margin = new Padding(2, 0, 2, 0)
            };
            wrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 64F));
            wrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 18F));
            wrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 18F));

            var box = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = active ? Color.FromArgb(244, 244, 244) : Color.White,
                Margin = new Padding(0, 0, 0, 5),
                Padding = new Padding(6)
            };
            box.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(active ? Color.FromArgb(80, 80, 80) : Color.FromArgb(175, 175, 175), active ? 1.8F : 1F))
                {
                    if (pending)
                    {
                        pen.DashStyle = DashStyle.Dash;
                    }
                    e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, box.Width - 1), Math.Max(0, box.Height - 1));
                }
            };

            var boxLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            boxLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
            boxLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            boxLayout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = number.ToString(),
                Font = new Font("Microsoft YaHei UI", 11.5F, FontStyle.Regular),
                ForeColor = pending ? Color.FromArgb(120, 120, 120) : _dark,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = Padding.Empty
            }, 0, 0);
            boxLayout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = name,
                Font = new Font("Microsoft YaHei UI", 9.5F, active ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = pending ? Color.FromArgb(120, 120, 120) : _dark,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = Padding.Empty
            }, 0, 1);
            box.Controls.Add(boxLayout);

            wrapper.Controls.Add(box, 0, 0);
            wrapper.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = status,
                Font = new Font("Microsoft YaHei UI", 9.2F, active ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = pending ? Color.FromArgb(120, 120, 120) : _dark,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = Padding.Empty
            }, 0, 1);
            wrapper.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = date,
                Font = new Font("Microsoft YaHei UI", 8.4F, FontStyle.Regular),
                ForeColor = _muted,
                TextAlign = ContentAlignment.TopCenter,
                Margin = Padding.Empty
            }, 0, 2);

            return wrapper;
        }

        private Control BuildKpiArea()
        {
            var area = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 5,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 10)
            };
            for (int i = 0; i < 5; i++)
            {
                area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            }

            area.Controls.Add(CreateKpiCard("图片总数", "128,560", "较昨日   +2,340   |   +1.85%"), 0, 0);
            area.Controls.Add(CreateKpiCard("已标注图片", "98,732", "较昨日   +1,890   |   +1.95%"), 1, 0);
            area.Controls.Add(CreateKpiCard("缺陷类别数", "42", "较昨日   +0   |   0.00%"), 2, 0);
            area.Controls.Add(CreateKpiCard("缺陷实例数", "256,731", "较昨日   +4,210   |   +1.67%"), 3, 0);
            area.Controls.Add(CreateKpiCard("待确认候选数", "5,362", "较昨日   +320   |   +6.34%"), 4, 0);
            return area;
        }

        private Control CreateKpiCard(string title, string value, string change)
        {
            var panel = CreateBorderPanel();
            panel.Margin = new Padding(0, 0, 10, 0);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10, 8, 10, 8)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));

            layout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = title,
                Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
                ForeColor = _dark,
                TextAlign = ContentAlignment.MiddleCenter
            }, 0, 0);
            layout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = value,
                Font = new Font("Microsoft YaHei UI", 18F, FontStyle.Regular),
                ForeColor = _dark,
                TextAlign = ContentAlignment.MiddleCenter
            }, 0, 1);
            layout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = change,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular),
                ForeColor = _dark,
                TextAlign = ContentAlignment.MiddleCenter
            }, 0, 2);
            layout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "数据截止： 2025-05-16 24:00",
                Font = new Font("Microsoft YaHei UI", 8.4F, FontStyle.Regular),
                ForeColor = _muted,
                TextAlign = ContentAlignment.BottomCenter
            }, 0, 3);

            panel.Controls.Add(layout);
            return panel;
        }

        private Control BuildBottomArea()
        {
            var bottom = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 3,
                RowCount = 1,
                Margin = Padding.Empty
            };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23F));

            bottom.Controls.Add(BuildRecentTraining(), 0, 0);
            bottom.Controls.Add(BuildRecentInspection(), 1, 0);
            bottom.Controls.Add(BuildPendingItems(), 2, 0);
            return bottom;
        }

        private Control BuildRecentTraining()
        {
            var grid = CreateGrid(new[]
            {
                "训练批次", "模型名称", "数据集版本", "状态", "F1", "缺陷召回率", "发布时间"
            });
            AddGridRow(grid, "TRN-20250516-001", "Model_A_1.3.2", "V2.1.0", "完成", "0.956", "0.872", "2025-05-16 14:32:10");
            AddGridRow(grid, "TRN-20250515-002", "Model_A_1.2.2", "V2.0.9", "完成", "0.948", "0.861", "2025-05-15 18:05:21");
            AddGridRow(grid, "TRN-20250514-001", "Model_A_1.2.1", "V2.0.8", "完成", "0.939", "0.842", "2025-05-14 16:41:33");
            AddGridRow(grid, "TRN-20250513-001", "Model_A_1.2.0", "V2.0.7", "完成", "0.932", "0.831", "2025-05-13 11:20:09");
            AddGridRow(grid, "TRN-20250512-001", "Model_A_1.1.9", "V2.0.6", "完成", "0.925", "0.823", "2025-05-12 09:54:17");

            return CreateTableSection("最近训练", grid);
        }

        private Control BuildRecentInspection()
        {
            var grid = CreateGrid(new[]
            {
                "检测批次", "产品批次", "相机/工位", "检测数", "NG数", "NG率", "检测时间"
            });
            AddGridRow(grid, "IMG-20250516-143210", "20 / 3 / 1", "工位5", "12,412", "236", "1.90%", "2025-05-16 14:32:12");
            AddGridRow(grid, "IMG-20250516-141305", "21 / 2 / 1", "工位3", "12,080", "218", "1.80%", "2025-05-16 14:13:08");
            AddGridRow(grid, "IMG-20250516-140210", "22 / 1 / 1", "工位2", "11,948", "205", "1.71%", "2025-05-16 14:03:05");
            AddGridRow(grid, "IMG-20250516-142856", "24 / 0 / 0", "工位0", "12,603", "249", "1.98%", "2025-05-16 14:28:59");
            AddGridRow(grid, "IMG-20250516-142752", "21 / 2 / 1", "工位4", "12,331", "231", "1.87%", "2025-05-16 14:27:54");

            return CreateTableSection("最近检测", grid);
        }

        private Control CreateTableSection(string title, DataGridView grid)
        {
            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 0, 0, 8)
            };
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            content.Controls.Add(grid, 0, 0);

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10, 7, 0, 0)
            };
            var more = CreateTextButton("查看更多...", 104);
            more.Dock = DockStyle.Left;
            buttonPanel.Controls.Add(more);
            content.Controls.Add(buttonPanel, 0, 1);

            return CreateSection(title, content);
        }

        private Control BuildPendingItems()
        {
            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 7,
                Padding = new Padding(10, 8, 10, 10)
            };
            for (int i = 0; i < 6; i++)
            {
                content.RowStyles.Add(new RowStyle(SizeType.Percent, 15.2F));
            }
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 8.8F));

            string[,] items =
            {
                { "待确认候选", "5,362" },
                { "待人工确认标注", "3,128" },
                { "待验收模型", "2" },
                { "待复核检测结果", "7,812" },
                { "待发布模型", "1" },
                { "异常检测告警", "3" }
            };

            for (int i = 0; i < items.GetLength(0); i++)
            {
                content.Controls.Add(CreatePendingRow(items[i, 0], items[i, 1]), 0, i);
            }

            var buttonHolder = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(0, 5, 0, 0) };
            var detail = CreateTextButton("查看详情", 96);
            detail.Dock = DockStyle.Left;
            buttonHolder.Controls.Add(detail);
            content.Controls.Add(buttonHolder, 0, 6);

            return CreateSection("待处理事项", content);
        }

        private Control CreatePendingRow(string name, string count)
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(252, 252, 252),
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 6),
                Padding = new Padding(10, 0, 10, 0)
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            row.Paint += (sender, e) =>
            {
                using (var pen = new Pen(_softBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, row.Width - 1), Math.Max(0, row.Height - 1));
                }
            };
            row.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = name,
                Font = new Font("Microsoft YaHei UI", 10.2F),
                ForeColor = _dark,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            row.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = count,
                Font = new Font("Microsoft YaHei UI", 10.5F),
                ForeColor = _dark,
                TextAlign = ContentAlignment.MiddleRight
            }, 1, 0);
            return row;
        }

        private Control CreateSection(string title, Control content)
        {
            var panel = CreateBorderPanel();
            panel.Margin = new Padding(0, 0, 10, 0);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var titleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = title,
                Font = new Font("Microsoft YaHei UI", 11.5F, FontStyle.Bold),
                ForeColor = _dark,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 2, 0, 0),
                BackColor = Color.White
            };

            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(content, 0, 1);
            panel.Controls.Add(layout);
            return panel;
        }

        private Panel CreateBorderPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = Padding.Empty,
                Padding = new Padding(1)
            };
            panel.Paint += (sender, e) =>
            {
                using (var pen = new Pen(_border))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, panel.Width - 1), Math.Max(0, panel.Height - 1));
                }
            };
            return panel;
        }

        private Label CreateFieldLabel(string text, bool label)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                Font = new Font("Microsoft YaHei UI", 9.6F, label ? FontStyle.Regular : FontStyle.Bold),
                ForeColor = label ? _muted : _dark,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty
            };
        }

        private DataGridView CreateGrid(string[] columns)
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 32,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                RowTemplate = { Height = 31 },
                GridColor = _softBorder,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false,
                ScrollBars = ScrollBars.Vertical
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = _headerGray;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = _dark;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 8.6F, FontStyle.Regular);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = _dark;
            grid.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 8.2F, FontStyle.Regular);
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 232, 232);
            grid.DefaultCellStyle.SelectionForeColor = _dark;

            foreach (string column in columns)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = column,
                    Name = column,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
            }

            return grid;
        }

        private static void AddGridRow(DataGridView grid, params object[] values)
        {
            grid.Rows.Add(values);
        }

        private Button CreateTextButton(string text, int width)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = _dark,
                Font = new Font("Microsoft YaHei UI", 9.2F),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
            return button;
        }

        private Control BuildStatusBar()
        {
            var bar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 245, 245),
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(18, 0, 0, 0),
                Margin = Padding.Empty
            };
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 185F));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 185F));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            string[] values = { "CPU/GPU状态", "HALCON Runtime", "ONNX Runtime", "SQLite", "离线模式" };
            for (int i = 0; i < values.Length; i++)
            {
                var label = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = i < values.Length - 1 ? values[i] + "   |" : values[i],
                    Font = new Font("Microsoft YaHei UI", 9F),
                    ForeColor = _dark,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = Padding.Empty
                };
                bar.Controls.Add(label, i, 0);
            }

            bar.Paint += (sender, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(205, 205, 205)))
                {
                    e.Graphics.DrawLine(pen, 0, 0, bar.Width, 0);
                }
            };
            return bar;
        }
    }
}
