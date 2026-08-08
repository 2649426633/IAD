using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using IAD.Models;
using IAD.Security;
using IAD.Services;

namespace IAD.Pages
{
    public partial class TemplateRecognitionPage : UserControl
    {
        private bool runtimeInitialized;
        private bool loadingData;
        private bool synchronizingCandidateSelection;
        private Product currentProduct;
        private IList<DefectRecognitionCandidate> currentCandidates = new List<DefectRecognitionCandidate>();
        private Bitmap queryPreview;
        private Bitmap heatmapPreview;

        public TemplateRecognitionPage()
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
            LoadRecognitionData();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (runtimeInitialized && Visible) LoadRecognitionData();
        }

        private void ConfigureRuntimeUi()
        {
            cboDefectClass.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDefectClass.Items.Clear();
            numSimilarity.Increment = 0.01M;
            numTopK.Increment = 1;
            btnSearchCandidates.Text = "生成识别候选";
            btnMaskRefine.Text = "边缘框精修";
            btnConfirm.Text = "确认并写入标注";
            btnReject.Text = "拒绝候选";
            btnAddHardNegative.Text = "加入难负样本";

            ConfigureGrid(dgvPositive);
            ConfigureGrid(dgvHardNegative);
            ConfigureGrid(dgvCandidates);
            ConfigureGrid(dgvCandidateList);
            dgvPositiveCol1.HeaderText = "标注ID";
            dgvPositiveCol2.HeaderText = "来源图片";
            dgvHardNegativeCol1.HeaderText = "负样本ID";
            dgvHardNegativeCol2.HeaderText = "来源图片";
            dgvHardNegativeCol3.HeaderText = "原相似度";
            grpHardNegative.Text = "难负样本（Hard Negative）";

            pnlQuery.BackgroundImageLayout = ImageLayout.Zoom;
            pnlHeatmap.BackgroundImageLayout = ImageLayout.Zoom;
            lblQuery.Text = "选择产品类别后，以已有标注作为正样本生成候选";
            lblHeatmap.Text = "候选相似度区域预览";
            lblLoopSteps.Text = "1 正样本 → 2 CPU 原型匹配 → 3 候选生成 → 4 边缘框精修 → 5 确认 / 难负样本";
            ClearPage("请先在“产品定义”中选择产品。", false);
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
            cboDefectClass.SelectedIndexChanged += delegate
            {
                if (!loadingData) LoadSelectedCategory(0);
            };
            btnSearchCandidates.Click += async delegate { await SearchCandidatesAsync(); };
            btnMaskRefine.Click += delegate { RefineSelectedCandidate(); };
            btnConfirm.Click += delegate { ConfirmSelectedCandidate(); };
            btnReject.Click += delegate { RejectSelectedCandidate(); };
            btnAddHardNegative.Click += delegate { AddSelectedHardNegative(); };
            dgvCandidates.SelectionChanged += delegate { CandidateSelectionChanged(dgvCandidates, dgvCandidateList); };
            dgvCandidateList.SelectionChanged += delegate { CandidateSelectionChanged(dgvCandidateList, dgvCandidates); };
            dgvCandidates.CellDoubleClick += delegate { ConfirmSelectedCandidate(); };
            dgvCandidateList.CellDoubleClick += delegate { ConfirmSelectedCandidate(); };
            AppSession.CurrentProductChanged += AppSession_CurrentProductChanged;
            Disposed += delegate
            {
                AppSession.CurrentProductChanged -= AppSession_CurrentProductChanged;
                ClearPreviews();
            };
        }

        private void AppSession_CurrentProductChanged(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate { AppSession_CurrentProductChanged(sender, e); }));
                return;
            }
            if (Visible) LoadRecognitionData();
        }

        private void LoadRecognitionData()
        {
            if (!runtimeInitialized || loadingData) return;
            long preferredCategoryId = GetSelectedCategory() == null ? 0 : GetSelectedCategory().Id;
            loadingData = true;
            try
            {
                currentProduct = null;
                if (AppSession.CurrentProductId > 0)
                    currentProduct = AppServices.Products.GetProduct(AppSession.CurrentProductId);

                cboDefectClass.Items.Clear();
                if (currentProduct == null)
                {
                    ClearPage("当前没有选择产品。请先到“产品定义”选择并保存产品。", false);
                    return;
                }

                foreach (DefectCategory category in AppServices.Products.GetDefectCategories(currentProduct.Id))
                {
                    if (!category.IsEnabled) continue;
                    cboDefectClass.Items.Add(new CategoryItem(category));
                }

                grpFewShot.Text = "瑕疵模板识别 | 当前产品：" + currentProduct.ProductCode + " · " + currentProduct.ProductName;
                int selectedIndex = 0;
                for (int i = 0; i < cboDefectClass.Items.Count; i++)
                {
                    CategoryItem item = cboDefectClass.Items[i] as CategoryItem;
                    if (item != null && item.Category.Id == preferredCategoryId) selectedIndex = i;
                }
                if (cboDefectClass.Items.Count > 0)
                    cboDefectClass.SelectedIndex = selectedIndex;
                else
                    ClearPage("当前产品没有已启用的瑕疵类别。请先到“产品定义”新增并启用类别。", true);
            }
            catch (Exception ex)
            {
                ClearPage("模板识别数据加载失败：" + ex.Message, currentProduct != null);
                MessageBox.Show(this, ex.Message, "加载模板识别失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                loadingData = false;
            }

            if (GetSelectedCategory() != null) LoadSelectedCategory(0);
        }

        private void LoadSelectedCategory(long preferredCandidateId)
        {
            DefectCategory category = GetSelectedCategory();
            if (currentProduct == null || category == null)
            {
                ClearPage("请选择一个已启用的瑕疵类别。", currentProduct != null);
                return;
            }

            loadingData = true;
            try
            {
                DefectRecognitionSettings settings = AppServices.DefectRecognition.GetSettings(currentProduct.Id, category.Id);
                numSimilarity.Value = Math.Max(numSimilarity.Minimum, Math.Min(numSimilarity.Maximum, (decimal)settings.SimilarityThreshold));
                numTopK.Value = Math.Max(numTopK.Minimum, Math.Min(numTopK.Maximum, settings.TopK));

                IList<DefectPrototypeSample> positives = AppServices.DefectRecognition.GetPositiveSamples(currentProduct.Id, category.Id);
                IList<DefectHardNegative> hardNegatives = AppServices.DefectRecognition.GetHardNegatives(currentProduct.Id, category.Id);
                currentCandidates = AppServices.DefectRecognition.GetLatestCandidates(currentProduct.Id, category.Id);
                FillPositiveSamples(positives);
                FillHardNegatives(hardNegatives);
                FillCandidates(preferredCandidateId);
                UpdateSummary(positives.Count, hardNegatives.Count);

                if (positives.Count == 0)
                    ShowPreviewMessage("该类别还没有正样本。\r\n请先到“数据集标注”中完成至少一个“" + category.CategoryName + "”标注。", "等待正样本");
                else if (currentCandidates.Count == 0)
                    ShowPreviewMessage("已加载 " + positives.Count + " 个正样本。\r\n点击“生成识别候选”扫描当前产品的数据集。", "尚未生成候选");
            }
            catch (Exception ex)
            {
                currentCandidates = new List<DefectRecognitionCandidate>();
                FillCandidates(0);
                ShowPreviewMessage("类别数据加载失败：\r\n" + ex.Message, "加载失败");
            }
            finally
            {
                loadingData = false;
            }

            if (currentCandidates.Count > 0) LoadSelectedCandidatePreview();
            UpdateAvailability();
        }

        private void FillPositiveSamples(IList<DefectPrototypeSample> samples)
        {
            dgvPositive.Rows.Clear();
            foreach (DefectPrototypeSample sample in samples)
            {
                int row = dgvPositive.Rows.Add("P-" + sample.Annotation.Id.ToString("0000"), sample.Image.FileName);
                dgvPositive.Rows[row].Tag = sample;
            }
            lblPositiveCount.Text = samples.Count.ToString();
        }

        private void FillHardNegatives(IList<DefectHardNegative> items)
        {
            dgvHardNegative.Rows.Clear();
            foreach (DefectHardNegative item in items)
            {
                int row = dgvHardNegative.Rows.Add(
                    "HN-" + item.Id.ToString("0000"), item.SourceFileName, item.Similarity.ToString("0.000"));
                dgvHardNegative.Rows[row].Tag = item;
            }
            lblHardNegativeCount.Text = items.Count.ToString();
        }

        private void FillCandidates(long preferredCandidateId)
        {
            synchronizingCandidateSelection = true;
            try
            {
                dgvCandidates.Rows.Clear();
                dgvCandidateList.Rows.Clear();
                int selectedRow = -1;
                for (int i = 0; i < currentCandidates.Count; i++)
                {
                    DefectRecognitionCandidate candidate = currentCandidates[i];
                    object[] values = { (i + 1).ToString(), candidate.Similarity.ToString("0.000"), candidate.SourceFileName, DisplayCandidateStatus(candidate.Status) };
                    int centerRow = dgvCandidates.Rows.Add(values);
                    int rightRow = dgvCandidateList.Rows.Add(values);
                    dgvCandidates.Rows[centerRow].Tag = candidate;
                    dgvCandidateList.Rows[rightRow].Tag = candidate;
                    ApplyCandidateRowStyle(dgvCandidates.Rows[centerRow], candidate.Status);
                    ApplyCandidateRowStyle(dgvCandidateList.Rows[rightRow], candidate.Status);
                    if (candidate.Id == preferredCandidateId) selectedRow = centerRow;
                }
                if (selectedRow < 0 && currentCandidates.Count > 0) selectedRow = 0;
                if (selectedRow >= 0)
                {
                    dgvCandidates.CurrentCell = dgvCandidates.Rows[selectedRow].Cells[0];
                    dgvCandidateList.CurrentCell = dgvCandidateList.Rows[selectedRow].Cells[0];
                }
            }
            finally
            {
                synchronizingCandidateSelection = false;
            }
        }

        private static void ApplyCandidateRowStyle(DataGridViewRow row, string status)
        {
            if (string.Equals(status, "已确认", StringComparison.Ordinal))
                row.DefaultCellStyle.BackColor = Color.FromArgb(226, 244, 230);
            else if (string.Equals(status, "已拒绝", StringComparison.Ordinal))
                row.DefaultCellStyle.BackColor = Color.FromArgb(242, 242, 242);
            else if (string.Equals(status, "Hard Negative", StringComparison.Ordinal))
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 238, 220);
        }

        private static string DisplayCandidateStatus(string status)
        {
            return string.Equals(status, "Hard Negative", StringComparison.Ordinal) ? "难负样本" : status;
        }

        private void CandidateSelectionChanged(DataGridView source, DataGridView other)
        {
            if (loadingData || synchronizingCandidateSelection || source.CurrentRow == null) return;
            DefectRecognitionCandidate selected = source.CurrentRow.Tag as DefectRecognitionCandidate;
            if (selected == null) return;

            synchronizingCandidateSelection = true;
            try
            {
                foreach (DataGridViewRow row in other.Rows)
                {
                    DefectRecognitionCandidate candidate = row.Tag as DefectRecognitionCandidate;
                    if (candidate == null || candidate.Id != selected.Id) continue;
                    other.CurrentCell = row.Cells[0];
                    row.Selected = true;
                    break;
                }
            }
            finally
            {
                synchronizingCandidateSelection = false;
            }
            LoadSelectedCandidatePreview();
            UpdateAvailability();
        }

        private async Task SearchCandidatesAsync()
        {
            DefectCategory category = GetSelectedCategory();
            if (currentProduct == null || category == null) return;
            DefectRecognitionSettings settings = new DefectRecognitionSettings
            {
                ProductId = currentProduct.Id,
                CategoryId = category.Id,
                SimilarityThreshold = (double)numSimilarity.Value,
                TopK = (int)numTopK.Value
            };

            btnSearchCandidates.Enabled = false;
            UseWaitCursor = true;
            grpCandidates.Text = "候选区域 | 正在执行 CPU 原型匹配…";
            try
            {
                IList<DefectRecognitionCandidate> generated = await Task.Run(
                    delegate { return AppServices.DefectRecognition.GenerateCandidates(settings); });
                LoadSelectedCategory(generated.Count > 0 ? generated[0].Id : 0);
                MessageBox.Show(this,
                    generated.Count == 0
                        ? "扫描完成，没有达到阈值的候选。可以适当降低相似度阈值后重试。"
                        : "扫描完成，已生成 " + generated.Count + " 个候选。\r\n双击候选也可以直接确认并写入标注。",
                    "候选生成完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "候选生成失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                UseWaitCursor = false;
                grpCandidates.Text = "候选区域（Top-K）";
                UpdateAvailability();
            }
        }

        private void ConfirmSelectedCandidate()
        {
            DefectRecognitionCandidate candidate = GetSelectedCandidate();
            DefectCategory category = GetSelectedCategory();
            if (currentProduct == null || category == null || candidate == null) return;
            if (MessageBox.Show(this,
                "将候选写入“" + category.CategoryName + "”标注。\r\n来源：" + candidate.SourceFileName +
                "\r\n相似度：" + candidate.Similarity.ToString("0.000") + "\r\n\r\n是否确认？",
                "确认识别候选", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                AppServices.DefectRecognition.ConfirmCandidate(currentProduct.Id, category.Id, candidate.Id);
                LoadSelectedCategory(candidate.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "确认候选失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RejectSelectedCandidate()
        {
            ExecuteCandidateAction("拒绝候选失败", delegate (DefectCategory category, DefectRecognitionCandidate candidate)
            {
                AppServices.DefectRecognition.RejectCandidate(currentProduct.Id, category.Id, candidate.Id);
            });
        }

        private void AddSelectedHardNegative()
        {
            ExecuteCandidateAction("加入 Hard Negative 失败", delegate (DefectCategory category, DefectRecognitionCandidate candidate)
            {
                AppServices.DefectRecognition.AddHardNegative(currentProduct.Id, category.Id, candidate.Id);
            });
        }

        private void RefineSelectedCandidate()
        {
            ExecuteCandidateAction("边缘框精修失败", delegate (DefectCategory category, DefectRecognitionCandidate candidate)
            {
                AppServices.DefectRecognition.RefineCandidate(currentProduct.Id, category.Id, candidate.Id);
            });
        }

        private void ExecuteCandidateAction(string errorTitle, Action<DefectCategory, DefectRecognitionCandidate> action)
        {
            DefectRecognitionCandidate candidate = GetSelectedCandidate();
            DefectCategory category = GetSelectedCategory();
            if (currentProduct == null || category == null || candidate == null) return;
            try
            {
                action(category, candidate);
                LoadSelectedCategory(candidate.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, errorTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadSelectedCandidatePreview()
        {
            DefectRecognitionCandidate candidate = GetSelectedCandidate();
            DefectCategory category = GetSelectedCategory();
            ClearPreviews();
            if (currentProduct == null || category == null || candidate == null) return;
            try
            {
                queryPreview = AppServices.DefectRecognition.BuildCandidatePreview(currentProduct.Id, category.Id, candidate.Id, false);
                heatmapPreview = AppServices.DefectRecognition.BuildCandidatePreview(currentProduct.Id, category.Id, candidate.Id, true);
                pnlQuery.BackgroundImage = queryPreview;
                pnlHeatmap.BackgroundImage = heatmapPreview;
                lblQuery.Visible = false;
                lblHeatmap.Visible = false;
                grpQuery.Text = "候选来源 | " + candidate.SourceFileName;
                grpHeatmap.Text = "相似度区域 | " + candidate.Similarity.ToString("0.000");
            }
            catch (Exception ex)
            {
                ShowPreviewMessage("候选预览失败：\r\n" + ex.Message, "预览失败");
            }
        }

        private void UpdateSummary(int positiveCount, int hardNegativeCount)
        {
            DefectCategory category = GetSelectedCategory();
            if (currentProduct == null || category == null)
            {
                lblLoopSummary.Text = "尚未选择产品类别";
                return;
            }
            DefectRecognitionSummary summary = AppServices.DefectRecognition.GetSummary(currentProduct.Id, category.Id);
            lblLoopSummary.Text = "本轮：已确认 " + summary.ConfirmedCount + "  /  已拒绝 " + summary.RejectedCount +
                                  "  /  待处理 " + summary.PendingCount + "  |  正样本 " + positiveCount +
                                  "  /  Hard Negative " + hardNegativeCount;
        }

        private void ClearPage(string message, bool hasProduct)
        {
            dgvPositive.Rows.Clear();
            dgvHardNegative.Rows.Clear();
            dgvCandidates.Rows.Clear();
            dgvCandidateList.Rows.Clear();
            currentCandidates = new List<DefectRecognitionCandidate>();
            lblPositiveCount.Text = "0";
            lblHardNegativeCount.Text = "0";
            lblLoopSummary.Text = message;
            grpFewShot.Text = hasProduct ? "瑕疵模板识别 | 当前产品尚未配置可用类别" : "瑕疵模板识别 | 尚未选择产品";
            ShowPreviewMessage(message, "识别准备");
            UpdateAvailability();
        }

        private void ShowPreviewMessage(string message, string title)
        {
            ClearPreviews();
            lblQuery.Text = message;
            lblHeatmap.Text = "生成候选后显示相似度区域";
            lblQuery.Visible = true;
            lblHeatmap.Visible = true;
            grpQuery.Text = title;
            grpHeatmap.Text = "相似度区域";
        }

        private void ClearPreviews()
        {
            pnlQuery.BackgroundImage = null;
            pnlHeatmap.BackgroundImage = null;
            if (queryPreview != null) queryPreview.Dispose();
            if (heatmapPreview != null) heatmapPreview.Dispose();
            queryPreview = null;
            heatmapPreview = null;
        }

        private DefectCategory GetSelectedCategory()
        {
            CategoryItem item = cboDefectClass.SelectedItem as CategoryItem;
            return item == null ? null : item.Category;
        }

        private DefectRecognitionCandidate GetSelectedCandidate()
        {
            DataGridView grid = dgvCandidateList.ContainsFocus ? dgvCandidateList : dgvCandidates;
            if (grid.CurrentRow == null) grid = grid == dgvCandidates ? dgvCandidateList : dgvCandidates;
            return grid.CurrentRow == null ? null : grid.CurrentRow.Tag as DefectRecognitionCandidate;
        }

        private void UpdateAvailability()
        {
            bool hasCategory = currentProduct != null && GetSelectedCategory() != null;
            bool hasPositive = dgvPositive.Rows.Count > 0;
            DefectRecognitionCandidate candidate = GetSelectedCandidate();
            bool pending = candidate != null && string.Equals(candidate.Status, "待确认", StringComparison.Ordinal);
            cboDefectClass.Enabled = currentProduct != null && cboDefectClass.Items.Count > 0;
            numSimilarity.Enabled = hasCategory;
            numTopK.Enabled = hasCategory;
            btnSearchCandidates.Enabled = hasCategory && hasPositive;
            btnMaskRefine.Enabled = pending;
            btnConfirm.Enabled = pending;
            btnReject.Enabled = pending;
            btnAddHardNegative.Enabled = candidate != null && !string.Equals(candidate.Status, "已确认", StringComparison.Ordinal) &&
                                               !string.Equals(candidate.Status, "Hard Negative", StringComparison.Ordinal);
        }

        public void RefreshCandidates()
        {
            if (runtimeInitialized) LoadSelectedCategory(0);
        }

        private sealed class CategoryItem
        {
            public CategoryItem(DefectCategory category) { Category = category; }
            public DefectCategory Category { get; private set; }
            public override string ToString() { return Category.CategoryName + " (" + Category.CategoryCode + ")"; }
        }
    }
}
