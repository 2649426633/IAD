using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using IAD.Models;
using IAD.Security;
using IAD.Services;
using IAD.UI;

namespace IAD.Pages
{
    public partial class DatasetAnnotationPage : UserControl
    {
        private bool runtimeInitialized;
        private bool loadingData;
        private Product currentProduct;
        private DatasetImage currentImage;
        private IList<DatasetImage> datasetImages = new List<DatasetImage>();
        private IList<DefectCategory> defectCategories = new List<DefectCategory>();
        private IList<DatasetAnnotation> currentAnnotations = new List<DatasetAnnotation>();
        private Bitmap currentBitmap;
        private string activeTool = "Rectangle";

        public DatasetAnnotationPage()
        {
            InitializeComponent();
        }

        public void InitializeRuntime()
        {
            if (runtimeInitialized) return;
            runtimeInitialized = true;

            AppServices.EnsureInitialized();
            ConfigureRuntimeUi();
            BindEvents();
            SetActiveTool("Rectangle");
            LoadDataset();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (runtimeInitialized && Visible)
                LoadDataset();
        }

        private void ConfigureRuntimeUi()
        {
            btnImportImages.Text = "导入图片";
            btnRectangle.Text = "矩形";
            btnPolygon.Text = "多边形";
            btnBrush.Text = "画笔";
            btnEraser.Text = "橡皮擦";
            btnMaskEdit.Text = "Mask 编辑";
            btnAutoFix.Text = "边界检查";
            btnVersion.Text = "发布版本";

            ConfigureGrid(dgvImages);
            ConfigureGrid(dgvClasses);
            ConfigureGrid(dgvLayers);
            ConfigureGrid(dgvQueue);
            dgvLayers.Cursor = Cursors.Hand;

            cboCurrentClass.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCurrentClass.Items.Clear();
            numThreshold.Increment = 0.05M;
            lblTotalAnnotations.Text = "0";
            lblBoundaryScore.Text = "-";
            lblQualityScore.Text = "-";
            lblQualityAdvice.Text = "待导入";

            pnlCanvas.BackColor = UiTheme.Canvas;
            pnlCanvas.TabStop = true;
            PropertyInfo doubleBuffered = typeof(Panel).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (doubleBuffered != null) doubleBuffered.SetValue(pnlCanvas, true, null);
            ShowCanvasMessage("请先选择产品并导入数据集图片。\r\n\r\n矩形：拖拽  |  多边形：单击顶点、双击结束  |  画笔：按住拖动");
        }

        private static void ConfigureGrid(DataGridView grid)
        {
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void BindEvents()
        {
            btnImportImages.Click += delegate { ImportImages(); };
            btnRectangle.Click += delegate { ActivateDrawingTool("Rectangle"); };
            btnPolygon.Click += delegate { ActivateDrawingTool("Polygon"); };
            btnBrush.Click += delegate { ActivateDrawingTool("Brush"); };
            btnEraser.Click += delegate { ActivateDrawingTool("Eraser"); };
            btnMaskEdit.Click += delegate { ActivateDrawingTool("Brush"); };
            btnAutoFix.Click += delegate { CheckAndRepairBounds(); };
            btnVersion.Click += delegate { PublishVersion(); };

            dgvImages.SelectionChanged += delegate { SelectImageFromGrid(); };
            dgvQueue.CellDoubleClick += dgvQueue_CellDoubleClick;
            dgvClasses.SelectionChanged += delegate { SelectClassFromGrid(); };
            cboCurrentClass.SelectedIndexChanged += delegate { ApplySelectedCategoryDefaults(); };
            dgvLayers.CellDoubleClick += dgvLayers_CellDoubleClick;

            BindCanvasEvents();
            pnlCanvas.Resize += delegate { pnlCanvas.Invalidate(); };
            Disposed += delegate { ClearCurrentBitmap(); };
        }

        public void LoadDataset()
        {
            LoadDataset(currentImage == null ? 0 : currentImage.Id);
        }

        private void LoadDataset(long preferredImageId)
        {
            if (!runtimeInitialized || loadingData) return;
            loadingData = true;
            try
            {
                IList<Product> products = AppServices.Products.GetAllProducts();
                currentProduct = FindProduct(products, AppSession.CurrentProductId);
                if (currentProduct == null && products.Count > 0)
                    currentProduct = products[0];

                if (currentProduct == null)
                {
                    AppSession.SelectProduct(0);
                    defectCategories = new List<DefectCategory>();
                    datasetImages = new List<DatasetImage>();
                    FillCategoryControls();
                    FillImageControls(0);
                    ClearCurrentImage();
                    grpImages.Text = "数据集图片 | 尚未创建产品";
                    grpQueue.Text = "标注队列 / 缩略图";
                    ShowCanvasMessage("请先在“产品定义”页面创建并保存产品。\r\n保存后返回本页即可导入图片。");
                    UpdateUiAvailability();
                    return;
                }

                AppSession.SelectProduct(currentProduct.Id);
                defectCategories = AppServices.Products.GetDefectCategories(currentProduct.Id);
                datasetImages = AppServices.Datasets.GetImages(currentProduct.Id);
                grpImages.Text = "数据集图片 | " + currentProduct.ProductCode + " · " + currentProduct.ProductName;

                FillCategoryControls();
                FillImageControls(preferredImageId);
                UpdateVersionCaption();
            }
            catch (Exception ex)
            {
                ClearCurrentImage();
                ShowCanvasMessage("数据集加载失败：\r\n" + ex.Message);
                MessageBox.Show(this, ex.Message, "加载数据集失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                loadingData = false;
            }

            SelectImageFromGrid();
            UpdateUiAvailability();
        }

        private static Product FindProduct(IList<Product> products, long productId)
        {
            if (productId <= 0) return null;
            foreach (Product product in products)
            {
                if (product.Id == productId) return product;
            }
            return null;
        }

        private void FillCategoryControls()
        {
            dgvClasses.Rows.Clear();
            cboCurrentClass.Items.Clear();

            foreach (DefectCategory category in defectCategories)
            {
                if (!category.IsEnabled) continue;
                int rowIndex = dgvClasses.Rows.Add(
                    category.CategoryCode,
                    category.CategoryName,
                    string.IsNullOrWhiteSpace(category.DefectType) ? "通用" : category.DefectType);
                dgvClasses.Rows[rowIndex].Tag = category;
                cboCurrentClass.Items.Add(new CategoryItem(category));
            }

            if (cboCurrentClass.Items.Count > 0)
                cboCurrentClass.SelectedIndex = 0;
        }

        private void FillImageControls(long preferredImageId)
        {
            dgvImages.Rows.Clear();
            dgvQueue.Rows.Clear();
            int preferredRow = -1;

            for (int i = 0; i < datasetImages.Count; i++)
            {
                DatasetImage image = datasetImages[i];
                int imageRow = dgvImages.Rows.Add(image.FileName, image.Status);
                dgvImages.Rows[imageRow].Tag = image;

                int classCount = CountClasses(image.Id);
                int queueRow = dgvQueue.Rows.Add((i + 1).ToString("0000"), image.FileName, image.Status, classCount.ToString());
                dgvQueue.Rows[queueRow].Tag = image;
                if (image.Id == preferredImageId) preferredRow = imageRow;
            }

            if (dgvImages.Rows.Count == 0)
            {
                ClearCurrentImage();
                ShowCanvasMessage("当前产品尚未导入数据集图片。\r\n\r\n点击“导入图片”可一次选择多张 PNG、JPG、BMP 或 TIFF 图片。");
                return;
            }

            if (preferredRow < 0) preferredRow = 0;
            dgvImages.ClearSelection();
            dgvImages.Rows[preferredRow].Selected = true;
            dgvImages.CurrentCell = dgvImages.Rows[preferredRow].Cells[0];
        }

        private int CountClasses(long imageId)
        {
            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
            foreach (DatasetAnnotation annotation in AppServices.Datasets.GetAnnotations(imageId))
                names.Add((annotation.CategoryId ?? 0) + "|" + (annotation.CategoryName ?? string.Empty));
            return names.Count;
        }

        private void SelectImageFromGrid()
        {
            if (loadingData || dgvImages.CurrentRow == null) return;
            DatasetImage image = dgvImages.CurrentRow.Tag as DatasetImage;
            if (image == null) return;
            LoadImage(image);
        }

        private void LoadImage(DatasetImage image)
        {
            CancelWorkingAnnotation();
            ClearCurrentBitmap();
            currentImage = image;

            try
            {
                string imagePath = AppServices.Datasets.GetImagePath(image);
                if (!File.Exists(imagePath))
                    throw new FileNotFoundException("图片文件已从 Workspace 中移除。", imagePath);
                using (Image source = Image.FromFile(imagePath))
                    currentBitmap = new Bitmap(source);

                currentAnnotations = AppServices.Datasets.GetAnnotations(image.Id);
                lblCanvasInfo.Visible = false;
                RefreshAnnotationPanels();
                UpdateCanvasCaption();
                pnlCanvas.Invalidate();
            }
            catch (Exception ex)
            {
                currentAnnotations = new List<DatasetAnnotation>();
                ShowCanvasMessage("无法打开图片：\r\n" + image.FileName + "\r\n\r\n" + ex.Message);
            }
            UpdateUiAvailability();
        }

        private void ImportImages()
        {
            if (currentProduct == null)
            {
                MessageBox.Show(this, "请先在“产品定义”页面创建并保存产品。", "导入图片", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Filter = "图像文件|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|所有文件|*.*";
                dialog.Title = "选择要导入的数据集图片";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                int imported = 0;
                long preferredId = 0;
                List<string> failures = new List<string>();
                foreach (string fileName in dialog.FileNames)
                {
                    try
                    {
                        DatasetImage image = AppServices.Datasets.ImportImage(currentProduct.Id, fileName);
                        if (preferredId == 0) preferredId = image.Id;
                        imported++;
                    }
                    catch (Exception ex)
                    {
                        failures.Add(Path.GetFileName(fileName) + "：" + ex.Message);
                    }
                }

                LoadDataset(preferredId);
                string message = "成功导入 " + imported + " 张图片。";
                if (failures.Count > 0)
                {
                    message += "\r\n失败 " + failures.Count + " 张：\r\n";
                    for (int i = 0; i < Math.Min(3, failures.Count); i++) message += failures[i] + "\r\n";
                }
                MessageBox.Show(this, message.TrimEnd(), failures.Count == 0 ? "导入完成" : "部分图片未导入",
                    MessageBoxButtons.OK, failures.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
        }

        private void ActivateDrawingTool(string tool)
        {
            if (currentImage == null || currentBitmap == null)
            {
                MessageBox.Show(this, "请先导入并选择一张图片。", "标注工具", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!string.Equals(tool, "Eraser", StringComparison.Ordinal) && GetSelectedCategory() == null)
            {
                MessageBox.Show(this, "当前产品没有已启用的瑕疵类别，请先在“产品定义”中配置。", "标注工具", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SetActiveTool(tool);
        }

        private void SetActiveTool(string tool)
        {
            CancelWorkingAnnotation();
            activeTool = tool;
            UpdateToolButtonStyles();
            pnlCanvas.Cursor = string.Equals(tool, "Eraser", StringComparison.Ordinal) ? Cursors.Hand : Cursors.Cross;
            UpdateCanvasCaption();
        }

        private void UpdateToolButtonStyles()
        {
            btnRectangle.BackColor = string.Equals(activeTool, "Rectangle", StringComparison.Ordinal) ? Color.FromArgb(210, 225, 242) : UiTheme.Surface;
            btnPolygon.BackColor = string.Equals(activeTool, "Polygon", StringComparison.Ordinal) ? Color.FromArgb(210, 225, 242) : UiTheme.Surface;
            btnBrush.BackColor = string.Equals(activeTool, "Brush", StringComparison.Ordinal) ? Color.FromArgb(210, 225, 242) : UiTheme.Surface;
            btnMaskEdit.BackColor = string.Equals(activeTool, "Brush", StringComparison.Ordinal) ? Color.FromArgb(210, 225, 242) : UiTheme.Surface;
            btnEraser.BackColor = string.Equals(activeTool, "Eraser", StringComparison.Ordinal) ? Color.FromArgb(242, 222, 210) : UiTheme.Surface;
        }

        private DefectCategory GetSelectedCategory()
        {
            CategoryItem item = cboCurrentClass.SelectedItem as CategoryItem;
            return item == null ? null : item.Category;
        }

        private void SelectClassFromGrid()
        {
            if (loadingData || dgvClasses.CurrentRow == null) return;
            DefectCategory category = dgvClasses.CurrentRow.Tag as DefectCategory;
            if (category == null) return;
            for (int i = 0; i < cboCurrentClass.Items.Count; i++)
            {
                CategoryItem item = cboCurrentClass.Items[i] as CategoryItem;
                if (item != null && item.Category.Id == category.Id)
                {
                    cboCurrentClass.SelectedIndex = i;
                    break;
                }
            }
        }

        private void ApplySelectedCategoryDefaults()
        {
            DefectCategory category = GetSelectedCategory();
            if (category == null) return;
            decimal threshold = (decimal)Math.Max(0D, Math.Min(1D, category.DefaultThreshold));
            numThreshold.Value = Math.Max(numThreshold.Minimum, Math.Min(numThreshold.Maximum, threshold));
        }

        private void dgvQueue_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DatasetImage image = dgvQueue.Rows[e.RowIndex].Tag as DatasetImage;
            if (image == null) return;
            foreach (DataGridViewRow row in dgvImages.Rows)
            {
                DatasetImage candidate = row.Tag as DatasetImage;
                if (candidate == null || candidate.Id != image.Id) continue;
                dgvImages.CurrentCell = row.Cells[0];
                row.Selected = true;
                break;
            }
        }

        private void dgvLayers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || currentImage == null) return;
            LayerItem layer = dgvLayers.Rows[e.RowIndex].Tag as LayerItem;
            if (layer == null) return;
            try
            {
                AppServices.Datasets.SetCategoryVisibility(currentImage.Id, layer.CategoryId, layer.CategoryName, !layer.IsVisible);
                RefreshCurrentAnnotations();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "切换图层失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshCurrentAnnotations()
        {
            if (currentImage == null) return;
            currentAnnotations = AppServices.Datasets.GetAnnotations(currentImage.Id);
            currentImage.Status = currentAnnotations.Count == 0 ? "未标注" : "已标注";
            RefreshAnnotationPanels();
            RefreshImageRowSummaries();
            pnlCanvas.Invalidate();
            UpdateUiAvailability();
        }

        private void RefreshAnnotationPanels()
        {
            RefreshLayerGrid();
            int total = currentAnnotations.Count;
            lblTotalAnnotations.Text = total.ToString();
            lblBoundaryScore.Text = total == 0 ? "-" : "1.00";
            lblQualityScore.Text = total == 0 ? "-" : "1.00";
            lblQualityAdvice.Text = total == 0 ? "待标注" : "通过";
        }

        private void RefreshLayerGrid()
        {
            dgvLayers.Rows.Clear();
            foreach (DefectCategory category in defectCategories)
            {
                if (!category.IsEnabled) continue;
                int count = 0;
                bool visible = true;
                foreach (DatasetAnnotation annotation in currentAnnotations)
                {
                    if (annotation.CategoryId != category.Id) continue;
                    count++;
                    if (!annotation.IsVisible) visible = false;
                }
                LayerItem layer = new LayerItem(category.Id, category.CategoryName, visible);
                int rowIndex = dgvLayers.Rows.Add(category.CategoryName, count.ToString(), visible ? "是" : "否");
                dgvLayers.Rows[rowIndex].Tag = layer;
            }
        }

        private void RefreshImageRowSummaries()
        {
            if (currentImage == null) return;
            int classCount = 0;
            HashSet<string> classes = new HashSet<string>(StringComparer.Ordinal);
            foreach (DatasetAnnotation annotation in currentAnnotations)
                classes.Add((annotation.CategoryId ?? 0) + "|" + (annotation.CategoryName ?? string.Empty));
            classCount = classes.Count;

            foreach (DataGridViewRow row in dgvImages.Rows)
            {
                DatasetImage image = row.Tag as DatasetImage;
                if (image != null && image.Id == currentImage.Id) row.Cells[1].Value = currentImage.Status;
            }
            foreach (DataGridViewRow row in dgvQueue.Rows)
            {
                DatasetImage image = row.Tag as DatasetImage;
                if (image == null || image.Id != currentImage.Id) continue;
                row.Cells[2].Value = currentImage.Status;
                row.Cells[3].Value = classCount.ToString();
            }
        }

        private void CheckAndRepairBounds()
        {
            if (currentImage == null) return;
            try
            {
                int repaired = AppServices.Datasets.RepairAnnotationBounds(currentImage.Id);
                RefreshCurrentAnnotations();
                MessageBox.Show(this,
                    repaired == 0 ? "边界检查完成，所有标注均位于图像范围内。" : "已修正 " + repaired + " 个越界标注。",
                    "边界检查", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "边界检查失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PublishVersion()
        {
            if (currentProduct == null || datasetImages.Count == 0) return;
            DialogResult answer = MessageBox.Show(this,
                "将以当前图片和标注数量创建一个只读版本记录。是否继续？",
                "发布数据集版本", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;

            try
            {
                DatasetVersion version = AppServices.Datasets.CreateVersion(currentProduct.Id, "由数据集标注页面发布");
                UpdateVersionCaption();
                MessageBox.Show(this,
                    "已发布 " + version.VersionCode + "\r\n图片：" + version.ImageCount + "\r\n标注：" + version.AnnotationCount,
                    "发布成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "发布版本失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateVersionCaption()
        {
            if (currentProduct == null)
            {
                grpQueue.Text = "标注队列 / 缩略图";
                return;
            }
            DatasetVersion latest = AppServices.Datasets.GetLatestVersion(currentProduct.Id);
            grpQueue.Text = "标注队列 / 缩略图 | 当前版本：" + (latest == null ? "未发布" : latest.VersionCode);
        }

        private void UpdateUiAvailability()
        {
            bool hasProduct = currentProduct != null;
            bool hasImage = currentImage != null && currentBitmap != null;
            bool hasCategory = GetSelectedCategory() != null;
            btnImportImages.Enabled = hasProduct;
            btnRectangle.Enabled = hasImage && hasCategory;
            btnPolygon.Enabled = hasImage && hasCategory;
            btnBrush.Enabled = hasImage && hasCategory;
            btnMaskEdit.Enabled = hasImage && hasCategory;
            btnEraser.Enabled = hasImage && currentAnnotations.Count > 0;
            btnAutoFix.Enabled = hasImage && currentAnnotations.Count > 0;
            btnVersion.Enabled = hasProduct && datasetImages.Count > 0;
            UpdateToolButtonStyles();
        }

        private void UpdateCanvasCaption()
        {
            if (currentImage == null) return;
            string instruction;
            switch (activeTool)
            {
                case "Polygon": instruction = "单击添加顶点，双击完成，右键取消"; break;
                case "Brush": instruction = "按住左键绘制 Mask 笔迹，右键取消"; break;
                case "Eraser": instruction = "单击标注区域即可删除"; break;
                default: instruction = "按住左键拖拽矩形，右键取消"; break;
            }
            grpCanvas.Text = "标注画布 | " + currentImage.Width + " × " + currentImage.Height + " | Fit | " + instruction;
        }

        private void ShowCanvasMessage(string text)
        {
            lblCanvasInfo.Text = text;
            lblCanvasInfo.Visible = true;
            lblCanvasInfo.BringToFront();
            pnlCanvas.Invalidate();
        }

        private void ClearCurrentImage()
        {
            CancelWorkingAnnotation();
            ClearCurrentBitmap();
            currentImage = null;
            currentAnnotations = new List<DatasetAnnotation>();
            dgvLayers.Rows.Clear();
            RefreshAnnotationPanels();
        }

        private void ClearCurrentBitmap()
        {
            if (currentBitmap == null) return;
            currentBitmap.Dispose();
            currentBitmap = null;
        }

        private sealed class CategoryItem
        {
            public CategoryItem(DefectCategory category) { Category = category; }
            public DefectCategory Category { get; private set; }
            public override string ToString() { return Category.CategoryName + " (" + Category.CategoryCode + ")"; }
        }

        private sealed class LayerItem
        {
            public LayerItem(long? categoryId, string categoryName, bool isVisible)
            {
                CategoryId = categoryId;
                CategoryName = categoryName;
                IsVisible = isVisible;
            }
            public long? CategoryId { get; private set; }
            public string CategoryName { get; private set; }
            public bool IsVisible { get; private set; }
        }
    }
}
