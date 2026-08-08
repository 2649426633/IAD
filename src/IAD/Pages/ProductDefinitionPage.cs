using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using IAD.Infrastructure.Storage;
using IAD.Models;
using IAD.Services;
using IAD.Security;
using IAD.Shell;

namespace IAD.Pages
{
    public partial class ProductDefinitionPage : UserControl
    {
        private bool runtimeInitialized;
        private long currentProductId;
        private Product currentProduct;
        private ProductDefinitionSettings currentSettings;
        private string referencePreviewMessage;
        private bool loadingDefectGrid;
        private bool savingDefectGrid;

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
            btnVersion.Text = "切换 / 新建产品";
            btnRectangleRoi.Text = "管理 ROI";
            btnClearRoi.Text = "清空 ROI";
            btnEditDefect.Text = "保存当前类别";
            btnBuildTemplate.Text = "校验模板配置";
            btnTestLocalization.Text = "测试定位（待接）";
            btnTestLocalization.Enabled = false;
            btnImportDefects.Enabled = true;
            btnExportDefects.Enabled = true;
            dgvDefects.AllowUserToAddRows = false;
            dgvDefects.AllowUserToDeleteRows = false;
            dgvDefects.MultiSelect = false;
            dgvDefects.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDefects.ReadOnly = false;
            dgvDefects.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvDefectsCol1.ReadOnly = true;
            dgvDefectsCol2.ReadOnly = false;
            dgvDefectsCol3.ReadOnly = false;
            dgvDefectsCol4.ReadOnly = false;
            dgvDefectsCol5.ReadOnly = false;
            dgvDefectsCol6.ReadOnly = false;
            dgvDefectsCol7.ReadOnly = false;
            dgvDefectsCol8.ReadOnly = true;

            pnlTemplateCanvas.BackgroundImageLayout = ImageLayout.Zoom;
            lblCanvasHint.BackColor = Color.Transparent;
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
            btnImportDefects.Click += delegate { ImportDefectCategories(); };
            btnExportDefects.Click += delegate { ExportDefectCategories(); };
            dgvDefects.CellValueChanged += dgvDefects_CellValueChanged;
            btnFastMode.Click += delegate { ApplyLocalizationPreset("快速模式"); };
            btnFineMode.Click += delegate { ApplyLocalizationPreset("精细模式"); };
            btnAutoMode.Click += delegate { ApplyLocalizationPreset("自动模式"); };
            btnBuildTemplate.Click += delegate { ValidateTemplateConfiguration(); };
            Disposed += delegate { ClearReferencePreview(); };
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
                    Product selectedProduct = AppSession.CurrentProductId > 0
                        ? AppServices.Products.GetProduct(AppSession.CurrentProductId)
                        : null;
                    currentProductId = selectedProduct == null ? products[0].Id : selectedProduct.Id;
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
            AppSession.SelectProduct(product.Id);
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
            AppSession.SelectProduct(0);
            currentSettings = ProductService.CreateDefaultSettings(0);
            ClearReferencePreview();
            referencePreviewMessage = null;

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
            grpDefects.Text = "缺陷类别管理 | 请先保存产品";
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
                bool isNewProduct = currentProduct == null || currentProductId <= 0;

                // 先完成页面参数解析和校验，避免产品主记录已创建、扩展配置却保存失败。
                ProductDefinitionSettings value = BuildSettingsFromPage(currentProductId);
                if (!isNewProduct) SaveAllDefectCategoriesFromGrid();

                if (isNewProduct)
                {
                    currentProduct = AppServices.Products.CreateProduct(code, name, null);
                    currentProductId = currentProduct.Id;
                    value.ProductId = currentProductId;
                    value.ProductDefinitionVersion = "PD-1.0.0";
                }
                else
                {
                    currentProduct.ProductCode = code;
                    currentProduct.ProductName = name;
                    AppServices.Products.UpdateProduct(currentProduct);
                    value.ProductDefinitionVersion = IncrementVersion(value.ProductDefinitionVersion, "PD-");
                }

                currentSettings = value;
                AppServices.Products.SaveDefinitionSettings(currentSettings);
                LoadProduct(currentProductId);
                MessageBox.Show(
                    this,
                    "产品定义已保存到 SQLite。\r\n当前版本：" + currentSettings.ProductDefinitionVersion,
                    "保存成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
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
            value.MinScore = ParseRangeDouble(txtMinScore.Text, "最小Score", 0, 1);
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
            UpdateCanvasHint(rois.Count);
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
                    CategoryCode = BuildNextDefectCode(categories),
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
                savingDefectGrid = true;
                dgvDefects.EndEdit();
                DefectCategory category = SaveDefectCategoryRow(dgvDefects.CurrentRow);
                RefreshDefectGrid(category.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "保存缺陷类别失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                savingDefectGrid = false;
            }
        }

        private void dgvDefects_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (loadingDefectGrid || savingDefectGrid || currentProductId <= 0 || e.RowIndex < 0) return;
            DataGridViewRow row = dgvDefects.Rows[e.RowIndex];
            DefectCategory category = row.Tag as DefectCategory;
            if (category == null) return;

            try
            {
                savingDefectGrid = true;
                SaveDefectCategoryRow(row);
                grpDefects.Text = "缺陷类别管理 | 已自动保存：“" + category.CategoryName + "”";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    ex.Message + "\r\n\r\n本次编辑未保存，列表将恢复数据库中的值。",
                    "缺陷类别保存失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                long categoryId = category.Id;
                BeginInvoke(new MethodInvoker(delegate
                {
                    if (!IsDisposed && !Disposing) RefreshDefectGrid(categoryId);
                }));
            }
            finally
            {
                savingDefectGrid = false;
            }
        }

        private DefectCategory SaveDefectCategoryRow(DataGridViewRow row)
        {
            if (row == null) throw new ArgumentNullException("row");
            DefectCategory category = row.Tag as DefectCategory;
            if (category == null) throw new InvalidOperationException("当前行未关联有效的缺陷类别。");
            if (category.ProductId != currentProductId)
                throw new InvalidOperationException("当前类别不属于所选产品，已阻止保存。");

            category.CategoryName = RequireText(CellText(row, 1), "缺陷名称");
            category.DefectType = RequireText(CellText(row, 2), "缺陷类型");
            category.DetectionStrategy = RequireText(CellText(row, 3), "检测策略");
            category.DefaultThreshold = ParseRangeDouble(CellText(row, 4), "默认阈值", 0, 1);
            category.MinArea = ParseOptionalNonNegativeDouble(CellText(row, 5), "最小面积");
            category.MinLength = ParseOptionalNonNegativeDouble(CellText(row, 6), "最小长度");
            category.IsEnabled = !string.Equals(CellText(row, 7), "停用", StringComparison.OrdinalIgnoreCase);
            return AppServices.Products.SaveDefectCategory(category);
        }

        private void SaveAllDefectCategoriesFromGrid()
        {
            if (currentProductId <= 0 || dgvDefects.Rows.Count == 0) return;
            bool previousSaving = savingDefectGrid;
            savingDefectGrid = true;
            try
            {
                dgvDefects.EndEdit();
                foreach (DataGridViewRow row in dgvDefects.Rows)
                    SaveDefectCategoryRow(row);
            }
            finally
            {
                savingDefectGrid = previousSaving;
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
            loadingDefectGrid = true;
            try
            {
                dgvDefects.Rows.Clear();
                if (currentProductId <= 0)
                {
                    grpDefects.Text = "缺陷类别管理 | 尚未选择产品";
                    return;
                }

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
                grpDefects.Text = "缺陷类别管理 | 当前产品共 " + categories.Count + " 类（编辑后自动保存）";
            }
            finally
            {
                loadingDefectGrid = false;
            }
        }

        private void RefreshReferenceSummary()
        {
            string relative = currentSettings == null ? null : currentSettings.ReferenceImagePath;
            if (string.IsNullOrWhiteSpace(relative))
            {
                lblReferenceFile.Text = "基准图：未导入";
                ClearReferencePreview();
                referencePreviewMessage = null;
            }
            else
            {
                try
                {
                    string fullPath = ResolveWorkspaceFile(relative);
                    lblReferenceFile.Text = File.Exists(fullPath)
                        ? "基准图：" + Path.GetFileName(relative)
                        : "基准图：文件缺失";
                    LoadReferencePreview(fullPath);
                }
                catch (Exception ex)
                {
                    ClearReferencePreview();
                    lblReferenceFile.Text = "基准图：路径无效";
                    referencePreviewMessage = ex.Message;
                }
            }

            lblTemplateType.Text = "定位模板：" + ((currentSettings == null || string.IsNullOrWhiteSpace(currentSettings.TemplateType)) ? "Shape Model" : currentSettings.TemplateType);
            int roiCount = currentProductId <= 0 ? 0 : AppServices.Products.GetRois(currentProductId).Count;
            UpdateCanvasHint(roiCount);
        }

        private void RefreshVersionSummary()
        {
            if (currentProduct == null || currentSettings == null) return;
            lblVersion.Text = "当前产品：" + currentProduct.ProductCode + " / " + currentProduct.ProductName +
                              "    产品定义版本：" + (currentSettings.ProductDefinitionVersion ?? "PD-1.0.0") +
                              "    模板版本：" + (currentSettings.TemplateVersion ?? "LT-1.0.0");
        }

        private void ApplyLocalizationPreset(string preset)
        {
            if (string.Equals(preset, "快速模式", StringComparison.Ordinal))
            {
                SelectComboValue(cboLocalizationMethod, "HALCON Shape Matching", "HALCON Shape Matching");
                SelectComboValue(cboModelType, "Shape Model", "Shape Model");
                txtMinScore.Text = "0.60";
                txtAngleRange.Text = "-15 ~ 15 deg";
                txtScaleRange.Text = "0.95 ~ 1.05";
                txtMatchCount.Text = "1";
            }
            else if (string.Equals(preset, "精细模式", StringComparison.Ordinal))
            {
                SelectComboValue(cboLocalizationMethod, "HALCON Shape Matching", "HALCON Shape Matching");
                SelectComboValue(cboModelType, "Shape Model", "Shape Model");
                txtMinScore.Text = "0.85";
                txtAngleRange.Text = "-5 ~ 5 deg";
                txtScaleRange.Text = "0.98 ~ 1.02";
                txtMatchCount.Text = "1";
            }
            else
            {
                SelectComboValue(cboLocalizationMethod, "HALCON Shape Matching", "HALCON Shape Matching");
                SelectComboValue(cboModelType, "Shape Model", "Shape Model");
                txtMinScore.Text = "0.80";
                txtAngleRange.Text = "-180 ~ 180 deg";
                txtScaleRange.Text = "0.90 ~ 1.10";
                txtMatchCount.Text = "1";
            }

            txtLastResult.Text = "已应用" + preset + "参数，尚未执行定位测试";
            lblLastScore.Text = "最近测试：参数已更新";
        }

        private void ValidateTemplateConfiguration()
        {
            if (!EnsureSavedProduct()) return;

            try
            {
                BuildSettingsFromPage(currentProductId);
                List<string> problems = new List<string>();

                if (!string.Equals(txtProductCode.Text.Trim(), currentProduct.ProductCode, StringComparison.Ordinal) ||
                    !string.Equals(txtProductName.Text.Trim(), currentProduct.ProductName, StringComparison.Ordinal))
                {
                    problems.Add("产品名称或编号有未保存修改");
                }

                string referencePath = currentSettings == null ? null : currentSettings.ReferenceImagePath;
                if (string.IsNullOrWhiteSpace(referencePath) || !File.Exists(ResolveWorkspaceFile(referencePath)))
                    problems.Add("尚未导入有效基准图");

                IList<RoiDefinition> rois = AppServices.Products.GetRois(currentProductId);
                bool hasEnabledRoi = false;
                foreach (RoiDefinition roi in rois)
                {
                    if (roi.IsEnabled)
                    {
                        hasEnabledRoi = true;
                        break;
                    }
                }
                if (!hasEnabledRoi) problems.Add("至少需要一个已启用 ROI");

                if (problems.Count > 0)
                {
                    MessageBox.Show(
                        this,
                        "模板基础配置尚未就绪：\r\n\r\n- " + string.Join("\r\n- ", problems.ToArray()),
                        "模板配置校验",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(
                    this,
                    "基准图、定位参数和 ROI 配置完整。\r\n\r\n当前阶段仅完成配置校验；实际 Shape Model 文件将在接入 HALCON SDK 后生成。",
                    "模板配置已就绪",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "模板配置校验失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ExportDefectCategories()
        {
            if (!EnsureSavedProduct()) return;

            IList<DefectCategory> categories = AppServices.Products.GetDefectCategories(currentProductId);
            if (categories.Count == 0)
            {
                MessageBox.Show(this, "当前产品没有可导出的缺陷类别。", "导出配置", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV 配置文件|*.csv";
                dialog.DefaultExt = "csv";
                dialog.AddExtension = true;
                dialog.FileName = MakeSafeFileName(currentProduct.ProductCode) + "_defect_categories.csv";
                dialog.Title = "导出缺陷类别配置";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    DefectCategoryCsv.Write(dialog.FileName, categories);
                    MessageBox.Show(this, "已导出 " + categories.Count + " 个缺陷类别。", "导出完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "导出配置失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ImportDefectCategories()
        {
            if (!EnsureSavedProduct()) return;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "CSV 配置文件|*.csv|所有文件|*.*";
                dialog.Title = "导入缺陷类别配置";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    IList<DefectCategory> imported = DefectCategoryCsv.Read(dialog.FileName, currentProductId);
                    if (imported.Count == 0)
                        throw new InvalidOperationException("配置文件中没有缺陷类别数据。");

                    if (MessageBox.Show(
                        this,
                        "将按缺陷类别编码新增或更新 " + imported.Count + " 条配置；未出现在文件中的现有类别会保留。是否继续？",
                        "确认导入",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) != DialogResult.Yes) return;

                    IList<DefectCategory> existing = AppServices.Products.GetDefectCategories(currentProductId);
                    Dictionary<string, DefectCategory> existingByCode = new Dictionary<string, DefectCategory>(StringComparer.OrdinalIgnoreCase);
                    foreach (DefectCategory category in existing)
                        existingByCode[category.CategoryCode] = category;

                    int added = 0;
                    int updated = 0;
                    long selectedId = 0;
                    foreach (DefectCategory category in imported)
                    {
                        DefectCategory saved;
                        DefectCategory oldCategory;
                        if (existingByCode.TryGetValue(category.CategoryCode, out oldCategory))
                        {
                            category.Id = oldCategory.Id;
                            category.CreatedAtUtc = oldCategory.CreatedAtUtc;
                            saved = AppServices.Products.SaveDefectCategory(category);
                            updated++;
                        }
                        else
                        {
                            saved = AppServices.Products.SaveDefectCategory(category);
                            added++;
                        }
                        selectedId = saved.Id;
                    }

                    RefreshDefectGrid(selectedId);
                    MessageBox.Show(
                        this,
                        "导入完成。\r\n新增：" + added + "\r\n更新：" + updated,
                        "导入配置",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "导入配置失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private static string BuildNextDefectCode(IList<DefectCategory> categories)
        {
            HashSet<string> codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DefectCategory category in categories)
            {
                if (!string.IsNullOrWhiteSpace(category.CategoryCode))
                    codes.Add(category.CategoryCode.Trim());
            }

            for (int number = 1; number < 100000; number++)
            {
                string code = "DEFECT-" + number.ToString("000", CultureInfo.InvariantCulture);
                if (!codes.Contains(code)) return code;
            }

            throw new InvalidOperationException("无法生成新的缺陷类别编码。");
        }

        private void LoadReferencePreview(string filePath)
        {
            ClearReferencePreview();
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                referencePreviewMessage = "已记录的基准图文件不存在，请重新导入";
                return;
            }

            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (Image source = Image.FromStream(stream))
                {
                    pnlTemplateCanvas.BackgroundImage = new Bitmap(source);
                }
                referencePreviewMessage = null;
            }
            catch (Exception ex)
            {
                referencePreviewMessage = "基准图无法预览：" + ex.Message;
            }
        }

        private void ClearReferencePreview()
        {
            if (pnlTemplateCanvas == null) return;
            Image oldImage = pnlTemplateCanvas.BackgroundImage;
            pnlTemplateCanvas.BackgroundImage = null;
            if (oldImage != null) oldImage.Dispose();
        }

        private void UpdateCanvasHint(int roiCount)
        {
            if (pnlTemplateCanvas.BackgroundImage != null)
            {
                lblCanvasHint.Text = string.Empty;
                return;
            }

            if (!string.IsNullOrWhiteSpace(referencePreviewMessage))
            {
                lblCanvasHint.Text = referencePreviewMessage;
                return;
            }

            string productName = currentProduct == null ? "新产品" : currentProduct.ProductName;
            lblCanvasHint.Text = productName + "\r\n\r\n尚未导入基准图\r\n已保存 ROI：" + roiCount + " 个";
        }

        private static string ResolveWorkspaceFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            string root = Path.GetFullPath(ProjectStoragePaths.RootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string fullPath = Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(ProjectStoragePaths.RootPath, path));
            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("基准图路径超出当前 Workspace。请重新导入基准图。");
            return fullPath;
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
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException(fieldName + "必须是有限数值。");
            return value;
        }

        private static double ParseRangeDouble(string text, string fieldName, double minimum, double maximum)
        {
            double value = ParseDouble(text, fieldName);
            if (value < minimum || value > maximum)
                throw new ArgumentException(fieldName + "必须在" + minimum.ToString(CultureInfo.InvariantCulture) + "到" + maximum.ToString(CultureInfo.InvariantCulture) + "之间。");
            return value;
        }

        private static double ParsePositiveDouble(string text, string fieldName)
        {
            double value = ParseDouble(text, fieldName);
            if (value <= 0) throw new ArgumentException(fieldName + "必须大于0。");
            return value;
        }

        private static double ParseOptionalNonNegativeDouble(string text, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Trim() == "-") return 0;
            double value = ParseDouble(text, fieldName);
            if (value < 0) throw new ArgumentException(fieldName + "不能小于0。");
            return value;
        }

        private static string IncrementVersion(string version, string prefix)
        {
            string value = string.IsNullOrWhiteSpace(version) ? string.Empty : version.Trim();
            if (!string.IsNullOrEmpty(prefix) && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                value = value.Substring(prefix.Length);

            string[] parts = value.Split('.');
            int major;
            int minor;
            int patch;
            if (parts.Length != 3 ||
                !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out major) ||
                !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out minor) ||
                !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out patch) ||
                major < 0 || minor < 0 || patch < 0 || patch == int.MaxValue)
            {
                return prefix + "1.0.1";
            }

            return prefix + major.ToString(CultureInfo.InvariantCulture) + "." +
                   minor.ToString(CultureInfo.InvariantCulture) + "." +
                   (patch + 1).ToString(CultureInfo.InvariantCulture);
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
