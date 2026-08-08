using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class OnlineInspectionPage : UserControl
    {
        public OnlineInspectionPage()
        {
            InitializeComponent();
            BindEvents();
        }

        private void BindEvents()
        {
            // 在线检测页功能事件统一放在此处。
            // 后续接入：图像加载、批量检测、开始/暂停、结果导出、实时状态与告警刷新。
        }

        public void StartInspection()
        {
            // TODO: 调用检测服务启动当前Recipe检测流程。
        }

        public void StopInspection()
        {
            // TODO: 停止或暂停检测任务。
        }
    }
}
