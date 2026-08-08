using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class TrainingModelsPage : UserControl
    {
        public TrainingModelsPage()
        {
            InitializeComponent();
            BuildView();
            BindEvents();
        }

        private void BindEvents()
        {
            // 训练与模型页功能事件统一放在此处。
            // 后续接入：训练任务提交、进度刷新、验收评估、模型发布/停用/回滚/导出。
        }

        public void RefreshTrainingJobs()
        {
            // TODO: 从训练服务读取任务与模型状态。
        }
    }
}
