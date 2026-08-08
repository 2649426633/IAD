using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class ProductDefinitionPage : UserControl
    {
        public ProductDefinitionPage()
        {
            InitializeComponent();
            BuildView();
            BindEvents();
        }

        private void BindEvents()
        {
            // 产品定义页功能事件统一放在此处。
            // 后续接入：基准图导入、ROI交互、模板建立、定位测试、类别管理、保存与版本管理。
        }

        public void LoadProductDefinition()
        {
            // TODO: 从服务层加载产品定义、定位模板、标定与缺陷类别配置。
        }

        public void SaveProductDefinition()
        {
            // TODO: 将当前页面配置提交给服务层持久化。
        }
    }
}
