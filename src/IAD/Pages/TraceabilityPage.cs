using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class TraceabilityPage : UserControl
    {
        public TraceabilityPage()
        {
            InitializeComponent();
            BuildView();
            BindEvents();
        }

        private void BindEvents()
        {
            // 结果追溯页功能事件统一放在此处。
            // 后续接入：筛选查询、记录选择、审计追踪、PDF/CSV/ZIP导出与打印。
        }

        public void QueryRecords()
        {
            // TODO: 调用追溯服务查询检测记录。
        }
    }
}
