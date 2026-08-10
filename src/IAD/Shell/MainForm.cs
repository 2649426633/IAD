using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using IAD.Pages;
using IAD.UI;

namespace IAD.Shell
{
    public partial class MainForm : Form
    {
        private readonly Dictionary<string, Func<UserControl>> pageFactories = new Dictionary<string, Func<UserControl>>();
        private readonly Dictionary<string, UserControl> pages = new Dictionary<string, UserControl>();
        private readonly Dictionary<string, Button> navButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Size> lastLayoutSizes = new Dictionary<string, Size>();
        private readonly HashSet<string> preparedPageLayouts = new HashSet<string>();
        private Font navFont;
        private Font navActiveFont;
        private string currentPage;
        private string selectedNavigationPage;
        private bool layoutInProgress;

        public MainForm()
        {
            InitializeComponent();

            // 设计器只解析壳体，不创建九个业务页面。
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            ApplyShellStyle();
            RegisterPageFactories();
            BuildNavigation();

            contentHost.SizeChanged += contentHost_SizeChanged;
            FormClosed += MainForm_FormClosed;

            ShowPage("工作台");
        }

        private void ApplyShellStyle()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
            EnableDoubleBuffer(contentHost);
            EnableDoubleBuffer(navigationPanel);

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

            navFont = UiTheme.Font(10.8F, false);
            navActiveFont = UiTheme.Font(10.8F, true);

            SizeChanged += MainForm_SizeChanged;
        }

        private void RegisterPageFactories()
        {
            pageFactories.Add("工作台", delegate { return new DashboardPage(); });
            pageFactories.Add("产品定义", delegate { return new ProductDefinitionPage(); });
            pageFactories.Add("数据集标注", delegate { return new DatasetAnnotationPage(); });
            pageFactories.Add("瑕疵模板识别", delegate { return new TemplateRecognitionPage(); });
            pageFactories.Add("训练与模型", delegate { return new TrainingModelsPage(); });
            pageFactories.Add("规则与Recipe", delegate { return new RulesRecipePage(); });
            pageFactories.Add("在线检测", delegate { return new OnlineInspectionPage(); });
            pageFactories.Add("结果追溯", delegate { return new TraceabilityPage(); });
            pageFactories.Add("系统设置", delegate { return new SystemSettingsPage(); });
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
                    Font = navFont,
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
            if (button == null || button.Tag == null) return;
            ShowPage(button.Tag.ToString());
        }

        private UserControl GetOrCreatePage(string pageName)
        {
            UserControl page;
            if (pages.TryGetValue(pageName, out page))
            {
                return page;
            }

            Func<UserControl> factory;
            if (!pageFactories.TryGetValue(pageName, out factory))
            {
                return null;
            }

            page = factory();
            page.Dock = DockStyle.Fill;
            page.Margin = Padding.Empty;
            page.Visible = false;
            contentHost.Controls.Add(page);
            pages.Add(pageName, page);
            InitializePageRuntime(page);
            return page;
        }

        private static void InitializePageRuntime(UserControl page)
        {
            DashboardPage dashboard = page as DashboardPage;
            if (dashboard != null) { dashboard.InitializeRuntime(); return; }

            ProductDefinitionPage product = page as ProductDefinitionPage;
            if (product != null) { product.InitializeRuntime(); return; }

            DatasetAnnotationPage annotation = page as DatasetAnnotationPage;
            if (annotation != null) { annotation.InitializeRuntime(); return; }

            TemplateRecognitionPage template = page as TemplateRecognitionPage;
            if (template != null) { template.InitializeRuntime(); return; }

            TrainingModelsPage training = page as TrainingModelsPage;
            if (training != null) { training.InitializeRuntime(); return; }

            RulesRecipePage rules = page as RulesRecipePage;
            if (rules != null) { rules.InitializeRuntime(); return; }

            OnlineInspectionPage inspection = page as OnlineInspectionPage;
            if (inspection != null) { inspection.InitializeRuntime(); return; }

            TraceabilityPage traceability = page as TraceabilityPage;
            if (traceability != null) { traceability.InitializeRuntime(); return; }

            SystemSettingsPage settings = page as SystemSettingsPage;
            if (settings != null) settings.InitializeRuntime();
        }

        private void ShowPage(string pageName)
        {
            if (string.IsNullOrEmpty(pageName) || layoutInProgress) return;

            UserControl visiblePage;
            if (string.Equals(currentPage, pageName, StringComparison.Ordinal) &&
                pages.TryGetValue(pageName, out visiblePage) && visiblePage.Visible)
            {
                return;
            }

            UserControl nextPage = null;
            layoutInProgress = true;
            contentHost.SuspendLayout();

            try
            {
                nextPage = GetOrCreatePage(pageName);
                if (nextPage == null) return;

                UserControl previousPage;
                if (!string.IsNullOrEmpty(currentPage) && pages.TryGetValue(currentPage, out previousPage))
                {
                    previousPage.Visible = false;
                }

                // 页面只在首次创建或宿主尺寸变化时重排。普通切页复用已经完成的布局。
                nextPage.Visible = false;
                if (preparedPageLayouts.Add(pageName)) PageFillLayoutManager.Apply(nextPage);
                FillPageToHost(nextPage);
                ApplyLayoutIfNeeded(pageName, nextPage);

                nextPage.Visible = true;
                nextPage.BringToFront();
                currentPage = pageName;
                UpdateNavigationSelection(pageName);
            }
            finally
            {
                contentHost.ResumeLayout(true);
                layoutInProgress = false;
            }
        }

        private void UpdateNavigationSelection(string pageName)
        {
            if (string.Equals(selectedNavigationPage, pageName, StringComparison.Ordinal)) return;

            Button previous;
            if (!string.IsNullOrEmpty(selectedNavigationPage) && navButtons.TryGetValue(selectedNavigationPage, out previous))
            {
                previous.BackColor = Color.Transparent;
                previous.Font = navFont;
            }

            Button current;
            if (navButtons.TryGetValue(pageName, out current))
            {
                current.BackColor = UiTheme.Active;
                current.Font = navActiveFont;
            }
            selectedNavigationPage = pageName;
        }

        private void FillPageToHost(UserControl page)
        {
            if (page == null) return;

            page.Dock = DockStyle.Fill;
            page.Margin = Padding.Empty;

            Rectangle target = contentHost.DisplayRectangle;
            if (target.Width > 0 && target.Height > 0 && page.Bounds != target)
            {
                page.Bounds = target;
            }
        }

        private bool ApplyLayoutIfNeeded(string pageName, UserControl page)
        {
            Size size = contentHost.ClientSize;
            if (size.Width <= 0 || size.Height <= 0) return false;

            Size previousSize;
            if (lastLayoutSizes.TryGetValue(pageName, out previousSize) && previousSize == size)
            {
                return false;
            }

            ResponsiveLayoutManager.Apply(page, size);
            lastLayoutSizes[pageName] = size;
            return true;
        }

        private void contentHost_SizeChanged(object sender, EventArgs e)
        {
            // 当前页面外框和内部布局同步完成，不再等待 Timer，避免“卡一下才铺满”。
            LayoutCurrentPageNow();
        }

        private void LayoutCurrentPageNow()
        {
            if (layoutInProgress || string.IsNullOrEmpty(currentPage)) return;

            UserControl page;
            if (!pages.TryGetValue(currentPage, out page)) return;

            layoutInProgress = true;
            page.SuspendLayout();
            try
            {
                FillPageToHost(page);
                ApplyLayoutIfNeeded(currentPage, page);
            }
            finally
            {
                page.ResumeLayout(true);
                layoutInProgress = false;
            }
        }

        private static void EnableDoubleBuffer(Control control)
        {
            if (control == null) return;
            PropertyInfo property = control.GetType().GetProperty(
                "DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (property != null) property.SetValue(control, true, null);
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (navFont != null) navFont.Dispose();
            if (navActiveFont != null) navActiveFont.Dispose();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
