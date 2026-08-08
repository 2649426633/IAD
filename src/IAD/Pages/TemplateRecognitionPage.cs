using System.ComponentModel;
using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class TemplateRecognitionPage : UserControl
    {
        public TemplateRecognitionPage()
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
            // TODO: 接入 Few-shot 检索、候选生成、AI Mask精修、确认/拒绝和Hard Negative回写。
        }

        private void LoadSampleData()
        {
            dgvPositive.Rows.Add("P-0001", "0.96");
            dgvPositive.Rows.Add("P-0002", "0.94");
            dgvPositive.Rows.Add("P-0003", "0.93");
            dgvHardNegative.Rows.Add("HN-0128", "IMG_142310", "0.68");
            dgvHardNegative.Rows.Add("HN-0127", "IMG_143105", "0.66");
            dgvCandidates.Rows.Add("1", "0.93", "IMG_142310", "待确认");
            dgvCandidates.Rows.Add("2", "0.91", "IMG_143105", "待确认");
            dgvCandidates.Rows.Add("3", "0.89", "IMG_142856", "待确认");
            dgvCandidateList.Rows.Add("1", "0.93", "IMG_142310", "待确认");
            dgvCandidateList.Rows.Add("2", "0.91", "IMG_143105", "待确认");
            dgvCandidateList.Rows.Add("3", "0.89", "IMG_142856", "待确认");
        }

        public void RefreshCandidates()
        {
            // TODO: 从少样本辅助标注服务读取候选并刷新。
        }
    }
}
