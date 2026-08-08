using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using IAD.Infrastructure.Storage;
using IAD.Models;
using IAD.Services;
using IAD.Shell;

namespace IAD.Pages
{
    public partial class ProductDefinitionPage : UserControl
    {
        private bool runtimeInitialized;
        private long currentProductId;
        private Product currentProduct;
        private ProductDefinitionSettings currentSettings;

        public ProductDefinitionPage()
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
            LoadProductDefinition();
        }

        private void ConfigureRuntimeUi()
        {
            btnVersion.Text = "产品管理";
            btnRectangleRoi.Text = "管理 ROI";
            btnClearRoi.Text = "清空 ROI";
            btnEditDefect.Text = "保存选中";
            btnImportDefects.Enabled = false;
            btnExportDefects.Enabled = false;
            dgvDefects.AllowUserToAddRows = false;
            dgvDefects.AllowUserToDeleteRows = false;
            dgvDefects.MultiSelect = false;
            dgvDefects.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void BindEvents()
        {
            btnSave.Click += delegate { SaveProductDefinition(); };
            btnVersion.Click += delegate { OpenProductManager(); };
            btnImportReference.Click += delegate { ImportReferenceImage(); };
            btnRectangleRoi.Click += delegate { OpenRoiManager(); };
            btnClearRoi.Click += delegate { ClearAllRois(); };
            btnAddDefect.Click += delegate { AddDefectCategory(); };
            btnEditDefect.Click += delegate { SaveSelectedDefectCategory(); };
            btnDeleteDefect.Click += delegate { DeleteSelectedDefectCategory(); };
            btnToggleDefect.Click += delegate { ToggleSelectedDefectCategory(); };
            btnBuildTemplate.Click += delegate
            {
                MessageBox.Show(this, "产品、基准图和 ROI 已可持久化。HALCON Shape Model 建模将在下一阶段接入。", "定位模板", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            btnTestLocalization.Click += delegate
            {
                lblLastScore.Text = "最近测试：等待 HALCON 定位模块";
            };
        }

        public void LoadProductDefinition()
        {
            try
            {
                IList<Product> products = AppServices.Products.GetAllProducts();
                if (currentProductId <= 0)
                {
                    if (products.Count == 0)
                    {
                        BeginNewProduct();
                        return;
                    }
                    currentProductId = products[0].Id;
                }

                LoadProduct(currentProductId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "加载产品定义失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                BeginNewProduct();
            }
        }

        private void LoadProduct(long productId)
        {
            Product product = AppServices.Products.GetProduct(productId);
            if (product == null)
            {
                BeginNewProduct();
                return;
            }

            currentProductId = product.Id;
            currentProduct = product;
            currentSettings = AppServices.Products.GetDefinitionSettings(product.Id);

            txtProductName.Text = product.ProductName ?? string.Empty;
            txtProductCode.Text = product.ProductCode ?? string.Empty;
            txtImageSize.Text = currentSettings.ImageSize ?? string.Empty;
            txtProductCount.Text = Math.Max(1, currentSettings.ProductCount).ToString(CultureInfo.InvariantCulture);
            SelectComboValue(cboPose, currentSettings.Pose, "允许旋转");
            txtAcquisition.Text = currentSettings.AcquisitionCondition ?? string.Empty;

            SelectComboValue(cboLocalizationMethod, currentSettings.LocalizationMethod, "Shape Matching");
            SelectComboValue(cboModelType, currentSettings.ModelType, "Shape Model");
            txtMinScore.Text = currentSettings.MinScore.ToString("0.###", CultureInfo.InvariantCulture);
            txtAngleRange.Text = currentSettings.AngleRange ?? string.Empty;
            txtScaleRange.Text = currentSettings.ScaleRange ?? string.Empty;
            txtMatchCount.Text = Math.Max(1, currentSettings.MatchCount).ToString(CultureInfo.InvariantCulture);
            txtLastResult.Text = "尚未执行定位测试";

            txtPixelX.Text = currentSettings.PixelX.ToString("0.######", CultureInfo.InvariantCulture);
            txtPixelY.Text = currentSettings.PixelY.ToString("0.######", CultureInfo.InvariantCulture);
            SelectComboValue(cboLengthUnit, currentSettings.LengthUnit, "px");
            SelectComboValue(cboAreaUnit, currentSettings.AreaUnit, "px²");
            txtCalibrationVersion.Text = currentSettings.CalibrationVersion ?? string.Empty;
            SelectComboValue(cboCalibrationState, currentSettings.CalibrationState, "未标定");

            RefreshDefectGrid();
            RefreshRoiSummary();
            RefreshReferenceSummary();
            RefreshVersionSummary();
        }

        private void BeginNewProduct()
        {
            currentProductId = 0;
            currentProduct = null;
            currentSettings = ProductService.CreateDefaultSettings(0);

            txtProductName.Text = string.Empty;
            txtProductCode.Text = string.Empty;
            txtImageSize.Text = string.Empty;
            txtProductCount.Text = "1";
            SelectComboValue(cboPose, "允许旋转", "允许旋转");
            txtAcquisition.Text = string.Empty;
            SelectComboValue(cboLocalizationMethod, "Shape Matching", "Shape Matching");
            SelectComboValue(cboModelType, "Shape Model", "Shape Model");
            txtMinScore.Text = "0.8";
            txtAngleRange.Text = "-180 ~ 180 deg";
            txtScaleRange.Text = "0.90 ~ 1.10";
            txtMatchCount.Text = "1";
            txtLastResult.Text = "尚未执行定位测试";
            txtPixelX.Text = "1";
            txtPixelY.Text = "1";
            SelectComboValue(cboLengthUnit, "px", "px");
            SelectComboValue(cboAreaUnit, "px²", "px²");
            txtCalibrationVersion.Text = "CAL-1.0.0";
            SelectComboValue(cboCalibrationState, "未标定", "未标定");
            dgvDefects.Rows.Clear();
            lblReferenceFile.Text = "基准图：未导入";
            lblRoiState.Text = "ROI：0 个";
            lblTemplateType.Text = "定位模板：Shape Model";
            lblLastScore.Text = "最近测试：未执行";
            lblVersion.Text = "新产品：填写产品名称和编号后点击“保存产品定义”";
            lblCanvasHint.Text = "新产品定义\r\n\r\n请先填写产品名称、产品编号并保存\r\n保存后可导入基准图和维护 ROI";
            txtProductName.Focus();
        }

        public void SaveProductDefinition()
        {
            try
            {
                string code = RequireText(txtProductCode.Text, "产品编号");
                string name = RequireText(txtProductName.Text, "产品名称");

                if (currentProduct == null || currentProductId <= 0)
                {
                    currentProduct = AppServices.Products.CreateProduct(code, name, null);
                    currentProductId = currentProduct.Id;
                }
                else
                {
                    currentProduct.ProductCode = code;
                    currentProduct.ProductName = name;
                    AppServices.Products.UpdateProduct(currentProduct);
                }

                currentSettings = BuildSettingsFromPage(currentProductId);
                AppServices.Products.SaveDefinitionSettings(currentSettings);
                LoadProduct(currentProductId);
                MessageBox.Show(this, "产品定义已保存到 SQLite。", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "保存产品定义失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private ProductDefinitionSettings BuildSettingsFromPage(long productId)
        {
            ProductDefinitionSettings value = currentSettings ?? ProductService.CreateDefaultSettings(productId);
            value.ProductId = productId;
            value.ImageSize = txtImageSize.Text.Trim();
            value.ProductCount = ParsePositiveInt(txtProductCount.Text, "单图产品数");
            value.Pose = cboPose.Text;
            value.AcquisitionCondition = txtAcquisition.Text.Trim();
            value.TemplateType = string.IsNullOrWhiteSpace(value.TemplateType) ? "Shape Model" : value.TemplateType;
            value.LocalizationMethod = cboLocalizationMethod.Text;
            value.ModelType = cboModelType.Text;
            value.MinScore = ParseDouble(txtMinScore.Text, "最小Score");
            value.AngleRange = txtAngleRange.Text.Trim();
            value.ScaleRange = txtScaleRange.Text.Trim();
            value.MatchCount = ParsePositiveInt(txtMatchCount.Text, "匹配数量");
            value.PixelX = ParsePositiveDouble(txtPixelX.Text, "Pixel X");
            value.PixelY = ParsePositiveDouble(txtPixelY.Text, "Pixel Y");
            value.LengthUnit = cboLengthUnit.Text;
            value.AreaUnit = cboAreaUnit.Text;
            value.CalibrationVersion = txtCalibrationVersion.Text.Trim();
            value.CalibrationState = cboCalibrationState.Text;
            if (string.IsNullOrWhiteSpace(value.ProductDefinitionVersion)) value.ProductDefinitionVersion = "PD-1.0.0";
            if (string.IsNullOrWhiteSpace(value.TemplateVersion)) value.TemplateVersion = "LT-1.0.0";
            return value;
        }

        private void OpenProductManager()
        {
            using (ProductSelectionDialog dialog = new ProductSelectionDialog(currentProductId))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                if (dialog.CreateNewRequested)
                    BeginNewProduct();
                else if (dialog.SelectedProductId > 0)
                    LoadProduct(dialog.SelectedProductId);
            }
        }

        private bool EnsureSavedProduct()
        {
            if (currentProductId > 0) return true;
            MessageBox.Show(this, "请先填写产品名称和编号，并保存产品定义。", "需要先保存产品", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private void ImportReferenceImage()
        {
            if (!EnsureSavedProduct()) return;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "图像文件|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|所有文件|*.*";
                dialog.Title = "选择产品基准图";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    string safeCode = MakeSafeFileName(currentProduct.ProductCode);
                    string targetDirectory = Path.Combine(ProjectStoragePaths.TemplatesPath, safeCode);
                    Directory.CreateDirectory(targetDirectory);
                    string targetFile = Path.Combine(targetDirectory, "reference" + Path.GetExtension(dialog.FileName).ToLowerInvariant());
                    File.Copy(dialog.FileName, targetFile, true);

                    currentSettings = BuildSettingsFromPage(currentProductId);
                    currentSettings.ReferenceImagePath = Path.Combine("Templates", safeCode, Path.GetFileName(targetFile));
                    AppServices.Products.SaveDefinitionSettings(currentSettings);
                    RefreshReferenceSummary();
                    lblCanvasHint.Text = "产品：" + currentProduct.ProductName + "\r\n\r\n基准图已保存\r\n" + Path.GetFileName(targetFile) + "\r\n\r\n下一阶段在此接入 HALCON HWindowControl";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "导入基准图失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void OpenRoiManager()
        {
            if (!EnsureSavedProduct()) return;
            using (RoiManagerDialog dialog = new RoiManagerDialog(currentProductId))
                dialog.ShowDialog(this);
            RefreshRoiSummary();
        }

        private void ClearAllRois()
        {
            if (!EnsureSavedProduct()) return;
            IList<RoiDefinition> rois = AppServices.Products.GetRois(currentProductId);
            if (rois.Count == 0) return;
            if (MessageBox.Show(this, "确定清空当前产品的全部 " + rois.Count + " 个 ROI？", "清空 ROI", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                AppServices.Products.DeleteAllRois(currentProductId);
                RefreshRoiSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "清空 ROI 失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshRoiSummary()
        {
            if (currentProductId <= 0)
            {
                lblRoiState.Text = "ROI：0 个";
                return;
            }

            IList<RoiDefinition> rois = AppServices.Products.GetRois(currentProductId);
            lblRoiState.Text = "ROI：" + rois.Count + " 个";
            lblCanvasHint.Text = "产品：" + currentProduct.ProductName + "\r\n\r\n已保存 ROI：" + rois.Count + " 个\r\n双击“管理 ROI”可维护精确坐标\r\n\r\n下一阶段在此接入 HALCON 图像显示";
        }

        private void AddDefectCategory()
        {
            if (!EnsureSavedProduct()) return;
            try
            {
                IList<DefectCategory> categories = AppServices.Products.GetDefectCategories(currentProductId);
                int order = categories.Count + 1;
                DefectCategory category = new DefectCategory
                {
                    ProductId = currentProductId,
                    CategoryCode = "DEFECT-" + order.ToString("000"),
                    CategoryName = "新缺陷" + order,
                    DefectType = "表面缺陷",
                    DetectionStrategy = "Multi-label Segmentation",
                    DefaultThreshold = 0.8,
                    MinArea = 0,
                    MinLength = 0,
                    DisplayOrder = order,
                    IsEnabled = true
                };
                AppServices.Products.SaveDefectCategory(category);
                RefreshDefectGrid(category.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "新增缺陷类别失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SaveSelectedDefectCategory()
        {
            if (!EnsureSavedProduct() || dgvDefects.CurrentRow == null || dgvDefects.CurrentRow.Tag == null) return;
            try
            {
                DefectCategory category = dgvDefects.CurrentRow.Tag as DefectCategory;
                if (category == null) return;
                DataGridViewRow row = dgvDefects.CurrentRow;
                category.CategoryName = CellText(row, 1);
                category.DefectType = CellText(row, 2);
                category.DetectionStrategy = CellText(row, 3);
                category.DefaultThreshold = ParseDouble(CellText(row, 4), "默认阈值");
                category.MinArea = ParseOptionalDouble(CellText(row, 5));
                category.MinLength = ParseOptionalDouble(CellText(row, 6));
                category.IsEnabled = !string.Equals(CellText(row, 7), "停用", StringComparison.OrdinalIgnoreCase);
                AppServices.Products.SaveDefectCategory(category);
                RefreshDefectGrid(category.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "保存缺陷类别失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteSelectedDefectCategory()
        {
            if (!EnsureSavedProduct() || dgvDefects.CurrentRow == null || dgvDefects.CurrentRow.Tag == null) return;
            DefectCategory category = dgvDefects.CurrentRow.Tag as DefectCategory;
            if (category == null) return;
            if (MessageBox.Show(this, "确定删除缺陷类别“" + category.CategoryName + "”？", "删除缺陷类别", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                AppServices.Products.DeleteDefectCategory(currentProductId, category.Id);
                RefreshDefectGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "删除缺陷类别失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ToggleSelectedDefectCategory()
        {
            if (!EnsureSavedProduct() || dgvDefects.CurrentRow == null || dgvDefects.CurrentRow.Tag == null) return;
            try
            {
                DefectCategory category = dgvDefects.CurrentRow.Tag as DefectCategory;
                if (category == null) return;
                category.IsEnabled = !category.IsEnabled;
                AppServices.Products.SaveDefectCategory(category);
                RefreshDefectGrid(category.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "更新缺陷类别状态失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshDefectGrid(long selectId = 0)
        {
            dgvDefects.Rows.Clear();
            if (currentProductId <= 0) return;

            IList<DefectCategory> categories = AppServices.Products.GetDefectCategories(currentProductId);
            foreach (DefectCategory category in categories)
            {
                int rowIndex = dgvDefects.Rows.Add(
                    category.DisplayOrder.ToString(CultureInfo.InvariantCulture),
                    category.CategoryName,
                    category.DefectType,
                    category.DetectionStrategy,
                    category.DefaultThreshold.ToString("0.###", CultureInfo.InvariantCulture),
                    category.MinArea <= 0 ? "-" : category.MinArea.ToString("0.###", CultureInfo.InvariantCulture),
                    category.MinLength <= 0 ? "-" : category.MinLength.ToString("0.###", CultureInfo.InvariantCulture),
                    category.IsEnabled ? "启用" : "停用");
                dgvDefects.Rows[rowIndex].Tag = category;
                if (category.Id == selectId)
                    dgvDefects.CurrentCell = dgvDefects.Rows[rowIndex].Cells[1];
            }
        }

        private void RefreshReferenceSummary()
        {
            string relative = currentSettings == null ? null : currentSettings.ReferenceImagePath;
            lblReferenceFile.Text = string.IsNullOrWhiteSpace(relative) ? "基准图：未导入" : "基准图：" + Path.GetFileName(relative);
            lblTemplateType.Text = "定位模板：" + ((currentSettings == null || string.IsNullOrWhiteSpace(currentSettings.TemplateType)) ? "Shape Model" : currentSettings.TemplateType);
        }

        private void RefreshVersionSummary()
        {
            if (currentProduct == null || currentSettings == null) return;
            lblVersion.Text = "当前产品：" + currentProduct.ProductCode + " / " + currentProduct.ProductName +
                              "    产品定义版本：" + (currentSettings.ProductDefinitionVersion ?? "PD-1.0.0") +
                              "    模板版本：" + (currentSettings.TemplateVersion ?? "LT-1.0.0");
        }

        private static void SelectComboValue(ComboBox comboBox, string value, string fallback)
        {
            string target = string.IsNullOrWhiteSpace(value) ? fallback : value;
            int index = comboBox.FindStringExact(target);
            if (index >= 0)
                comboBox.SelectedIndex = index;
            else if (comboBox.DropDownStyle != ComboBoxStyle.DropDownList)
                comboBox.Text = target;
            else if (comboBox.Items.Count > 0)
                comboBox.SelectedIndex = 0;
        }

        private static string RequireText(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(fieldName + "不能为空。");
            return value.Trim();
        }

        private static int ParsePositiveInt(string text, string fieldName)
        {
            int value;
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) && !int.TryParse(text, out value))
                throw new ArgumentException(fieldName + "格式不正确。");
            if (value <= 0) throw new ArgumentException(fieldName + "必须大于0。");
            return value;
        }

        private static double ParseDouble(string text, string fieldName)
        {
            double value;
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && !double.TryParse(text, out value))
                throw new ArgumentException(fieldName + "格式不正确。");
            return value;
        }

        private static double ParsePositiveDouble(string text, string fieldName)
        {
            double value = ParseDouble(text, fieldName);
            if (value <= 0) throw new ArgumentException(fieldName + "必须大于0。");
            return value;
        }

        private static double ParseOptionalDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Trim() == "-") return 0;
            return ParseDouble(text, "数值");
        }

        private static string CellText(DataGridViewRow row, int index)
        {
            object value = row.Cells[index].Value;
            return value == null ? string.Empty : Convert.ToString(value).Trim();
        }

        private static string MakeSafeFileName(string value)
        {
            string result = value ?? "Product";
            foreach (char c in Path.GetInvalidFileNameChars()) result = result.Replace(c, '_');
            return result;
        }
    }
}
