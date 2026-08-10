using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
        private ProductDefinitionSettings currentProductDefinition;
        private long loadedProductId = -1;
        private long loadedProductDataRevision = -1;
        private DatasetImage currentImage;
        private IList<DatasetImage> datasetImages = new List<DatasetImage>();
        private IList<DefectCategory> defectCategories = new List<DefectCategory>();
        private IList<DatasetAnnotation> currentAnnotations = new List<DatasetAnnotation>();
        private Bitmap currentBitmap;
        private string activeTool = "Rectangle";
        private static readonly HashSet<string> SupportedImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff"
        };

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
            if (!runtimeInitialized) return;
            if (Visible)
            {
                if (IsDatasetSnapshotStale()) LoadDataset();
            }
            else
            {
                CaptureDatasetSnapshotRevision();
            }
        }

        private bool IsDatasetSnapshotStale()
        {
            long productId = AppSession.CurrentProductId;
            return loadedProductId != productId ||
                   loadedProductDataRevision != ProductDataRevisionTracker.GetRevision(productId);
        }

        private void CaptureDatasetSnapshotRevision()
        {
            loadedProductId = AppSession.CurrentProductId;
            loadedProductDataRevision = ProductDataRevisionTracker.GetRevision(loadedProductId);
        }

        private void ConfigureRuntimeUi()
        {
            btnImportImages.Text = "导入文件";
            btnImportFolder.Text = "导入文件夹";
            btnDeleteImages.Text = "删除图片";
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
            dgvImages.MultiSelect = true;
            dgvLayers.Cursor = Cursors.Hand;

            AllowDrop = true;
            dgvImages.AllowDrop = true;
            pnlCanvas.AllowDrop = true;
            lblCanvasInfo.AllowDrop = true;

            cboCurrentClass.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCurrentClass.Items.Clear();
            numThreshold.Increment = 0.05M;
            lblCurrentProduct.ForeColor = UiTheme.Text;
            lblCurrentProduct.Font = UiTheme.Font(9.2F, true);
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
            btnImportFolder.Click += delegate { ImportFolder(); };
            btnDeleteImages.Click += delegate { DeleteSelectedImages(); };
            btnRectangle.Click += delegate { ActivateDrawingTool("Rectangle"); };
            btnPolygon.Click += delegate { ActivateDrawingTool("Polygon"); };
            btnBrush.Click += delegate { ActivateDrawingTool("Brush"); };
            btnEraser.Click += delegate { ActivateDrawingTool("Eraser"); };
            btnMaskEdit.Click += delegate { ActivateDrawingTool("Brush"); };
            btnAutoFix.Click += delegate { CheckAndRepairBounds(); };
            btnVersion.Click += delegate { PublishVersion(); };

            dgvImages.SelectionChanged += delegate { SelectImageFromGrid(); };
            dgvImages.KeyDown += dgvImages_KeyDown;
            dgvQueue.CellDoubleClick += dgvQueue_CellDoubleClick;
            dgvClasses.SelectionChanged += delegate { SelectClassFromGrid(); };
            cboCurrentClass.SelectedIndexChanged += delegate { ApplySelectedCategoryDefaults(); };
            dgvLayers.CellDoubleClick += dgvLayers_CellDoubleClick;
            AppSession.CurrentProductChanged += AppSession_CurrentProductChanged;

            DragEnter += ImportPath_DragEnter;
            DragDrop += ImportPath_DragDrop;
            dgvImages.DragEnter += ImportPath_DragEnter;
            dgvImages.DragDrop += ImportPath_DragDrop;
            pnlCanvas.DragEnter += ImportPath_DragEnter;
            pnlCanvas.DragDrop += ImportPath_DragDrop;
            lblCanvasInfo.DragEnter += ImportPath_DragEnter;
            lblCanvasInfo.DragDrop += ImportPath_DragDrop;

            BindCanvasEvents();
            pnlCanvas.Resize += delegate { pnlCanvas.Invalidate(); };
            Disposed += delegate
            {
                AppSession.CurrentProductChanged -= AppSession_CurrentProductChanged;
                ClearCurrentBitmap();
            };
        }

        public void LoadDataset()
        {
            long preferredImageId = currentProduct != null && currentProduct.Id == AppSession.CurrentProductId && currentImage != null
                ? currentImage.Id
                : 0;
            LoadDataset(preferredImageId);
        }

        private void LoadDataset(long preferredImageId)
        {
            if (!runtimeInitialized || loadingData) return;
            loadingData = true;
            try
            {
                IList<Product> products = AppServices.Products.GetAllProducts();
                currentProduct = FindProduct(products, AppSession.CurrentProductId);

                if (currentProduct == null)
                {
                    if (AppSession.CurrentProductId > 0) AppSession.SelectProduct(0);
                    currentProductDefinition = null;
                    defectCategories = new List<DefectCategory>();
                    datasetImages = new List<DatasetImage>();
                    FillCategoryControls();
                    FillImageControls(0);
                    ClearCurrentImage();
                    bool hasProducts = products.Count > 0;
                    grpImages.Text = hasProducts ? "数据集图片 | 尚未选择产品" : "数据集图片 | 尚未创建产品";
                    grpQueue.Text = "标注队列 / 缩略图";
                    lblCurrentProduct.Text = hasProducts
                        ? "当前产品：未选择 | 请先在产品定义中选择并保存"
                        : "当前产品：无 | 请先创建并保存产品定义";
                    ShowCanvasMessage(hasProducts
                        ? "请先到“产品定义”页面选择并保存一个产品。\r\n返回本页后，将只加载该产品的数据集和标注。"
                        : "请先在“产品定义”页面创建并保存产品。\r\n保存后返回本页即可导入图片。");
                    UpdateUiAvailability();
                    return;
                }

                currentProductDefinition = AppServices.Datasets.GetSavedProductDefinition(currentProduct.Id);
                if (currentProductDefinition == null || string.IsNullOrWhiteSpace(currentProductDefinition.ProductDefinitionVersion))
                {
                    defectCategories = new List<DefectCategory>();
                    datasetImages = new List<DatasetImage>();
                    FillCategoryControls();
                    FillImageControls(0);
                    ClearCurrentImage();
                    grpImages.Text = "数据集图片 | " + currentProduct.ProductCode + " · " + currentProduct.ProductName;
                    grpQueue.Text = "标注队列 / 缩略图";
                    lblCurrentProduct.Text = "当前产品：" + currentProduct.ProductCode + " · " + currentProduct.ProductName + " | 产品定义：未保存";
                    ShowCanvasMessage("当前产品定义尚未保存。\r\n请先回到“产品定义”页面保存，再进行数据集标注。");
                    UpdateUiAvailability();
                    return;
                }

                defectCategories = AppServices.Products.GetDefectCategories(currentProduct.Id);
                datasetImages = AppServices.Datasets.GetImages(currentProduct.Id);
                grpImages.Text = "数据集图片 | " + currentProduct.ProductCode + " · " + currentProduct.ProductName;
                lblCurrentProduct.Text = "当前产品：" + currentProduct.ProductCode + " · " + currentProduct.ProductName +
                                         " | 已保存定义：" + currentProductDefinition.ProductDefinitionVersion;

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
                CaptureDatasetSnapshotRevision();
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

        private void AppSession_CurrentProductChanged(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate { AppSession_CurrentProductChanged(sender, e); }));
                return;
            }
            if (Visible) LoadDataset(0);
        }

        private static string DisplayDefinitionVersion(string version)
        {
            return string.IsNullOrWhiteSpace(version) ? "未绑定" : version;
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
            IDictionary<long, int> classCounts = currentProduct == null
                ? new Dictionary<long, int>()
                : AppServices.Datasets.GetClassCounts(currentProduct.Id);

            for (int i = 0; i < datasetImages.Count; i++)
            {
                DatasetImage image = datasetImages[i];
                int imageRow = dgvImages.Rows.Add(image.FileName, image.Status, DisplayDefinitionVersion(image.ProductDefinitionVersion));
                dgvImages.Rows[imageRow].Tag = image;

                int classCount;
                classCounts.TryGetValue(image.Id, out classCount);
                int queueRow = dgvQueue.Rows.Add((i + 1).ToString("0000"), image.FileName, image.Status, classCount.ToString(), DisplayDefinitionVersion(image.ProductDefinitionVersion));
                dgvQueue.Rows[queueRow].Tag = image;
                if (image.Id == preferredImageId) preferredRow = imageRow;
            }

            if (dgvImages.Rows.Count == 0)
            {
                ClearCurrentImage();
                ShowCanvasMessage("当前产品尚未导入数据集图片。\r\n\r\n可以导入多个图片文件、选择整个文件夹，或把文件/文件夹拖到此页面。");
                return;
            }

            if (preferredRow < 0) preferredRow = 0;
            dgvImages.ClearSelection();
            dgvImages.Rows[preferredRow].Selected = true;
            dgvImages.CurrentCell = dgvImages.Rows[preferredRow].Cells[0];
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
            if (currentProduct == null || image.ProductId != currentProduct.Id)
            {
                ShowCanvasMessage("所选图片不属于当前产品，已阻止加载。请重新进入数据集标注页面。");
                return;
            }
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
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Filter = "支持的图像|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|所有文件|*.*";
                dialog.Title = "选择要导入的数据集图片";
                dialog.RestoreDirectory = true;
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                ImportPaths(dialog.FileNames, "文件导入");
            }
        }

        private void ImportFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择图片文件夹（将同时扫描其子文件夹）";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                ImportPaths(new[] { dialog.SelectedPath }, "文件夹导入");
            }
        }

        private async void ImportPaths(IEnumerable<string> paths, string operationName)
        {
            if (currentProduct == null || currentProductDefinition == null)
            {
                MessageBox.Show(this, "请先在“产品定义”页面选择并保存产品。", operationName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<string> discoveryFailures = new List<string>();
            List<string> files = CollectImagePaths(paths, discoveryFailures);
            if (files.Count == 0)
            {
                string emptyMessage = "没有找到可导入的 PNG、JPG、BMP 或 TIFF 图片。";
                if (discoveryFailures.Count > 0) emptyMessage += "\r\n\r\n" + discoveryFailures[0];
                MessageBox.Show(this, emptyMessage, operationName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!datasetWorkflowInitialized) InitializeDatasetWorkflowUi();
            long productId = currentProduct.Id;
            DatasetImportBatchSummary summary = new DatasetImportBatchSummary();
            summary.Failures.AddRange(discoveryFailures);
            CancellationToken cancellationToken = BeginImportProgress(files.Count);
            IProgress<int> progress = new Progress<int>(delegate(int completed) { ReportImportProgress(completed, files.Count); });
            try
            {
                await Task.Run(delegate
                {
                    AppServices.Datasets.BackfillContentHashes(productId);
                    for (int i = 0; i < files.Count; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            summary.Cancelled = true;
                            break;
                        }
                        string fileName = files[i];
                        try
                        {
                            DatasetImageImportResult result = AppServices.Datasets.ImportImageChecked(productId, fileName, false);
                            if (summary.PreferredImageId == 0) summary.PreferredImageId = result.Image.Id;
                            if (result.IsDuplicate) summary.Duplicates++;
                            else summary.Imported++;
                        }
                        catch (Exception ex)
                        {
                            summary.Failures.Add(Path.GetFileName(fileName) + "：" + ex.Message);
                        }
                        progress.Report(i + 1);
                    }
                });
            }
            finally
            {
                EndImportProgress();
            }

            LoadDataset(summary.PreferredImageId);
            string message = "发现 " + files.Count + " 张图片，成功导入 " + summary.Imported + " 张。";
            if (summary.Duplicates > 0) message += "\r\n按 SHA-256 跳过重复图片 " + summary.Duplicates + " 张。";
            if (summary.Cancelled) message += "\r\n导入已由用户取消，已完成的图片保持有效。";
            if (summary.Failures.Count > 0)
            {
                message += "\r\n失败 " + summary.Failures.Count + " 项：\r\n";
                for (int i = 0; i < Math.Min(5, summary.Failures.Count); i++) message += summary.Failures[i] + "\r\n";
                if (summary.Failures.Count > 5) message += "其余 " + (summary.Failures.Count - 5) + " 项未展开。";
            }
            MessageBox.Show(this, message.TrimEnd(), summary.Failures.Count == 0 ? "导入完成" : "部分图片未导入",
                MessageBoxButtons.OK, summary.Failures.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private static List<string> CollectImagePaths(IEnumerable<string> paths, IList<string> failures)
        {
            List<string> files = new List<string>();
            HashSet<string> uniqueFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (paths == null) return files;

            foreach (string rawPath in paths)
            {
                if (string.IsNullOrWhiteSpace(rawPath)) continue;
                string path;
                try
                {
                    path = Path.GetFullPath(rawPath);
                }
                catch (Exception ex)
                {
                    failures.Add(rawPath + "：路径无效（" + ex.Message + "）");
                    continue;
                }

                if (File.Exists(path))
                {
                    if (IsSupportedImagePath(path))
                    {
                        if (uniqueFiles.Add(path)) files.Add(path);
                    }
                    else
                    {
                        failures.Add(Path.GetFileName(path) + "：不支持该文件格式");
                    }
                    continue;
                }

                if (Directory.Exists(path))
                {
                    CollectDirectoryImages(path, files, uniqueFiles, failures);
                    continue;
                }

                failures.Add(path + "：文件或文件夹不存在");
            }

            files.Sort(StringComparer.CurrentCultureIgnoreCase);
            return files;
        }

        private static void CollectDirectoryImages(string rootPath, IList<string> files, ISet<string> uniqueFiles, IList<string> failures)
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(rootPath);
            while (pending.Count > 0)
            {
                string directory = pending.Pop();
                try
                {
                    foreach (string file in Directory.GetFiles(directory))
                    {
                        if (!IsSupportedImagePath(file)) continue;
                        string fullPath = Path.GetFullPath(file);
                        if (uniqueFiles.Add(fullPath)) files.Add(fullPath);
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(directory + "：读取图片失败（" + ex.Message + "）");
                }

                try
                {
                    foreach (string child in Directory.GetDirectories(directory))
                    {
                        FileAttributes attributes = File.GetAttributes(child);
                        if ((attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) continue;
                        pending.Push(child);
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(directory + "：读取子文件夹失败（" + ex.Message + "）");
                }
            }
        }

        private static bool IsSupportedImagePath(string path)
        {
            return SupportedImageExtensions.Contains(Path.GetExtension(path) ?? string.Empty);
        }

        private void ImportPath_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void ImportPath_DragDrop(object sender, DragEventArgs e)
        {
            string[] paths = e.Data == null ? null : e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null || paths.Length == 0) return;
            ImportPaths(paths, "拖拽导入");
        }

        private void dgvImages_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete) return;
            e.Handled = true;
            e.SuppressKeyPress = true;
            DeleteSelectedImages();
        }

        private void DeleteSelectedImages()
        {
            if (currentProduct == null || dgvImages.SelectedRows.Count == 0) return;

            List<DatasetImage> selected = new List<DatasetImage>();
            HashSet<long> selectedIds = new HashSet<long>();
            foreach (DataGridViewRow row in dgvImages.SelectedRows)
            {
                DatasetImage image = row.Tag as DatasetImage;
                if (image != null && selectedIds.Add(image.Id)) selected.Add(image);
            }
            if (selected.Count == 0) return;

            int annotationCount = 0;
            int maskCount = 0;
            try
            {
                foreach (DatasetImage image in selected)
                {
                    annotationCount += AppServices.Datasets.GetAnnotations(image.Id).Count;
                    maskCount += AppServices.Masks.GetMasks(image.Id).Count;
                }
            }
            catch
            {
                annotationCount = -1;
                maskCount = -1;
            }

            string names = string.Empty;
            for (int i = 0; i < Math.Min(5, selected.Count); i++) names += "\r\n• " + selected[i].FileName;
            if (selected.Count > 5) names += "\r\n• 其余 " + (selected.Count - 5) + " 张图片";
            DialogResult answer = MessageBox.Show(this,
                "确定从当前产品数据集中删除 " + selected.Count + " 张图片吗？" +
                (annotationCount >= 0
                    ? "\r\n关联的 " + annotationCount + " 个矢量标注和 " + maskCount + " 个 Mask 也会删除。"
                    : "\r\n所有关联矢量标注和 Mask 也会一并删除。") + names +
                "\r\n\r\n已发布的数据集版本不会被修改。",
                "删除数据集图片", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer != DialogResult.Yes) return;

            long preferredId = 0;
            if (currentImage != null && !selectedIds.Contains(currentImage.Id)) preferredId = currentImage.Id;
            if (preferredId == 0)
            {
                foreach (DatasetImage image in datasetImages)
                {
                    if (selectedIds.Contains(image.Id)) continue;
                    preferredId = image.Id;
                    break;
                }
            }
            if (currentImage != null && selectedIds.Contains(currentImage.Id)) ClearCurrentImage();

            int deleted = 0;
            int retainedByVersion = 0;
            List<string> warnings = new List<string>();
            List<string> failures = new List<string>();
            UseWaitCursor = true;
            try
            {
                foreach (DatasetImage image in selected)
                {
                    try
                    {
                        string notice = AppServices.Datasets.DeleteImage(currentProduct.Id, image.Id);
                        deleted++;
                        if (string.IsNullOrWhiteSpace(notice)) continue;
                        if (notice.IndexOf("已发布版本", StringComparison.Ordinal) >= 0) retainedByVersion++;
                        else warnings.Add(image.FileName + "：" + notice);
                    }
                    catch (Exception ex)
                    {
                        failures.Add(image.FileName + "：" + ex.Message);
                    }
                }
            }
            finally
            {
                UseWaitCursor = false;
            }

            LoadDataset(preferredId);
            string result = "成功删除 " + deleted + " 张图片及其矢量标注和 Mask。";
            if (retainedByVersion > 0)
                result += "\r\n其中 " + retainedByVersion + " 张的存储文件被历史发布版本引用，已安全保留。";
            if (warnings.Count > 0)
            {
                result += "\r\n存储清理警告 " + warnings.Count + " 项：\r\n";
                for (int i = 0; i < Math.Min(3, warnings.Count); i++) result += warnings[i] + "\r\n";
            }
            if (failures.Count > 0)
            {
                result += "\r\n删除失败 " + failures.Count + " 项：\r\n";
                for (int i = 0; i < Math.Min(3, failures.Count); i++) result += failures[i] + "\r\n";
            }
            MessageBox.Show(this, result.TrimEnd(), failures.Count == 0 ? "删除完成" : "部分图片未删除",
                MessageBoxButtons.OK, failures.Count == 0 && warnings.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
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
            RefreshQualityMetrics();
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
            if (currentProduct == null || currentProductDefinition == null || datasetImages.Count == 0) return;
            DatasetQualityReport qualityReport;
            try
            {
                qualityReport = AppServices.DatasetWorkflow.EvaluateProduct(currentProduct.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "质量门禁执行失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (qualityReport.ErrorCount > 0)
            {
                MessageBox.Show(this,
                    "数据集存在 " + qualityReport.ErrorCount + " 张未通过质量门禁的图片，暂不能发布版本。\r\n\r\n" +
                    "请打开“数据集管理”，完成缺陷标注或将无缺陷图片标记为正常样本，然后审核通过。",
                    "发布前质量门禁", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult answer = MessageBox.Show(this,
                "当前产品：" + currentProduct.ProductCode + " · " + currentProduct.ProductName +
                "\r\n产品定义：" + currentProductDefinition.ProductDefinitionVersion +
                "\r\n质量门禁：通过 " + qualityReport.PassedCount + "，警告 " + qualityReport.WarningCount +
                "\r\n\r\n将以当前图片、矢量标注、Mask、审核状态和数据划分创建只读版本。是否继续？",
                "发布数据集版本", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;

            string versionNotes;
            if (!TryPromptVersionNotes(out versionNotes)) return;

            try
            {
                DatasetVersion version = AppServices.Datasets.CreateVersion(currentProduct.Id, versionNotes);
                UpdateVersionCaption();
                MessageBox.Show(this,
                    "已发布 " + version.VersionCode + "\r\n产品定义：" + version.ProductDefinitionVersion +
                    "\r\n图片：" + version.ImageCount + "\r\n矢量标注：" + version.AnnotationCount + "\r\nMask：" + version.MaskCount,
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
            bool hasProduct = currentProduct != null && currentProductDefinition != null;
            bool hasImage = currentImage != null && currentBitmap != null;
            bool hasCategory = GetSelectedCategory() != null;
            btnImportImages.Enabled = hasProduct;
            btnImportFolder.Enabled = hasProduct;
            btnDeleteImages.Enabled = hasProduct && dgvImages.SelectedRows.Count > 0;
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
            grpCanvas.Text = "标注画布 | " + currentImage.Width + " × " + currentImage.Height +
                             " | " + DisplayDefinitionVersion(currentImage.ProductDefinitionVersion) + " | Fit | " + instruction;
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
