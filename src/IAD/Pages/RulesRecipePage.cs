using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class RulesRecipePage : UserControl
    {
        public RulesRecipePage()
        {
            InitializeComponent();
            BuildView();
            BindEvents();
        }

        private void BindEvents()
        {
            // 规则与Recipe页功能事件统一放在此处。
            // 后续接入：规则编辑、区域配置、阈值策略、Recipe保存/发布/回滚与验收评估。
        }

        public void LoadRecipe()
        {
            // TODO: 从服务层加载当前Recipe和规则版本。
        }
    }
}
