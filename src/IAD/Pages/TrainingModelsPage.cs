using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class TrainingModelsPage : UserControl
    {
        private bool runtimeInitialized;

        public TrainingModelsPage()
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
            // TODO: 接入训练任务提交、进度刷新、验收评估、模型发布/停用/回滚/导出。
        }

        private void LoadSampleData()
        {
            dgvDatasetSplit.Rows.Add("Train", "18,732", "70%", "98,732", "70.1%");
            dgvDatasetSplit.Rows.Add("Validation", "4,027", "15%", "21,365", "15.2%");
            dgvDatasetSplit.Rows.Add("Acceptance", "4,027", "15%", "21,402", "14.7%");
            dgvTrainingQueue.Rows.Add("TRN-005", "SegFormer-B2", "V2.1.0", "14:25:32", "训练中", "高");
            dgvTrainingQueue.Rows.Add("TRN-004", "UNet", "V2.1.0", "13:48:21", "排队中", "中");
            txtTrainingLog.Text = "当前任务：TRN-20250516-005 (SegFormer-B2)\r\n进度：Epoch 38 / 100 (38%)\r\n最佳验证集 mIoU：0.7421\r\n\r\n[14:43:15] Validation mIoU 0.7421  Recall 0.7325  Precision 0.7719";
            dgvBenchmark.Rows.Add("SegFormer-B2", "0.7325", "0.7719", "0.7518", "0.7421", "0.6123", "42.3", "6.21");
            dgvBenchmark.Rows.Add("UNet", "0.7086", "0.7472", "0.7274", "0.7098", "0.5891", "31.7", "5.03");
            dgvModelLibrary.Rows.Add("V2.1.0", "SegFormer-B2", "V2.1.0", "05-16", "0.7398", "已发布", "3e6f7a9c...", "导出/停用/回滚");
            dgvModelLibrary.Rows.Add("V2.0.0", "SegFormer-B1", "V2.0.0", "05-14", "0.7216", "已停用", "7b1c2d3e...", "导出/发布/回滚");
        }

        public void RefreshTrainingJobs()
        {
            // TODO: 从训练服务读取任务、日志、指标与模型状态。
        }
    }
}
