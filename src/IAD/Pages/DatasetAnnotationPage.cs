using System.ComponentModel;
using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class DatasetAnnotationPage : UserControl
    {
        public DatasetAnnotationPage()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                LoadSampleData();
                BindEvents();
            }
        }

        private void BindEvents()
        {
            // TODO: 接入图片导入、Rectangle、Polygon、Brush、Mask修改和版本管理事件。
        }

        private void LoadSampleData()
        {
            dgvImages.Rows.Add("IMG_142310.png", "已标注");
            dgvImages.Rows.Add("IMG_143105.png", "已标注");
            dgvImages.Rows.Add("IMG_143701.png", "部分标注");
            dgvImages.Rows.Add("IMG_144203.png", "未标注");
            dgvClasses.Rows.Add("1", "划痕", "Rectangle");
            dgvClasses.Rows.Add("2", "缺料", "Polygon");
            dgvClasses.Rows.Add("3", "裂纹", "Polygon");
            dgvClasses.Rows.Add("4", "脏污", "Brush / Mask");
            dgvLayers.Rows.Add("划痕", "4", "是");
            dgvLayers.Rows.Add("缺料", "1", "是");
            dgvLayers.Rows.Add("裂纹", "1", "是");
            dgvLayers.Rows.Add("脏污", "1", "是");
            for (int i = 1; i <= 10; i++)
            {
                dgvQueue.Rows.Add(i.ToString("0000"), "IMG_" + i.ToString("0000") + ".png", i % 4 == 0 ? "未标注" : "已标注", i % 4 == 0 ? "0" : "2");
            }
        }

        public void LoadDataset()
        {
            // TODO: 从数据集服务加载图片、类别、标注和版本信息。
        }
    }
}
