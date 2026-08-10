using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using IAD.Models;
using IAD.Services;
using IAD.Shell;
using IAD.UI;

namespace IAD.Pages
{
    public partial class DatasetAnnotationPage
    {
        private bool datasetWorkflowInitialized;
        private Button btnMarkNormalRuntime;
        private Button btnApproveRuntime;
        private Button btnDatasetManagerRuntime;
        private Button btnBatchRelabelRuntime;
        private ProgressBar importProgressRuntime;
        private Label lblImportProgressRuntime;
        private Button btnCancelImportRuntime;
        private CancellationTokenSource importCancellation;
        private DatasetAnnotation copiedAnnotation;

        private void InitializeDatasetWorkflowUi()
        {
            if (datasetWorkflowInitialized) return;
            datasetWorkflowInitialized = true;

            rootLayout.RowStyles[0].Height = 78F;
            toolbar.WrapContents = true;
            toolbar.AutoScroll = true;

            btnMarkNormalRuntime = CreateWorkflowButton("确认正常");
            btnApproveRuntime = CreateWorkflowButton("审核通过");
            btnDatasetManagerRuntime = CreateWorkflowButton("数据集管理");
            btnBatchRelabelRuntime = CreateWorkflowButton("批量改类");
            btnMarkNormalRuntime.Click += delegate { ApplyCurrentReviewStatus(DatasetReviewStatus.Normal); };
            btnApproveRuntime.Click += delegate { ApplyCurrentReviewStatus(DatasetReviewStatus.Approved); };
            btnDatasetManagerRuntime.Click += delegate { OpenDatasetManager(); };
            btnBatchRelabelRuntime.Click += delegate { BatchRelabelSelectedCategory(); };

            importProgressRuntime = new ProgressBar { Width = 150, Height = 18, Visible = false, Margin = new Padding(6, 7, 2, 0) };
            lblImportProgressRuntime = new Label { AutoSize = true, Visible = false, Margin = new Padding(3, 7, 2, 0), Text = "导入 0/0" };
            btnCancelImportRuntime = CreateWorkflowButton("取消导入");
            btnCancelImportRuntime.Visible = false;
            btnCancelImportRuntime.Click += delegate { if (importCancellation != null) importCancellation.Cancel(); };

            int versionIndex = toolbar.Controls.GetChildIndex(btnVersion);
            toolbar.Controls.Add(btnMarkNormalRuntime);
            toolbar.Controls.Add(btnApproveRuntime);
            toolbar.Controls.Add(btnDatasetManagerRuntime);
            toolbar.Controls.Add(btnBatchRelabelRuntime);
            toolbar.Controls.SetChildIndex(btnMarkNormalRuntime, versionIndex + 1);
            toolbar.Controls.SetChildIndex(btnApproveRuntime, versionIndex + 2);
            toolbar.Controls.SetChildIndex(btnDatasetManagerRuntime, versionIndex + 3);
            toolbar.Controls.SetChildIndex(btnBatchRelabelRuntime, versionIndex + 4);
            toolbar.Controls.Add(importProgressRuntime);
            toolbar.Controls.Add(lblImportProgressRuntime);
            toolbar.Controls.Add(btnCancelImportRuntime);

            dgvImages.SelectionChanged += delegate { ScheduleWorkflowRefresh(); };
            pnlCanvas.MouseUp += delegate { ScheduleWorkflowRefresh(); };
            ScheduleWorkflowRefresh();
        }

        private static Button CreateWorkflowButton(string text)
        {
            return new Button
            {
                Text = text,
                AutoSize = true,
                Height = 30,
                BackColor = UiTheme.Surface,
                UseVisualStyleBackColor = false
            };
        }

        private void ScheduleWorkflowRefresh()
        {
            if (IsDisposed || Disposing || !IsHandleCreated) return;
            BeginInvoke(new MethodInvoker(delegate
            {
                if (IsDisposed || Disposing) return;
                RefreshQualityMetrics();
                UpdateWorkflowButtons();
            }));
        }

        private void RefreshQualityMetrics()
        {
            if (currentImage == null)
            {
                lblTotalAnnotations.Text = "0";
                lblBoundaryScore.Text = "-";
                lblQualityScore.Text = "-";
                lblQualityAdvice.Text = "待选择图片";
                return;
            }
            try
            {
                DatasetImage latest = AppServices.Datasets.GetImage(currentImage.Id);
                if (latest != null)
                {
                    currentImage.Status = latest.Status;
                    currentImage.ReviewStatus = latest.ReviewStatus;
                    currentImage.DatasetSplit = latest.DatasetSplit;
                    currentImage.ReviewComment = latest.ReviewComment;
                    currentImage.ReviewedBy = latest.ReviewedBy;
                    currentImage.ReviewedAtUtc = latest.ReviewedAtUtc;
                }
                DatasetImageQuality quality = AppServices.DatasetWorkflow.EvaluateImage(currentImage.Id);
                lblTotalAnnotations.Text = quality.VectorAnnotationCount + " + " + quality.MaskCount + " Mask";
                lblBoundaryScore.Text = quality.BoundaryScore.ToString("0.00");
                lblQualityScore.Text = quality.QualityScore.ToString("0.00");
                lblQualityAdvice.Text = quality.Issues.Count == 0 ? "通过" : quality.Issues[0].Message;
                lblQualityAdvice.ForeColor = quality.CanApprove ? Color.FromArgb(28, 120, 76) : Color.FromArgb(180, 70, 55);
                grpQuality.Text = "标注质量 | " + ReviewText(currentImage.ReviewStatus) + " | " + SplitText(currentImage.DatasetSplit);
            }
            catch (Exception ex)
            {
                lblBoundaryScore.Text = "-";
                lblQualityScore.Text = "-";
                lblQualityAdvice.Text = ex.Message;
                lblQualityAdvice.ForeColor = Color.FromArgb(180, 70, 55);
            }
        }

        private static DatasetImage FindImage(IList<DatasetImage> items, long id)
        {
            foreach (DatasetImage item in items) if (item.Id == id) return item;
            return null;
        }

        private void ApplyCurrentReviewStatus(string status)
        {
            if (currentProduct == null || currentImage == null) return;
            try
            {
                AppServices.DatasetWorkflow.SetReviewStatus(currentProduct.Id, new[] { currentImage.Id }, status, null, Environment.UserName);
                LoadDataset(currentImage.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "更新审核状态失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenDatasetManager()
        {
            if (currentProduct == null)
            {
                MessageBox.Show(this, "请先选择并保存产品。", "数据集管理", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            long preferredImageId = currentImage == null ? 0 : currentImage.Id;
            using (DatasetManagerDialog dialog = new DatasetManagerDialog(currentProduct)) dialog.ShowDialog(this);
            ResetAllAnnotationHistory();
            LoadDataset(preferredImageId);
        }

        private bool TryPromptVersionNotes(out string notes)
        {
            notes = null;
            using (Form dialog = new Form())
            using (TextBox input = new TextBox())
            using (Button ok = new Button())
            using (Button cancel = new Button())
            {
                dialog.Text = "数据集版本说明";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(480, 150);
                dialog.Font = Font;

                Label prompt = new Label
                {
                    Text = "填写本次版本内容或用途（可留空）：",
                    AutoSize = true,
                    Location = new Point(14, 14)
                };
                input.Multiline = true;
                input.ScrollBars = ScrollBars.Vertical;
                input.MaxLength = 500;
                input.Location = new Point(14, 40);
                input.Size = new Size(450, 62);
                ok.Text = "发布";
                ok.DialogResult = DialogResult.OK;
                ok.Location = new Point(304, 112);
                cancel.Text = "取消";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Location = new Point(389, 112);
                dialog.Controls.Add(prompt);
                dialog.Controls.Add(input);
                dialog.Controls.Add(ok);
                dialog.Controls.Add(cancel);
                dialog.AcceptButton = ok;
                dialog.CancelButton = cancel;
                if (dialog.ShowDialog(this) != DialogResult.OK) return false;
                notes = string.IsNullOrWhiteSpace(input.Text) ? "由数据集标注页面发布" : input.Text.Trim();
                return true;
            }
        }

        private void UpdateWorkflowButtons()
        {
            bool hasImage = currentProduct != null && currentImage != null;
            if (btnMarkNormalRuntime != null) btnMarkNormalRuntime.Enabled = hasImage;
            if (btnApproveRuntime != null) btnApproveRuntime.Enabled = hasImage;
            if (btnDatasetManagerRuntime != null) btnDatasetManagerRuntime.Enabled = currentProduct != null;
            if (btnBatchRelabelRuntime != null) btnBatchRelabelRuntime.Enabled = hasImage && selectedAnnotationId > 0 && GetSelectedCategory() != null;
        }

        private void BatchRelabelSelectedCategory()
        {
            DatasetAnnotation selected = GetSelectedAnnotation();
            DefectCategory target = GetSelectedCategory();
            if (selected == null || target == null || currentImage == null) return;
            List<DatasetAnnotation> candidates = new List<DatasetAnnotation>();
            foreach (DatasetAnnotation annotation in currentAnnotations)
            {
                if (annotation.CategoryId == selected.CategoryId) candidates.Add(annotation);
            }
            if (candidates.Count == 0 || (candidates.Count == 1 && candidates[0].CategoryId == target.Id)) return;
            if (MessageBox.Show(this,
                "将当前图片中与选中标注同类别的 " + candidates.Count + " 个矢量标注统一改为“" + target.CategoryName + "”。是否继续？",
                "批量修改类别", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            int changed = 0;
            foreach (DatasetAnnotation annotation in candidates)
            {
                if (annotation.CategoryId == target.Id) continue;
                try
                {
                    DatasetAnnotation before = CloneAnnotation(annotation);
                    DatasetAnnotation after = CloneAnnotation(annotation);
                    after.CategoryId = target.Id;
                    after.CategoryCode = target.CategoryCode;
                    after.CategoryName = target.CategoryName;
                    after = AppServices.AnnotationEditing.Update(after);
                    AnnotationIdentity identity = GetIdentity(after.Id);
                    RegisterHistory(new AnnotationHistoryItem(
                        AnnotationHistoryKind.Update,
                        identity,
                        before,
                        CloneAnnotation(after)));
                    changed++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "已修改 " + changed + " 个标注，随后失败：" + ex.Message,
                        "批量修改类别", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                }
            }
            RefreshCurrentAnnotations();
        }

        private CancellationToken BeginImportProgress(int total)
        {
            if (importCancellation != null) importCancellation.Dispose();
            importCancellation = new CancellationTokenSource();
            importProgressRuntime.Minimum = 0;
            importProgressRuntime.Maximum = Math.Max(1, total);
            importProgressRuntime.Value = 0;
            importProgressRuntime.Visible = true;
            lblImportProgressRuntime.Text = "导入 0/" + total;
            lblImportProgressRuntime.Visible = true;
            btnCancelImportRuntime.Visible = true;
            btnImportImages.Enabled = false;
            btnImportFolder.Enabled = false;
            return importCancellation.Token;
        }

        private void ReportImportProgress(int completed, int total)
        {
            if (IsDisposed || Disposing) return;
            int value = Math.Max(importProgressRuntime.Minimum, Math.Min(importProgressRuntime.Maximum, completed));
            importProgressRuntime.Value = value;
            lblImportProgressRuntime.Text = "导入 " + completed + "/" + total;
        }

        private void EndImportProgress()
        {
            importProgressRuntime.Visible = false;
            lblImportProgressRuntime.Visible = false;
            btnCancelImportRuntime.Visible = false;
            btnImportImages.Enabled = currentProduct != null && currentProductDefinition != null;
            btnImportFolder.Enabled = btnImportImages.Enabled;
            if (importCancellation != null)
            {
                importCancellation.Dispose();
                importCancellation = null;
            }
        }

        private void SelectRelativeImage(int delta)
        {
            if (dgvImages.Rows.Count == 0) return;
            int index = 0;
            if (dgvImages.SelectedRows.Count > 0) index = dgvImages.SelectedRows[0].Index;
            index = Math.Max(0, Math.Min(dgvImages.Rows.Count - 1, index + delta));
            dgvImages.ClearSelection();
            dgvImages.Rows[index].Selected = true;
            dgvImages.CurrentCell = dgvImages.Rows[index].Cells[0];
        }

        private void SelectCategoryShortcut(int index)
        {
            if (index < 0 || index >= cboCurrentClass.Items.Count) return;
            cboCurrentClass.SelectedIndex = index;
            pnlCanvas.Focus();
        }

        private void CopySelectedAnnotation()
        {
            DatasetAnnotation selected = GetSelectedAnnotation();
            if (selected == null) return;
            copiedAnnotation = CloneAnnotation(selected);
        }

        private void PasteCopiedAnnotation()
        {
            if (copiedAnnotation == null || currentImage == null) return;
            try
            {
                DatasetAnnotation snapshot = CloneAnnotation(copiedAnnotation);
                snapshot.Id = 0;
                snapshot.DatasetImageId = currentImage.Id;
                IList<PointF> points = AnnotationGeometry.Parse(snapshot.GeometryData);
                List<PointF> shifted = new List<PointF>();
                foreach (PointF point in points)
                    shifted.Add(new PointF(Math.Min(currentImage.Width, point.X + 10F), Math.Min(currentImage.Height, point.Y + 10F)));
                snapshot.GeometryData = AnnotationGeometry.Serialize(shifted);
                DatasetAnnotation created = AppServices.AnnotationEditing.Recreate(snapshot);
                AnnotationIdentity identity = GetIdentity(created.Id);
                RegisterHistory(new AnnotationHistoryItem(AnnotationHistoryKind.Create, identity, null, CloneAnnotation(created)));
                selectedAnnotationId = created.Id;
                RefreshCurrentAnnotations();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "粘贴标注失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private sealed class DatasetImportBatchSummary
        {
            public int Imported;
            public int Duplicates;
            public long PreferredImageId;
            public bool Cancelled;
            public readonly List<string> Failures = new List<string>();
        }
    }
}
