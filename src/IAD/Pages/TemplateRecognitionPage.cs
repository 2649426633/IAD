using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class TemplateRecognitionPage : UserControl
    {
        public TemplateRecognitionPage()
        {
            InitializeComponent();
            BuildView();
            BindEvents();
        }

        private void BindEvents()
        {
            // 瑕疵模板识别页功能事件统一放在此处。
            // 后续接入：Few-shot 检索、候选生成、AI Mask精修、确认/拒绝、Hard Negative回写。
        }

        public void RefreshCandidates()
        {
            // TODO: 从服务层读取候选区域并刷新界面。
        }
    }
}
