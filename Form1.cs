using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace IAD
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Visual Studio 设计器只加载 InitializeComponent，
            // 不执行运行时全屏及动态工作台构建逻辑。
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode)
            {
                return;
            }

            DashboardUiBuilder.Build(this);
        }
    }
}
