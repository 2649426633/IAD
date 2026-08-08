using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class RulesRecipePage : UserControl
    {
        private bool runtimeInitialized;

        public RulesRecipePage()
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
            // TODO: 接入规则编辑、区域配置、Recipe保存/发布/回滚和验收评估。
        }

        private void LoadSampleData()
        {
            dgvRules.Rows.Add("划痕", "全局", "≥0.90", "≥8.00", "≥3.00", "≥0.20", "≤0", "NG");
            dgvRules.Rows.Add("刮伤", "全局", "≥0.85", "≥5.00", "≥2.00", "≥0.20", "≤1", "NG");
            dgvRules.Rows.Add("凹坑", "全局", "≥0.90", "≥4.00", "≥1.50", "≥0.20", "≤2", "NG");
            dgvThresholds.Rows.Add("划痕", "≥0.95", "≥10", "≥4", "≥0.20", "≤0");
            dgvThresholds.Rows.Add("刮伤", "≥0.90", "≥6", "≥2.5", "≥0.20", "≤1");
            dgvRecipeVersions.Rows.Add("1.0.0", "当前", "05-16 14:35", "Admin");
            dgvRecipeVersions.Rows.Add("0.9.2", "已发布", "05-14 10:21", "Admin");
        }

        public void LoadRecipe()
        {
            // TODO: 从规则与Recipe服务加载当前配置。
        }
    }
}
