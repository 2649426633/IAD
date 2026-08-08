using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class OnlineInspectionPage : UserControl
    {
        private bool runtimeInitialized;

        public OnlineInspectionPage()
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
            btnStart.Click += delegate { StartInspection(); };
            btnPause.Click += delegate { StopInspection(); };
        }

        private void LoadSampleData()
        {
            dgvQueue.Rows.Add("IMG_142310.png", "检测中");
            dgvQueue.Rows.Add("IMG_142311.png", "等待中");
            dgvQueue.Rows.Add("IMG_142312.png", "等待中");
            dgvResults.Rows.Add("#1", "OK", "0");
            dgvResults.Rows.Add("#2", "NG", "1");
            dgvResults.Rows.Add("#3", "OK", "0");
            dgvResults.Rows.Add("#4", "NG", "2");
            dgvResults.Rows.Add("#5", "OK", "0");
            dgvResults.Rows.Add("#6", "NG", "1");
            dgvResults.Rows.Add("#7", "ERROR", "-");
            dgvResults.Rows.Add("#8", "OK", "0");
            dgvDefects.Rows.Add("划痕", "0.96", "1824", "(1320,182)");
            dgvNgStats.Rows.Add("划痕", "186", "45.8%");
            dgvNgStats.Rows.Add("脏污", "128", "31.5%");
            dgvNgStats.Rows.Add("缺口", "73", "18.0%");
        }

        public void StartInspection()
        {
            lblDetect.Text = "运行中";
        }

        public void StopInspection()
        {
            lblDetect.Text = "已暂停";
        }
    }
}
