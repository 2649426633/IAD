using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using IAD.Pages;
using IAD.UI;

namespace IAD.Shell
{
    public partial class MainForm : Form
    {
        private readonly Dictionary<string, UserControl> pages = new Dictionary<string, UserControl>();
        private readonly Dictionary<string, Button> navButtons = new Dictionary<string, Button>();
        private string currentPage;

        public MainForm()
        {
            InitializeComponent();
            ApplyShellStyle();
            RegisterPages();
            BuildNavigation();
            ShowPage("工作台");
        }

        private void ApplyShellStyle()
        {
            BackColor = UiTheme.Page;
            rootLayout.BackColor = UiTheme.Page;
            headerLayout.BackColor = UiTheme.Header;
            bodyLayout.BackColor = UiTheme.Page;
            navigationPanel.BackColor = Color.FromArgb(249, 249, 249);
            contentHost.BackColor = UiTheme.Page;
            statusLabel.BackColor = UiTheme.Header;
            statusLabel.ForeColor = UiTheme.Text;
            statusLabel.Font = UiTheme.Font(8.6F, false);
            titleLabel.Font = UiTheme.Font(16F, true);
            titleLabel.ForeColor = UiTheme.Text;
            projectLabel.Font = UiTheme.Font(10.5F, false);
            projectLabel.ForeColor = UiTheme.Text;
            closeButton.BackColor = UiTheme.Header;
            closeButton.ForeColor = UiTheme.Text;
            closeButton.Font = new Font("Segoe UI", 17F, FontStyle.Regular, GraphicsUnit.Point);
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 225, 225);
            SizeChanged += delegate(object sender, EventArgs e)
            {
                if (WindowState != FormWindowState.Maximized) WindowState = FormWindowState.Maximized;
            };
        }

        private void RegisterPages()
        {
            pages.Add("工作台", new DashboardPage());
            pages.Add("产品定义", new ProductDefinitionPage());
            pages.Add("数据集标注", new DatasetAnnotationPage());
            pages.Add("瑕疵模板识别", new TemplateRecognitionPage());
            pages.Add("训练与模型", new TrainingModelsPage());
            pages.Add("规则与Recipe", new RulesRecipePage());
            pages.Add("在线检测", new OnlineInspectionPage());
            pages.Add("结果追溯", new TraceabilityPage());
            pages.Add("系统设置", new SystemSettingsPage());
        }

        private void BuildNavigation()
        {
            string[] names = { "工作台", "产品定义", "数据集标注", "瑕疵模板识别", "训练与模型", "规则与Recipe", "在线检测", "结果追溯", "系统设置" };
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                Button button = new Button
                {
                    Text = name,
                    Width = 204,
                    Height = 52,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(20, 0, 0, 0),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = UiTheme.Text,
                    Font = UiTheme.Font(10.8F, false),
                    Margin = new Padding(0, 0, 0, 5),
                    TabStop = false,
                    Tag = name
                };
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 235, 235);
                button.Click += navigationButton_Click;
                navigationPanel.Controls.Add(button);
                navButtons.Add(name, button);
            }
        }

        private void navigationButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button != null) ShowPage(button.Tag.ToString());
        }

        private void ShowPage(string pageName)
        {
            if (!pages.ContainsKey(pageName)) return;
            currentPage = pageName;
            contentHost.SuspendLayout();
            contentHost.Controls.Clear();
            UserControl page = pages[pageName];
            page.Dock = DockStyle.Fill;
            contentHost.Controls.Add(page);
            contentHost.ResumeLayout(true);

            foreach (KeyValuePair<string, Button> pair in navButtons)
            {
                bool active = pair.Key == pageName;
                pair.Value.BackColor = active ? UiTheme.Active : Color.Transparent;
                pair.Value.Font = UiTheme.Font(10.8F, active);
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
