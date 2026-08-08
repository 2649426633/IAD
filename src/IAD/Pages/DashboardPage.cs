using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class DashboardPage : UserControl
    {
        public DashboardPage()
        {
            InitializeComponent();
            BindEvents();
        }

        private void BindEvents()
        {
            // 工作台功能事件统一放在此处。
            // 后续可接入统计刷新、训练状态、检测状态和待办刷新。
        }

        public void RefreshDashboard()
        {
            // TODO: 从服务层读取真实统计数据并刷新页面。
        }
    }
}
