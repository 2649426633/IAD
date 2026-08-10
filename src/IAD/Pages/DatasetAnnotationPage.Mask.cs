using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using IAD.Models;
using IAD.Services;
using IAD.UI;

namespace IAD.Pages
{
    public partial class DatasetAnnotationPage
    {
        private const int MaxMaskHistory = 20;

        private bool maskRuntimeInitialized;
        private bool maskLateEventsBound;
        private bool maskModeActive;
        private bool maskStrokeActive;
        private bool maskStrokeErase;
        private bool maskStrokeChanged;
        private PointF maskLastImagePoint;
        private byte[] maskStrokeBefore;
        private long maskContextImageId = -1;
        private long maskContextCategoryId = -1;
        private DatasetMask currentMaskRecord;
        private Bitmap currentMaskBitmap;
        private readonly List<byte[]> maskUndoHistory = new List<byte[]>();
        private readonly List<byte[]> maskRedoHistory = new List<byte[]>();

        private Button btnEditAnnotationRuntime;
        private FlowLayoutPanel maskToolPanel;
        private Button btnMaskAdd;
        private Button btnMaskErase;
        private Button btnMaskRasterize;
        private Button btnMaskUndo;
        private Button btnMaskRedo;
        private Button btnMaskClear;
        private Button btnMaskExit;
        private Label lblMaskState;

        private enum PixelMaskPaintMode
        {
            Add,
            Erase
        }

        private PixelMaskPaintMode maskPaintMode = PixelMaskPaintMode.Add;

        private void InitializeMaskEditorRuntime()
        {
            if (maskRuntimeInitialized) return;
            maskRuntimeInitialized = true;

            btnMaskEdit.Text = "Mask编辑";
            numLineWidth.Maximum = 256;
            CreateAnnotationEditButton();
            CreateMaskToolPanel();

            Disposed += delegate { DisposeMaskEditor(); };

            if (IsHandleCreated)
                BeginInvoke(new MethodInvoker(BindMaskEventsLate));
        }

        private void BindMaskEventsLate()
        {
            if (maskLateEventsBound || IsDisposed || Disposing) return;
            maskLateEventsBound = true;

            btnMaskEdit.Click += delegate
            {
                BeginInvoke(new MethodInvoker(delegate
                {
                    if (maskModeActive) ExitMaskMode();
                    else EnterMaskMode();
                }));
            };

            btnRectangle.Click += delegate { LeaveMaskModeForVectorTool(); };
            btnPolygon.Click += delegate { LeaveMaskModeForVectorTool(); };
            btnBrush.Click += delegate { LeaveMaskModeForVectorTool(); };
            btnEraser.Click += delegate { LeaveMaskModeForVectorTool(); };

            dgvImages.SelectionChanged += delegate
            {
                if (IsDisposed || Disposing) return;
                BeginInvoke(new MethodInvoker(LoadMaskForCurrentContext));
            };
            cboCurrentClass.SelectedIndexChanged += delegate
            {
                if (IsDisposed || Disposing) return;
                BeginInvoke(new MethodInvoker(LoadMaskForCurrentContext));
            };

            pnlCanvas.Paint += pnlCanvas_MaskPaint;
            pnlCanvas.MouseDown += pnlCanvas_MaskMouseDown;
            pnlCanvas.MouseMove += pnlCanvas_MaskMouseMove;
            pnlCanvas.MouseUp += pnlCanvas_MaskMouseUp;
        }

        private void CreateAnnotationEditButton()
        {
            btnEditAnnotationRuntime = new Button
            {
                Text = "编辑标注",
                AutoSize = true,
                Height = btnMaskEdit.Height,
                Margin = btnMaskEdit.Margin,
                BackColor = UiTheme.Surface,
                UseVisualStyleBackColor = false
            };
            btnEditAnnotationRuntime.Click += delegate
            {
                LeaveMaskModeVisualOnly();
                SetActiveTool("Select");
                btnEditAnnotationRuntime.BackColor = Color.FromArgb(210, 225, 242);
                btnMaskEdit.BackColor = UiTheme.Surface;
                pnlCanvas.Focus();
            };

            int maskIndex = toolbar.Controls.GetChildIndex(btnMaskEdit);
            toolbar.Controls.Add(btnEditAnnotationRuntime);
            toolbar.Controls.SetChildIndex(btnEditAnnotationRuntime, Math.Max(0, maskIndex));
        }

        private void CreateMaskToolPanel()
        {
            maskToolPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(6),
                Margin = Padding.Empty,
                Visible = false,
                Location = new Point(8, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            btnMaskAdd = CreateMaskPanelButton("增加Mask");
            btnMaskErase = CreateMaskPanelButton("擦除Mask");
            btnMaskRasterize = CreateMaskPanelButton("由标注生成");
            btnMaskUndo = CreateMaskPanelButton("撤销Mask");
            btnMaskRedo = CreateMaskPanelButton("重做Mask");
            btnMaskClear = CreateMaskPanelButton("清空Mask");
            btnMaskExit = CreateMaskPanelButton("退出Mask");
            lblMaskState = new Label
            {
                AutoSize = true,
                Margin = new Padding(10, 7, 4, 0),
                ForeColor = UiTheme.Text,
                Text = "Mask：未加载"
            };

            btnMaskAdd.Click += delegate { SetMaskPaintMode(PixelMaskPaintMode.Add); };
            btnMaskErase.Click += delegate { SetMaskPaintMode(PixelMaskPaintMode.Erase); };
            btnMaskRasterize.Click += delegate { RasterizeCurrentCategoryToMask(); };
            btnMaskUndo.Click += delegate { UndoMaskEdit(); };
            btnMaskRedo.Click += delegate { RedoMaskEdit(); };
            btnMaskClear.Click += delegate { ClearCurrentMask(); };
            btnMaskExit.Click += delegate { ExitMaskMode(); };

            maskToolPanel.Controls.Add(btnMaskAdd);
            maskToolPanel.Controls.Add(btnMaskErase);
            maskToolPanel.Controls.Add(btnMaskRasterize);
            maskToolPanel.Controls.Add(btnMaskUndo);
            maskToolPanel.Controls.Add(btnMaskRedo);
            maskToolPanel.Controls.Add(btnMaskClear);
            maskToolPanel.Controls.Add(btnMaskExit);
            maskToolPanel.Controls.Add(lblMaskState);

            pnlCanvas.Controls.Add(maskToolPanel);
            maskToolPanel.BringToFront();
            UpdateMaskToolState();
        }

        private static Button CreateMaskPanelButton(string text)
        {
            return new Button
            {
                AutoSize = true,
                Text = text,
                Height = 28,
                Margin = new Padding(2),
                BackColor = UiTheme.Surface,
                UseVisualStyleBackColor = false
            };
        }

        private void EnterMaskMode()
        {
            if (currentImage == null || currentBitmap == null)
            {
                MessageBox.Show(this, "请先导入并选择一张图片。", "Mask 编辑", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DefectCategory category = GetSelectedCategory();
            if (category == null)
            {
                MessageBox.Show(this, "请选择一个已启用的瑕疵类别后再编辑 Mask。", "Mask 编辑", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            maskModeActive = true;
            SetActiveTool("Mask");
            selectedAnnotationId = 0;
            btnMaskEdit.Text = "Mask编辑";
            btnMaskEdit.BackColor = Color.FromArgb(210, 225, 242);
            if (btnEditAnnotationRuntime != null) btnEditAnnotationRuntime.BackColor = UiTheme.Surface;
            if (lblLineWidthKey != null) lblLineWidthKey.Text = "Mask笔刷";
            maskToolPanel.Visible = true;
            maskToolPanel.BringToFront();
            SetMaskPaintMode(PixelMaskPaintMode.Add);
            LoadMaskForCurrentContext();
            pnlCanvas.Focus();
            pnlCanvas.Invalidate();
        }

        private void ExitMaskMode()
        {
            LeaveMaskModeVisualOnly();
            SetActiveTool("Select");
            if (btnEditAnnotationRuntime != null) btnEditAnnotationRuntime.BackColor = Color.FromArgb(210, 225, 242);
            pnlCanvas.Invalidate();
        }

        private void LeaveMaskModeForVectorTool()
        {
            if (!maskModeActive) return;
            LeaveMaskModeVisualOnly();
        }

        private void LeaveMaskModeVisualOnly()
        {
            maskModeActive = false;
            maskStrokeActive = false;
            maskStrokeBefore = null;
            if (maskToolPanel != null) maskToolPanel.Visible = false;
            if (btnMaskEdit != null) btnMaskEdit.BackColor = UiTheme.Surface;
            if (lblLineWidthKey != null) lblLineWidthKey.Text = "线宽";
        }

        private void LoadMaskForCurrentContext()
        {
            if (!maskRuntimeInitialized || IsDisposed || Disposing) return;
            DefectCategory category = GetSelectedCategory();
            long imageId = currentImage == null ? 0 : currentImage.Id;
            long categoryId = category == null ? 0 : category.Id;

            if (imageId <= 0 || categoryId <= 0)
            {
                ResetMaskContext();
                UpdateMaskToolState();
                pnlCanvas.Invalidate();
                return;
            }

            bool contextChanged = imageId != maskContextImageId || categoryId != maskContextCategoryId;
            if (!contextChanged && currentMaskBitmap != null)
            {
                UpdateMaskToolState();
                return;
            }

            DisposeCurrentMaskBitmap();
            currentMaskRecord = null;
            maskContextImageId = imageId;
            maskContextCategoryId = categoryId;
            maskUndoHistory.Clear();
            maskRedoHistory.Clear();

            try
            {
                currentMaskRecord = AppServices.Masks.GetMask(imageId, categoryId);
                currentMaskBitmap = currentMaskRecord == null
                    ? new Bitmap(currentImage.Width, currentImage.Height, PixelFormat.Format32bppArgb)
                    : AppServices.Masks.LoadEditableBitmap(currentMaskRecord);
            }
            catch (Exception ex)
            {
                currentMaskBitmap = null;
                MessageBox.Show(this, ex.Message, "加载 Mask 失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            UpdateMaskToolState();
            pnlCanvas.Invalidate();
        }

        private void SetMaskPaintMode(PixelMaskPaintMode mode)
        {
            maskPaintMode = mode;
            if (btnMaskAdd != null)
                btnMaskAdd.BackColor = mode == PixelMaskPaintMode.Add ? Color.FromArgb(210, 225, 242) : UiTheme.Surface;
            if (btnMaskErase != null)
                btnMaskErase.BackColor = mode == PixelMaskPaintMode.Erase ? Color.FromArgb(225, 225, 225) : UiTheme.Surface;
            if (pnlCanvas != null) pnlCanvas.Cursor = Cursors.Cross;
        }

        private void pnlCanvas_MaskPaint(object sender, PaintEventArgs e)
        {
            if (!maskModeActive || currentMaskBitmap == null || currentImage == null) return;
            RectangleF imageBounds = GetImageDisplayBounds();
            if (imageBounds.Width <= 0 || imageBounds.Height <= 0) return;

            using (ImageAttributes attributes = new ImageAttributes())
            {
                ColorMatrix matrix = new ColorMatrix
                {
                    Matrix00 = 1F,
                    Matrix11 = 1F,
                    Matrix22 = 1F,
                    Matrix33 = 0.42F,
                    Matrix44 = 1F
                };
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                e.Graphics.DrawImage(
                    currentMaskBitmap,
                    Rectangle.Round(imageBounds),
                    0,
                    0,
                    currentMaskBitmap.Width,
                    currentMaskBitmap.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            maskToolPanel.BringToFront();
        }

        private void pnlCanvas_MaskMouseDown(object sender, MouseEventArgs e)
        {
            if (!maskModeActive || currentMaskBitmap == null || currentImage == null) return;
            if (panningCanvas || e.Button == MouseButtons.Middle || spacePanHeld) return;
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;

            PointF imagePoint;
            if (!TryCanvasToImage(e.Location, out imagePoint)) return;

            maskStrokeActive = true;
            maskStrokeChanged = false;
            maskStrokeErase = e.Button == MouseButtons.Right || maskPaintMode == PixelMaskPaintMode.Erase;
            maskLastImagePoint = imagePoint;
            maskStrokeBefore = CaptureMaskSnapshot();
            DrawMaskSegment(imagePoint, imagePoint, maskStrokeErase);
            maskStrokeChanged = true;
            pnlCanvas.Invalidate();
        }

        private void pnlCanvas_MaskMouseMove(object sender, MouseEventArgs e)
        {
            if (!maskModeActive || !maskStrokeActive || currentMaskBitmap == null) return;
            PointF imagePoint;
            if (!TryCanvasToImage(e.Location, out imagePoint)) return;

            DrawMaskSegment(maskLastImagePoint, imagePoint, maskStrokeErase);
            maskLastImagePoint = imagePoint;
            maskStrokeChanged = true;
            pnlCanvas.Invalidate();
        }

        private void pnlCanvas_MaskMouseUp(object sender, MouseEventArgs e)
        {
            if (!maskModeActive || !maskStrokeActive) return;
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;

            maskStrokeActive = false;
            if (!maskStrokeChanged)
            {
                maskStrokeBefore = null;
                return;
            }

            try
            {
                PersistCurrentMask();
                PushMaskHistory(maskUndoHistory, maskStrokeBefore);
                maskRedoHistory.Clear();
            }
            catch (Exception ex)
            {
                RestoreMaskSnapshot(maskStrokeBefore);
                MessageBox.Show(this, ex.Message, "保存 Mask 失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                maskStrokeBefore = null;
                maskStrokeChanged = false;
                UpdateMaskToolState();
                pnlCanvas.Invalidate();
            }
        }

        private void DrawMaskSegment(PointF from, PointF to, bool erase)
        {
            if (currentMaskBitmap == null) return;
            float width = Math.Max(1F, (float)numLineWidth.Value);
            using (Graphics graphics = Graphics.FromImage(currentMaskBitmap))
            {
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.PixelOffsetMode = PixelOffsetMode.None;
                graphics.CompositingMode = CompositingMode.SourceCopy;
                Color color = erase ? Color.Transparent : Color.White;
                using (Pen pen = new Pen(color, width))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    pen.LineJoin = LineJoin.Round;
                    graphics.DrawLine(pen, from, to);
                }
                float radius = width / 2F;
                using (SolidBrush brush = new SolidBrush(color))
                    graphics.FillEllipse(brush, to.X - radius, to.Y - radius, width, width);
            }
        }

        private void PersistCurrentMask()
        {
            DefectCategory category = GetSelectedCategory();
            if (currentImage == null || category == null || currentMaskBitmap == null)
                throw new InvalidOperationException("当前 Mask 编辑上下文无效。");

            currentMaskRecord = AppServices.Masks.SaveMask(currentImage.Id, category.Id, currentMaskBitmap);
            UpdateMaskToolState();
        }

        private void RasterizeCurrentCategoryToMask()
        {
            if (currentImage == null) return;
            DefectCategory category = GetSelectedCategory();
            if (category == null) return;

            DialogResult answer = MessageBox.Show(this,
                "将当前类别的 Rectangle / Polygon / Brush 标注栅格化为像素 Mask。\r\n如果已经存在 Mask，将用栅格化结果覆盖当前 Mask。是否继续？",
                "由标注生成 Mask",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;

            byte[] before = CaptureMaskSnapshot();
            try
            {
                currentMaskRecord = AppServices.Masks.RasterizeAnnotations(currentImage.Id, category.Id);
                DisposeCurrentMaskBitmap();
                currentMaskBitmap = currentMaskRecord == null
                    ? new Bitmap(currentImage.Width, currentImage.Height, PixelFormat.Format32bppArgb)
                    : AppServices.Masks.LoadEditableBitmap(currentMaskRecord);
                PushMaskHistory(maskUndoHistory, before);
                maskRedoHistory.Clear();
                UpdateMaskToolState();
                pnlCanvas.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "生成 Mask 失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearCurrentMask()
        {
            if (currentImage == null || currentMaskBitmap == null) return;
            DefectCategory category = GetSelectedCategory();
            if (category == null) return;

            DialogResult answer = MessageBox.Show(this,
                "确定清空当前图片、当前类别的全部 Mask 像素吗？\r\n已发布的数据集版本中的历史 Mask 不会被修改。",
                "清空 Mask",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (answer != DialogResult.Yes) return;

            byte[] before = CaptureMaskSnapshot();
            try
            {
                using (Graphics graphics = Graphics.FromImage(currentMaskBitmap))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.Clear(Color.Transparent);
                }
                currentMaskRecord = AppServices.Masks.SaveMask(currentImage.Id, category.Id, currentMaskBitmap);
                PushMaskHistory(maskUndoHistory, before);
                maskRedoHistory.Clear();
                UpdateMaskToolState();
                pnlCanvas.Invalidate();
            }
            catch (Exception ex)
            {
                RestoreMaskSnapshot(before);
                MessageBox.Show(this, ex.Message, "清空 Mask 失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UndoMaskEdit()
        {
            if (maskUndoHistory.Count == 0 || currentMaskBitmap == null) return;
            byte[] current = CaptureMaskSnapshot();
            byte[] previous = PopMaskHistory(maskUndoHistory);
            try
            {
                RestoreMaskSnapshot(previous);
                PersistCurrentMask();
                PushMaskHistory(maskRedoHistory, current);
                UpdateMaskToolState();
                pnlCanvas.Invalidate();
            }
            catch (Exception ex)
            {
                RestoreMaskSnapshot(current);
                PushMaskHistory(maskUndoHistory, previous);
                MessageBox.Show(this, ex.Message, "撤销 Mask 失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RedoMaskEdit()
        {
            if (maskRedoHistory.Count == 0 || currentMaskBitmap == null) return;
            byte[] current = CaptureMaskSnapshot();
            byte[] next = PopMaskHistory(maskRedoHistory);
            try
            {
                RestoreMaskSnapshot(next);
                PersistCurrentMask();
                PushMaskHistory(maskUndoHistory, current);
                UpdateMaskToolState();
                pnlCanvas.Invalidate();
            }
            catch (Exception ex)
            {
                RestoreMaskSnapshot(current);
                PushMaskHistory(maskRedoHistory, next);
                MessageBox.Show(this, ex.Message, "重做 Mask 失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private byte[] CaptureMaskSnapshot()
        {
            if (currentMaskBitmap == null) return null;
            using (MemoryStream stream = new MemoryStream())
            {
                currentMaskBitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private void RestoreMaskSnapshot(byte[] snapshot)
        {
            DisposeCurrentMaskBitmap();
            if (snapshot == null || snapshot.Length == 0)
            {
                if (currentImage != null)
                    currentMaskBitmap = new Bitmap(currentImage.Width, currentImage.Height, PixelFormat.Format32bppArgb);
                return;
            }

            using (MemoryStream stream = new MemoryStream(snapshot, false))
            using (Bitmap temporary = new Bitmap(stream))
                currentMaskBitmap = new Bitmap(temporary);
        }

        private static void PushMaskHistory(IList<byte[]> history, byte[] snapshot)
        {
            if (snapshot == null) return;
            history.Add(snapshot);
            while (history.Count > MaxMaskHistory) history.RemoveAt(0);
        }

        private static byte[] PopMaskHistory(IList<byte[]> history)
        {
            int index = history.Count - 1;
            byte[] value = history[index];
            history.RemoveAt(index);
            return value;
        }

        private void UpdateMaskToolState()
        {
            bool hasContext = currentImage != null && GetSelectedCategory() != null && currentMaskBitmap != null;
            if (btnMaskAdd != null) btnMaskAdd.Enabled = hasContext;
            if (btnMaskErase != null) btnMaskErase.Enabled = hasContext;
            if (btnMaskRasterize != null) btnMaskRasterize.Enabled = currentImage != null && GetSelectedCategory() != null;
            if (btnMaskUndo != null) btnMaskUndo.Enabled = hasContext && maskUndoHistory.Count > 0;
            if (btnMaskRedo != null) btnMaskRedo.Enabled = hasContext && maskRedoHistory.Count > 0;
            if (btnMaskClear != null) btnMaskClear.Enabled = hasContext && currentMaskRecord != null;

            if (lblMaskState != null)
            {
                DefectCategory category = GetSelectedCategory();
                string categoryText = category == null ? "未选类别" : category.CategoryName;
                if (currentMaskRecord == null)
                    lblMaskState.Text = categoryText + " | Mask：空";
                else
                    lblMaskState.Text = categoryText + " | r" + currentMaskRecord.Revision +
                                        " | 前景 " + currentMaskRecord.PixelCount + " px";
            }
        }

        private void ResetMaskContext()
        {
            DisposeCurrentMaskBitmap();
            currentMaskRecord = null;
            maskContextImageId = -1;
            maskContextCategoryId = -1;
            maskUndoHistory.Clear();
            maskRedoHistory.Clear();
        }

        private void DisposeCurrentMaskBitmap()
        {
            if (currentMaskBitmap != null)
            {
                currentMaskBitmap.Dispose();
                currentMaskBitmap = null;
            }
        }

        private void DisposeMaskEditor()
        {
            DisposeCurrentMaskBitmap();
            maskUndoHistory.Clear();
            maskRedoHistory.Clear();
        }
    }
}
