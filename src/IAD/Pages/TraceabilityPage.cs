using System.ComponentModel;
using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class TraceabilityPage : UserControl
    {
        public TraceabilityPage()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                LoadSampleData();
                BindEvents();
            }
        }

        private void BindEvents()
        {
            btnQuery.Click += delegate { QueryRecords(); };
        }

        private void LoadSampleData()
        {
            dgvRecords.Rows.Add("1", "IMG_143212", "14:32:12", "B250516-001", "24", "20", "3", "1", "V2.1.0", "NG");
            dgvRecords.Rows.Add("2", "IMG_143018", "14:30:18", "B250516-001", "24", "21", "2", "1", "V2.1.0", "NG");
            dgvRecords.Rows.Add("3", "IMG_143005", "14:30:05", "B250516-001", "24", "22", "1", "1", "V2.1.0", "NG");
            dgvCurrentDefects.Rows.Add("1", "划伤", "842,312", "192×34", "0.92", "Rule_Scratch_v2.1", "NG");
            dgvCurrentDefects.Rows.Add("2", "缺口", "1563,1024", "87×91", "0.87", "Rule_Notch_v1.3", "NG");
            txtAudit.Text = "14:30:18  检测完成  operator_01  结果：NG（2个缺陷）\r\n14:30:20  结果确认  operator_01  确认正确\r\n14:31:05  标记为需复检  qc_lead\r\n14:35:22  人工复检完成  qc_lead  结果：NG\r\n14:36:01  归档  system";
        }

        public void QueryRecords()
        {
            // TODO: 调用追溯服务按筛选条件查询检测记录。
        }
    }
}
