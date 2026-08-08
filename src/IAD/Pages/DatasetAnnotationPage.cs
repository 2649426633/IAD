using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class DatasetAnnotationPage : UserControl
    {
        public DatasetAnnotationPage()
        {
            InitializeComponent();
            BuildView();
            BindEvents();
        }

        private void BindEvents()
        {
            // 数据集标注页功能事件统一放在此处。
            // 后续接入：图片导入、ROI/Polygon/Brush、Mask编辑、自动修正、版本管理。
        }

        public void LoadDataset()
        {
            // TODO: 从服务层加载数据集、类别、标注与版本信息。
        }
    }
}
