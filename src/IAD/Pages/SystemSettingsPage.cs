using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class SystemSettingsPage : UserControl
    {
        public SystemSettingsPage()
        {
            InitializeComponent();
            BuildView();
            BindEvents();
        }

        private void BindEvents()
        {
            // 系统设置页功能事件统一放在此处。
            // 后续接入：运行时检测、路径设置、离线部署、日志、备份恢复、权限与适配器配置。
        }

        public void LoadSettings()
        {
            // TODO: 从配置服务加载系统设置。
        }

        public void SaveSettings()
        {
            // TODO: 将设置写入配置服务。
        }
    }
}
