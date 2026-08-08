using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class DashboardPage : UserControl
    {
        private bool runtimeInitialized;

        public DashboardPage()
        {
            InitializeComponent();
        }

        public void InitializeRuntime()
        {
            if (runtimeInitialized) return;
            runtimeInitialized = true;
            LoadSampleData();
            BindEvents();
        }

        private void BindEvents()
        {
            // 功能事件仅放在本文件。
        }

        private void LoadSampleData()
        {
            dgvTraining.Rows.Add("TRN-20250516-001", "Model_A_1.3.2", "V2.1.0", "完成", "0.956", "0.872", "2025-05-16");
            dgvTraining.Rows.Add("TRN-20250515-002", "Model_A_1.2.2", "V2.0.9", "完成", "0.948", "0.861", "2025-05-15");
            dgvTraining.Rows.Add("TRN-20250514-001", "Model_A_1.2.1", "V2.0.8", "完成", "0.939", "0.842", "2025-05-14");
            dgvInspection.Rows.Add("IMG-143210", "20/3/1", "工位5", "12,412", "236", "1.90%", "14:32:12");
            dgvInspection.Rows.Add("IMG-141305", "21/2/1", "工位3", "12,080", "218", "1.80%", "14:13:08");
            dgvInspection.Rows.Add("IMG-140210", "22/1/1", "工位2", "11,948", "205", "1.71%", "14:03:05");
        }

        public void RefreshDashboard()
        {
            // TODO: 从服务层读取真实统计、训练、检测和待办数据。
        }
    }
}
