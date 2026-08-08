using System.Windows.Forms;
using IAD.UI;

namespace IAD.Pages
{
    public partial class SystemSettingsPage : UserControl
    {
        private bool runtimeInitialized;

        public SystemSettingsPage()
        {
            InitializeComponent();
        }

        public void InitializeRuntime()
        {
            if (runtimeInitialized) return;
            runtimeInitialized = true;
            PageFillLayoutManager.Apply(this);
            LoadSampleData();
            BindEvents();
        }

        private void BindEvents()
        {
            btnSaveSettings.Click += delegate { SaveSettings(); };
        }

        private void LoadSampleData()
        {
            dgvRoles.Rows.Add("管理员", "2", "系统配置 / 全部模块");
            dgvRoles.Rows.Add("工程师", "4", "模型 / 规则 / 部分设置");
            dgvRoles.Rows.Add("操作员", "12", "检测 / 结果 / 追溯");
            dgvRoles.Rows.Add("访客", "3", "结果查看");

            dgvAdapters.Rows.Add("Camera", "预留", "GigE Vision", "192.168.1.100", "后续相机接入");
            dgvAdapters.Rows.Add("PLC", "预留", "Modbus TCP", "192.168.1.200:502", "OK/NG输出");
            dgvAdapters.Rows.Add("MES", "预留", "HTTP REST", "192.168.1.210/api", "生产系统");
            dgvAdapters.Rows.Add("Result Export", "已配置", "CSV + PNG", @"D:\InspectSys\Export", "结果导出");
        }

        public void LoadSettings()
        {
            // TODO: 从配置服务加载运行环境、路径、备份、权限和适配器设置。
        }

        public void SaveSettings()
        {
            // TODO: 将设置写入配置服务。
        }
    }
}
