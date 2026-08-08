using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class ProductDefinitionPage : UserControl
    {
        private bool runtimeInitialized;

        public ProductDefinitionPage()
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
            btnSave.Click += delegate { SaveProductDefinition(); };
            btnTestLocalization.Click += delegate { lblLastScore.Text = "最近测试：Score 0.92"; };
        }

        private void LoadSampleData()
        {
            dgvDefects.Rows.Clear();
            dgvDefects.Rows.Add("1", "划痕", "表面缺陷", "Multi-label Segmentation", "0.80", "30", "15", "启用");
            dgvDefects.Rows.Add("2", "脏污", "表面缺陷", "Multi-label Segmentation", "0.75", "50", "-", "启用");
            dgvDefects.Rows.Add("3", "凹坑", "表面缺陷", "Multi-label Segmentation", "0.82", "40", "-", "启用");
            dgvDefects.Rows.Add("4", "孔洞", "结构缺陷", "Multi-label Segmentation", "0.90", "20", "-", "启用");
            dgvDefects.Rows.Add("5", "缺边", "结构缺陷", "Multi-label Segmentation", "0.90", "-", "25", "启用");
            dgvDefects.Rows.Add("6", "异物", "表面缺陷", "Multi-label Segmentation", "0.78", "20", "-", "启用");
        }

        public void LoadProductDefinition()
        {
            // TODO: 从服务层加载产品、定位模板、标定与缺陷类别。
        }

        public void SaveProductDefinition()
        {
            // TODO: 将页面配置提交到产品定义服务并生成版本。
        }
    }
}
