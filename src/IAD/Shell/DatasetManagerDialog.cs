using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using IAD.Models;
using IAD.Services;

namespace IAD.Shell
{
    internal sealed partial class DatasetManagerDialog : Form
    {
        private readonly Product product;
        private IList<DatasetImage> images = new List<DatasetImage>();
        private int imageRefreshGeneration;

        public DatasetManagerDialog(Product product)
        {
            this.product = product ?? throw new ArgumentNullException("product");
            InitializeComponent();
            Text = "数据集管理 · " + product.ProductCode + " · " + product.ProductName;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AppServices.EnsureInitialized();
            txtDestination.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "IAD_Exports");
            RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshImages();
            RefreshVersions();
        }

        private async void RefreshImages()
        {
            long[] selected = SelectedImageIds();
            HashSet<long> selectedIds = new HashSet<long>(selected);
            int generation = ++imageRefreshGeneration;
            lblSummary.Text = "正在后台检查图片、矢量标注和 Mask 质量…";
            dgvImages.Enabled = false;
            try
            {
                IList<DatasetImage> loadedImages = AppServices.Datasets.GetImages(product.Id);
                IList<DatasetQualityRow> rows = await Task.Run(delegate
                {
                    List<DatasetQualityRow> result = new List<DatasetQualityRow>();
                    foreach (DatasetImage image in loadedImages)
                    {
                        DatasetImageQuality quality;
                        try { quality = AppServices.DatasetWorkflow.EvaluateImage(image.Id); }
                        catch (Exception ex)
                        {
                            quality = new DatasetImageQuality { ImageId = image.Id, FileName = image.FileName };
                            quality.Issues.Add(new DatasetQualityIssue { Severity = "Error", Message = ex.Message });
                        }
                        result.Add(new DatasetQualityRow { Image = image, Quality = quality });
                    }
                    return result;
                });
                if (IsDisposed || Disposing || generation != imageRefreshGeneration) return;

                images = loadedImages;
                dgvImages.Rows.Clear();
                int passed = 0;
                int errors = 0;
                foreach (DatasetQualityRow item in rows)
                {
                    DatasetImage image = item.Image;
                    DatasetImageQuality quality = item.Quality;
                    if (quality.CanApprove) passed++; else errors++;
                    int rowIndex = dgvImages.Rows.Add(
                        image.FileName,
                        ReviewText(image.ReviewStatus),
                        SplitText(image.DatasetSplit),
                        quality.VectorAnnotationCount + " / " + quality.MaskCount,
                        quality.QualityScore.ToString("0.00"),
                        quality.Issues.Count == 0 ? "通过" : quality.Issues[0].Message,
                        image.ReviewedBy ?? string.Empty);
                    dgvImages.Rows[rowIndex].Tag = image;
                    if (selectedIds.Contains(image.Id)) dgvImages.Rows[rowIndex].Selected = true;
                }
                lblSummary.Text = "图片 " + images.Count + " | 质量可通过 " + passed + " | 待处理 " + errors +
                                  " | 选择多行后可批量设置审核状态和数据划分";
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing && generation == imageRefreshGeneration)
                    lblSummary.Text = "质量检查失败：" + ex.Message;
            }
            finally
            {
                if (!IsDisposed && !Disposing && generation == imageRefreshGeneration) dgvImages.Enabled = true;
            }
        }

        private void RefreshVersions()
        {
            dgvVersions.Rows.Clear();
            foreach (DatasetVersion version in AppServices.Datasets.GetVersions(product.Id))
            {
                int rowIndex = dgvVersions.Rows.Add(
                    version.VersionCode,
                    version.ProductDefinitionVersion,
                    version.ImageCount,
                    version.AnnotationCount,
                    version.MaskCount,
                    version.CreatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                    version.Notes);
                dgvVersions.Rows[rowIndex].Tag = version;
            }
        }

        private long[] SelectedImageIds()
        {
            List<long> ids = new List<long>();
            foreach (DataGridViewRow row in dgvImages.SelectedRows)
            {
                DatasetImage image = row.Tag as DatasetImage;
                if (image != null) ids.Add(image.Id);
            }
            return ids.ToArray();
        }

        private void ApplyReview(string status)
        {
            long[] ids = SelectedImageIds();
            if (ids.Length == 0)
            {
                MessageBox.Show(this, "请先选择至少一张图片。", "审核状态", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                AppServices.DatasetWorkflow.SetReviewStatus(product.Id, ids, status, txtReviewComment.Text, Environment.UserName);
                RefreshImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "更新审核状态失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplySplit(string split)
        {
            long[] ids = SelectedImageIds();
            if (ids.Length == 0)
            {
                MessageBox.Show(this, "请先选择至少一张图片。", "数据集划分", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                AppServices.DatasetWorkflow.SetSplit(product.Id, ids, split);
                RefreshImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "更新数据集划分失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AssignSplits()
        {
            int train = (int)numTrain.Value;
            int validation = (int)numValidation.Value;
            if (train + validation > 100)
            {
                MessageBox.Show(this, "训练集与验证集比例之和不能超过 100%。", "自动划分", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                AppServices.DatasetWorkflow.AssignSplits(product.Id, train, validation, (int)numSeed.Value);
                RefreshImages();
                MessageBox.Show(this, "已按 Train " + train + "% / Validation " + validation + "% / Test " + (100 - train - validation) + "% 完成确定性划分。",
                    "自动划分完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "自动划分失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BrowseDestination()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择训练数据导出目录";
                if (Directory.Exists(txtDestination.Text)) dialog.SelectedPath = txtDestination.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK) txtDestination.Text = dialog.SelectedPath;
            }
        }

        private DatasetExportOptions CreateExportOptions()
        {
            return new DatasetExportOptions
            {
                DestinationDirectory = txtDestination.Text.Trim(),
                ExportCoco = chkCoco.Checked,
                ExportYolo = chkYolo.Checked,
                ExportMasks = chkMasks.Checked,
                ApprovedOnly = chkApprovedOnly.Checked,
                RequireQualityGate = chkQualityGate.Checked
            };
        }

        private void ExportCurrent()
        {
            RunExport(delegate { return AppServices.DatasetWorkflow.ExportCurrent(product.Id, CreateExportOptions()); });
        }

        private void ImportCoco()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "选择 COCO instances.json";
                dialog.Filter = "COCO JSON (*.json)|*.json|所有文件 (*.*)|*.*";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                string path = dialog.FileName;
                RunImportAsync(delegate { return AppServices.DatasetWorkflow.ImportCoco(product.Id, path); });
            }
        }

        private void ImportYolo()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择包含 images、labels、classes.txt/dataset.yaml 的 YOLO 数据集目录";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                string path = dialog.SelectedPath;
                RunImportAsync(delegate { return AppServices.DatasetWorkflow.ImportYolo(product.Id, path); });
            }
        }

        private async void RunImportAsync(Func<DatasetImportResult> import)
        {
            Enabled = false;
            UseWaitCursor = true;
            try
            {
                DatasetImportResult result = await Task.Run(import);
                if (IsDisposed || Disposing) return;
                RefreshAll();
                string message = "导入完成。\r\n\r\n新增图片：" + result.ImageCount +
                                 "\r\n重复图片：" + result.DuplicateImageCount +
                                 "\r\n新增矢量标注：" + result.AnnotationCount +
                                 "\r\n写入 Mask：" + result.MaskCount;
                if (result.Warnings.Count > 0)
                {
                    message += "\r\n\r\n提示 " + result.Warnings.Count + " 项：\r\n";
                    for (int i = 0; i < Math.Min(8, result.Warnings.Count); i++) message += result.Warnings[i] + "\r\n";
                    if (result.Warnings.Count > 8) message += "其余 " + (result.Warnings.Count - 8) + " 项未展开。";
                }
                MessageBox.Show(this, message.TrimEnd(), result.Warnings.Count == 0 ? "导入完成" : "导入完成（有提示）",
                    MessageBoxButtons.OK, result.Warnings.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.Show(this, ex.Message, "训练数据导入失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    UseWaitCursor = false;
                    Enabled = true;
                }
            }
        }

        private void ExportSelectedVersion()
        {
            if (dgvVersions.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "请先选择一个历史版本。", "版本导出", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DatasetVersion version = dgvVersions.SelectedRows[0].Tag as DatasetVersion;
            if (version == null) return;
            RunExport(delegate { return AppServices.DatasetWorkflow.ExportVersion(product.Id, version.Id, CreateExportOptions()); });
        }

        private void CompareSelectedVersions()
        {
            if (dgvVersions.SelectedRows.Count != 2)
            {
                MessageBox.Show(this, "请按住 Ctrl 选择两个历史版本。", "版本比较", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DatasetVersion first = dgvVersions.SelectedRows[0].Tag as DatasetVersion;
            DatasetVersion second = dgvVersions.SelectedRows[1].Tag as DatasetVersion;
            if (first == null || second == null) return;
            try
            {
                DatasetVersionComparison comparison = AppServices.DatasetWorkflow.CompareVersions(product.Id, first.Id, second.Id);
                MessageBox.Show(this,
                    comparison.LeftVersionCode + " → " + comparison.RightVersionCode +
                    "\r\n\r\n图片：新增 " + comparison.AddedImages + "，移除 " + comparison.RemovedImages +
                    "\r\n矢量标注：新增 " + comparison.AddedAnnotations + "，移除 " + comparison.RemovedAnnotations +
                    "\r\nMask：新增 " + comparison.AddedMasks + "，移除 " + comparison.RemovedMasks +
                    "\r\n数据划分变化：" + comparison.SplitChanges +
                    "\r\n审核状态变化：" + comparison.ReviewChanges,
                    "版本比较", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "版本比较失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RestoreSelectedVersion()
        {
            if (dgvVersions.SelectedRows.Count != 1)
            {
                MessageBox.Show(this, "请选择一个需要恢复的历史版本。", "恢复版本", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DatasetVersion version = dgvVersions.SelectedRows[0].Tag as DatasetVersion;
            if (version == null) return;
            DialogResult answer = MessageBox.Show(this,
                "将使用 “" + version.VersionCode + "” 的图片、矢量标注、Mask、审核状态和数据划分替换当前工作集。\r\n\r\n" +
                "已有历史版本不会被修改。建议先发布当前工作集作为备份。是否继续？",
                "恢复历史版本", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer != DialogResult.Yes) return;
            try
            {
                AppServices.Datasets.RestoreVersion(product.Id, version.Id);
                AppServices.Masks.CleanupOrphanFiles();
                RefreshAll();
                MessageBox.Show(this, "已恢复 " + version.VersionCode + "。", "恢复完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "恢复版本失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void RunExport(Func<DatasetExportResult> export)
        {
            Enabled = false;
            UseWaitCursor = true;
            try
            {
                DatasetExportResult result = await Task.Run(export);
                if (IsDisposed || Disposing) return;
                string message = "导出完成。\r\n\r\n图片：" + result.ImageCount +
                                 "\r\n矢量标注：" + result.AnnotationCount +
                                 "\r\nMask：" + result.MaskCount +
                                 "\r\n目录：" + result.OutputDirectory;
                if (result.Warnings.Count > 0) message += "\r\n\r\n提示：" + string.Join("\r\n", new List<string>(result.Warnings).ToArray());
                if (MessageBox.Show(this, message + "\r\n\r\n是否打开导出目录？", "训练数据导出",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    Process.Start("explorer.exe", result.OutputDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "导出失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    UseWaitCursor = false;
                    Enabled = true;
                }
            }
        }

        private static string ReviewText(string status)
        {
            if (string.Equals(status, DatasetReviewStatus.Normal, StringComparison.OrdinalIgnoreCase)) return "正常样本";
            if (string.Equals(status, DatasetReviewStatus.Approved, StringComparison.OrdinalIgnoreCase)) return "已通过";
            if (string.Equals(status, DatasetReviewStatus.Rejected, StringComparison.OrdinalIgnoreCase)) return "已驳回";
            if (string.Equals(status, DatasetReviewStatus.Ignored, StringComparison.OrdinalIgnoreCase)) return "已忽略";
            return "待审核";
        }

        private static string SplitText(string split)
        {
            if (string.Equals(split, DatasetSplit.Train, StringComparison.OrdinalIgnoreCase)) return "Train";
            if (string.Equals(split, DatasetSplit.Validation, StringComparison.OrdinalIgnoreCase)) return "Validation";
            if (string.Equals(split, DatasetSplit.Test, StringComparison.OrdinalIgnoreCase)) return "Test";
            return "未划分";
        }

        private sealed class DatasetQualityRow
        {
            public DatasetImage Image { get; set; }
            public DatasetImageQuality Quality { get; set; }
        }
    }
}
