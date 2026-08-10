using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using IAD.Models;
using IAD.Services;

namespace IAD.Pages
{
    public partial class DatasetAnnotationPage
    {
        private const float MinViewZoom = 0.25F;
        private const float MaxViewZoom = 12F;
        private const int MaxHistoryCount = 100;
        private const float SelectionHandleRadius = 5F;

        private bool drawingAnnotation;
        private readonly List<PointF> workingPoints = new List<PointF>();
        private PointF hoverImagePoint;
        private bool hasHoverPoint;

        private long editingImageId = -1;
        private long selectedAnnotationId;
        private float viewZoom = 1F;
        private PointF viewPan = PointF.Empty;
        private bool panningCanvas;
        private bool spacePanHeld;
        private Point panStartCanvas;
        private PointF panStartOffset;

        private AnnotationEditMode annotationEditMode = AnnotationEditMode.None;
        private AnnotationIdentity editingIdentity;
        private DatasetAnnotation editBeforeSnapshot;
        private readonly List<PointF> editOriginalPoints = new List<PointF>();
        private readonly List<PointF> editWorkingPoints = new List<PointF>();
        private PointF editDragStartImagePoint;
        private int editHandleIndex = -1;
        private bool editChanged;

        private readonly Dictionary<long, AnnotationIdentity> annotationIdentities = new Dictionary<long, AnnotationIdentity>();
        private readonly List<AnnotationHistoryItem> undoHistory = new List<AnnotationHistoryItem>();
        private readonly List<AnnotationHistoryItem> redoHistory = new List<AnnotationHistoryItem>();
        private string lastEnhancedCaption;

        private void BindCanvasEvents()
        {
            btnMaskEdit.Text = "编辑标注";
            btnMaskEdit.Click += delegate
            {
                SetActiveTool("Select");
                UpdateEditingButtonStyle();
                UpdateCanvasCaptionEnhanced();
                pnlCanvas.Focus();
            };
            btnRectangle.Click += delegate { UpdateEditingButtonStyle(); UpdateCanvasCaptionEnhanced(); };
            btnPolygon.Click += delegate { UpdateEditingButtonStyle(); UpdateCanvasCaptionEnhanced(); };
            btnBrush.Click += delegate { UpdateEditingButtonStyle(); UpdateCanvasCaptionEnhanced(); };
            btnEraser.Click += delegate { UpdateEditingButtonStyle(); UpdateCanvasCaptionEnhanced(); };

            pnlCanvas.Paint += pnlCanvas_Paint;
            pnlCanvas.MouseDown += pnlCanvas_MouseDown;
            pnlCanvas.MouseMove += pnlCanvas_MouseMove;
            pnlCanvas.MouseUp += pnlCanvas_MouseUp;
            pnlCanvas.MouseClick += pnlCanvas_MouseClick;
            pnlCanvas.MouseDoubleClick += pnlCanvas_MouseDoubleClick;
            pnlCanvas.MouseWheel += pnlCanvas_MouseWheel;
            pnlCanvas.MouseEnter += delegate { if (currentBitmap != null) pnlCanvas.Focus(); };
            pnlCanvas.MouseLeave += delegate
            {
                hasHoverPoint = false;
                if (!panningCanvas && !spacePanHeld && string.Equals(activeTool, "Select", StringComparison.Ordinal))
                    pnlCanvas.Cursor = Cursors.Default;
                pnlCanvas.Invalidate();
            };
            pnlCanvas.KeyDown += pnlCanvas_KeyDown;
            pnlCanvas.KeyUp += pnlCanvas_KeyUp;
            pnlCanvas.LostFocus += delegate
            {
                spacePanHeld = false;
                if (!panningCanvas) UpdateCanvasCursor();
            };
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (runtimeInitialized && currentImage != null)
            {
                if (keyData == (Keys.Control | Keys.Z))
                {
                    UndoLastAnnotationAction();
                    return true;
                }
                if (keyData == (Keys.Control | Keys.Y))
                {
                    RedoLastAnnotationAction();
                    return true;
                }
                if (keyData == Keys.Delete && selectedAnnotationId > 0 && string.Equals(activeTool, "Select", StringComparison.Ordinal))
                {
                    DeleteSelectedAnnotationWithHistory();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void pnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            EnsureEditingContext();
            e.Graphics.Clear(pnlCanvas.BackColor);
            if (currentBitmap == null) return;

            RectangleF imageBounds = GetImageDisplayBounds();
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.DrawImage(currentBitmap, imageBounds);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (DatasetAnnotation annotation in currentAnnotations)
            {
                if (!annotation.IsVisible) continue;
                DrawAnnotation(e.Graphics, annotation, imageBounds);
            }

            DrawWorkingAnnotation(e.Graphics, imageBounds);
            DrawSelectedAnnotation(e.Graphics, imageBounds);

            using (Pen border = new Pen(Color.FromArgb(145, 255, 255, 255), 1F))
                e.Graphics.DrawRectangle(border, imageBounds.X, imageBounds.Y, imageBounds.Width, imageBounds.Height);

            UpdateCanvasCaptionEnhanced();
        }

        private void DrawAnnotation(Graphics graphics, DatasetAnnotation annotation, RectangleF imageBounds)
        {
            List<PointF> imagePoints = GetDisplayPoints(annotation);
            if (imagePoints == null || imagePoints.Count == 0) return;

            PointF[] points = ToCanvasPoints(imagePoints, imageBounds);
            Color color = GetAnnotationColor(annotation);
            using (Pen pen = new Pen(color, selectedAnnotationId == annotation.Id ? 2.6F : 2F))
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(selectedAnnotationId == annotation.Id ? 60 : 45, color)))
            {
                pen.LineJoin = LineJoin.Round;
                if (string.Equals(annotation.AnnotationType, "Rectangle", StringComparison.OrdinalIgnoreCase) && points.Length == 2)
                {
                    RectangleF rectangle = MakeRectangle(points[0], points[1]);
                    graphics.FillRectangle(fill, rectangle);
                    graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
                }
                else if (string.Equals(annotation.AnnotationType, "Polygon", StringComparison.OrdinalIgnoreCase) && points.Length >= 3)
                {
                    graphics.FillPolygon(fill, points);
                    graphics.DrawPolygon(pen, points);
                }
                else if (string.Equals(annotation.AnnotationType, "Brush", StringComparison.OrdinalIgnoreCase) && points.Length >= 2)
                {
                    float scale = imageBounds.Width / Math.Max(1F, currentImage.Width);
                    pen.Width = Math.Max(2F, Math.Min(60F, annotation.BrushWidth * scale));
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    graphics.DrawLines(pen, points);
                }
            }
        }

        private void DrawWorkingAnnotation(Graphics graphics, RectangleF imageBounds)
        {
            if (!drawingAnnotation || workingPoints.Count == 0) return;
            List<PointF> preview = new List<PointF>(workingPoints);
            if (string.Equals(activeTool, "Polygon", StringComparison.Ordinal) && hasHoverPoint)
                preview.Add(hoverImagePoint);
            PointF[] points = ToCanvasPoints(preview, imageBounds);

            using (Pen pen = new Pen(Color.FromArgb(255, 225, 80), 2F))
            {
                pen.DashStyle = string.Equals(activeTool, "Brush", StringComparison.Ordinal) ? DashStyle.Solid : DashStyle.Dash;
                pen.LineJoin = LineJoin.Round;
                if (string.Equals(activeTool, "Rectangle", StringComparison.Ordinal) && points.Length == 2)
                {
                    RectangleF rectangle = MakeRectangle(points[0], points[1]);
                    graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
                }
                else if (points.Length >= 2)
                {
                    if (string.Equals(activeTool, "Brush", StringComparison.Ordinal))
                    {
                        float scale = imageBounds.Width / Math.Max(1F, currentImage.Width);
                        pen.Width = Math.Max(2F, (float)numLineWidth.Value * scale);
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                    }
                    graphics.DrawLines(pen, points);
                }

                for (int i = 0; i < points.Length && string.Equals(activeTool, "Polygon", StringComparison.Ordinal); i++)
                    graphics.FillEllipse(Brushes.Gold, points[i].X - 3F, points[i].Y - 3F, 6F, 6F);
            }
        }

        private void DrawSelectedAnnotation(Graphics graphics, RectangleF imageBounds)
        {
            DatasetAnnotation annotation = GetSelectedAnnotation();
            if (annotation == null || !annotation.IsVisible) return;
            List<PointF> imagePoints = GetDisplayPoints(annotation);
            if (imagePoints == null || imagePoints.Count == 0) return;
            PointF[] canvasPoints = ToCanvasPoints(imagePoints, imageBounds);

            using (Pen selectionPen = new Pen(Color.White, 1.4F))
            using (SolidBrush handleFill = new SolidBrush(Color.White))
            using (Pen handleBorder = new Pen(Color.FromArgb(40, 40, 40), 1F))
            {
                selectionPen.DashStyle = DashStyle.Dash;
                if (string.Equals(annotation.AnnotationType, "Rectangle", StringComparison.OrdinalIgnoreCase) && canvasPoints.Length == 2)
                {
                    RectangleF rectangle = MakeRectangle(canvasPoints[0], canvasPoints[1]);
                    graphics.DrawRectangle(selectionPen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
                    PointF[] handles = GetRectangleHandlePoints(rectangle);
                    for (int i = 0; i < handles.Length; i++)
                        DrawHandle(graphics, handles[i], handleFill, handleBorder);
                }
                else if (string.Equals(annotation.AnnotationType, "Polygon", StringComparison.OrdinalIgnoreCase) && canvasPoints.Length >= 3)
                {
                    graphics.DrawPolygon(selectionPen, canvasPoints);
                    for (int i = 0; i < canvasPoints.Length; i++)
                        DrawHandle(graphics, canvasPoints[i], handleFill, handleBorder);
                }
                else if (string.Equals(annotation.AnnotationType, "Brush", StringComparison.OrdinalIgnoreCase) && canvasPoints.Length >= 2)
                {
                    RectangleF bounds = GetPointBounds(canvasPoints);
                    bounds.Inflate(4F, 4F);
                    graphics.DrawRectangle(selectionPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                }
            }
        }

        private static void DrawHandle(Graphics graphics, PointF point, Brush fill, Pen border)
        {
            RectangleF handle = new RectangleF(
                point.X - SelectionHandleRadius,
                point.Y - SelectionHandleRadius,
                SelectionHandleRadius * 2F,
                SelectionHandleRadius * 2F);
            graphics.FillRectangle(fill, handle);
            graphics.DrawRectangle(border, handle.X, handle.Y, handle.Width, handle.Height);
        }

        private void pnlCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            EnsureEditingContext();
            pnlCanvas.Focus();

            if (ShouldStartPan(e))
            {
                BeginCanvasPan(e.Location);
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                CancelWorkingAnnotation();
                CancelAnnotationEdit();
                return;
            }
            if (e.Button != MouseButtons.Left || currentBitmap == null) return;

            PointF imagePoint;
            if (!TryCanvasToImage(e.Location, out imagePoint))
            {
                if (string.Equals(activeTool, "Select", StringComparison.Ordinal))
                {
                    selectedAnnotationId = 0;
                    pnlCanvas.Invalidate();
                }
                return;
            }

            if (string.Equals(activeTool, "Select", StringComparison.Ordinal))
            {
                BeginSelectInteraction(e.Location, imagePoint);
                return;
            }
            if (string.Equals(activeTool, "Eraser", StringComparison.Ordinal))
            {
                EraseAt(imagePoint);
                return;
            }
            if (string.Equals(activeTool, "Rectangle", StringComparison.Ordinal))
            {
                drawingAnnotation = true;
                workingPoints.Clear();
                workingPoints.Add(imagePoint);
                workingPoints.Add(imagePoint);
            }
            else if (string.Equals(activeTool, "Brush", StringComparison.Ordinal))
            {
                drawingAnnotation = true;
                workingPoints.Clear();
                workingPoints.Add(imagePoint);
            }
        }

        private void pnlCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (panningCanvas)
            {
                viewPan = new PointF(
                    panStartOffset.X + e.X - panStartCanvas.X,
                    panStartOffset.Y + e.Y - panStartCanvas.Y);
                ClampViewPan();
                pnlCanvas.Invalidate();
                UpdateCanvasCaptionEnhanced();
                return;
            }

            PointF imagePoint;
            bool inside = TryCanvasToImage(e.Location, out imagePoint);

            if (annotationEditMode != AnnotationEditMode.None)
            {
                if (inside)
                {
                    UpdateAnnotationEdit(imagePoint);
                    pnlCanvas.Invalidate();
                }
                return;
            }

            if (string.Equals(activeTool, "Select", StringComparison.Ordinal))
            {
                UpdateSelectCursor(e.Location, inside ? (PointF?)imagePoint : null);
                return;
            }

            if (string.Equals(activeTool, "Polygon", StringComparison.Ordinal))
            {
                hasHoverPoint = inside;
                if (inside) hoverImagePoint = imagePoint;
            }
            if (!drawingAnnotation || !inside)
            {
                if (string.Equals(activeTool, "Polygon", StringComparison.Ordinal)) pnlCanvas.Invalidate();
                return;
            }

            if (string.Equals(activeTool, "Rectangle", StringComparison.Ordinal) && workingPoints.Count == 2)
                workingPoints[1] = imagePoint;
            else if (string.Equals(activeTool, "Brush", StringComparison.Ordinal))
            {
                PointF last = workingPoints[workingPoints.Count - 1];
                if (Distance(last, imagePoint) >= Math.Max(1F, (float)numLineWidth.Value / 2F))
                    workingPoints.Add(imagePoint);
            }
            pnlCanvas.Invalidate();
        }

        private void pnlCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (panningCanvas && (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Left))
            {
                EndCanvasPan();
                return;
            }

            if (e.Button == MouseButtons.Left && annotationEditMode != AnnotationEditMode.None)
            {
                CommitAnnotationEdit();
                return;
            }

            if (e.Button != MouseButtons.Left || !drawingAnnotation) return;
            if (string.Equals(activeTool, "Rectangle", StringComparison.Ordinal))
            {
                if (workingPoints.Count == 2 && Distance(workingPoints[0], workingPoints[1]) >= 2F)
                    SaveWorkingAnnotation("Rectangle");
                else
                    CancelWorkingAnnotation();
            }
            else if (string.Equals(activeTool, "Brush", StringComparison.Ordinal))
            {
                if (workingPoints.Count >= 2)
                    SaveWorkingAnnotation("Brush");
                else
                    CancelWorkingAnnotation();
            }
        }

        private void pnlCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (!string.Equals(activeTool, "Polygon", StringComparison.Ordinal) || e.Button != MouseButtons.Left || e.Clicks > 1)
                return;
            PointF imagePoint;
            if (!TryCanvasToImage(e.Location, out imagePoint)) return;
            if (!drawingAnnotation)
            {
                drawingAnnotation = true;
                workingPoints.Clear();
            }
            workingPoints.Add(imagePoint);
            pnlCanvas.Invalidate();
        }

        private void pnlCanvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (string.Equals(activeTool, "Polygon", StringComparison.Ordinal))
            {
                if (workingPoints.Count >= 3)
                    SaveWorkingAnnotation("Polygon");
                else
                    CancelWorkingAnnotation();
                return;
            }

            if (string.Equals(activeTool, "Select", StringComparison.Ordinal))
                ResetCanvasView();
        }

        private void pnlCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            EnsureEditingContext();
            if (currentBitmap == null) return;

            RectangleF oldBounds = GetImageDisplayBounds();
            if (oldBounds.IsEmpty) return;
            float oldZoom = viewZoom;
            float factor = e.Delta > 0 ? 1.2F : 1F / 1.2F;
            float newZoom = Math.Max(MinViewZoom, Math.Min(MaxViewZoom, oldZoom * factor));
            if (Math.Abs(newZoom - oldZoom) < 0.0001F) return;

            float ratioX = oldBounds.Contains(e.Location) ? (e.X - oldBounds.X) / oldBounds.Width : 0.5F;
            float ratioY = oldBounds.Contains(e.Location) ? (e.Y - oldBounds.Y) / oldBounds.Height : 0.5F;
            ratioX = Math.Max(0F, Math.Min(1F, ratioX));
            ratioY = Math.Max(0F, Math.Min(1F, ratioY));

            RectangleF fit = GetFitImageBounds();
            float newWidth = fit.Width * newZoom;
            float newHeight = fit.Height * newZoom;
            float baseX = (pnlCanvas.ClientSize.Width - newWidth) / 2F;
            float baseY = (pnlCanvas.ClientSize.Height - newHeight) / 2F;
            viewZoom = newZoom;
            viewPan = new PointF(
                e.X - ratioX * newWidth - baseX,
                e.Y - ratioY * newHeight - baseY);
            ClampViewPan();
            pnlCanvas.Invalidate();
            UpdateCanvasCaptionEnhanced();
        }

        private void pnlCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                UndoLastAnnotationAction();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.Y)
            {
                RedoLastAnnotationAction();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.KeyCode == Keys.Space)
            {
                spacePanHeld = true;
                pnlCanvas.Cursor = Cursors.Hand;
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.KeyCode == Keys.Home || e.KeyCode == Keys.D0)
            {
                ResetCanvasView();
                e.Handled = true;
                return;
            }
            if (e.KeyCode == Keys.Delete && selectedAnnotationId > 0 && string.Equals(activeTool, "Select", StringComparison.Ordinal))
            {
                DeleteSelectedAnnotationWithHistory();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                if (annotationEditMode != AnnotationEditMode.None)
                    CancelAnnotationEdit();
                else if (drawingAnnotation)
                    CancelWorkingAnnotation();
                else
                    selectedAnnotationId = 0;
                pnlCanvas.Invalidate();
                e.Handled = true;
                return;
            }
            if (e.KeyCode == Keys.Enter && string.Equals(activeTool, "Polygon", StringComparison.Ordinal) && workingPoints.Count >= 3)
            {
                SaveWorkingAnnotation("Polygon");
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Back && string.Equals(activeTool, "Polygon", StringComparison.Ordinal) && workingPoints.Count > 0)
            {
                workingPoints.RemoveAt(workingPoints.Count - 1);
                if (workingPoints.Count == 0) drawingAnnotation = false;
                pnlCanvas.Invalidate();
                e.Handled = true;
            }
        }

        private void pnlCanvas_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Space) return;
            spacePanHeld = false;
            if (!panningCanvas) UpdateCanvasCursor();
            e.Handled = true;
        }

        private void SaveWorkingAnnotation(string annotationType)
        {
            DefectCategory category = GetSelectedCategory();
            if (currentImage == null || category == null)
            {
                CancelWorkingAnnotation();
                return;
            }

            try
            {
                DatasetAnnotation created = AppServices.Datasets.CreateAnnotation(
                    currentImage.Id,
                    category.Id,
                    annotationType,
                    AnnotationGeometry.Serialize(workingPoints),
                    (float)numLineWidth.Value,
                    (double)numThreshold.Value);
                AnnotationIdentity identity = GetIdentity(created.Id);
                RegisterHistory(new AnnotationHistoryItem(
                    AnnotationHistoryKind.Create,
                    identity,
                    null,
                    CloneAnnotation(created)));
                CancelWorkingAnnotation();
                RefreshCurrentAnnotations();
            }
            catch (Exception ex)
            {
                CancelWorkingAnnotation();
                MessageBox.Show(this, ex.Message, "保存标注失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void EraseAt(PointF imagePoint)
        {
            DatasetAnnotation annotation = FindTopAnnotation(imagePoint);
            if (annotation == null) return;
            DeleteAnnotationWithHistory(annotation);
        }

        private void DeleteSelectedAnnotationWithHistory()
        {
            DatasetAnnotation annotation = GetSelectedAnnotation();
            if (annotation == null) return;
            DeleteAnnotationWithHistory(annotation);
        }

        private void DeleteAnnotationWithHistory(DatasetAnnotation annotation)
        {
            try
            {
                DatasetAnnotation snapshot = CloneAnnotation(annotation);
                AnnotationIdentity identity = GetIdentity(annotation.Id);
                AppServices.Datasets.DeleteAnnotation(currentImage.Id, annotation.Id);
                RebindIdentity(identity, 0);
                RegisterHistory(new AnnotationHistoryItem(
                    AnnotationHistoryKind.Delete,
                    identity,
                    snapshot,
                    null));
                selectedAnnotationId = 0;
                CancelAnnotationEdit();
                RefreshCurrentAnnotations();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "删除标注失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BeginSelectInteraction(Point canvasPoint, PointF imagePoint)
        {
            DatasetAnnotation selected = GetSelectedAnnotation();
            int handleIndex;
            AnnotationEditMode handleMode;
            if (selected != null && TryHitSelectionHandle(selected, canvasPoint, out handleIndex, out handleMode))
            {
                BeginAnnotationEdit(selected, handleMode, handleIndex, imagePoint);
                return;
            }

            DatasetAnnotation hit = FindTopAnnotation(imagePoint);
            if (hit == null)
            {
                selectedAnnotationId = 0;
                pnlCanvas.Cursor = Cursors.Default;
                pnlCanvas.Invalidate();
                UpdateCanvasCaptionEnhanced();
                return;
            }

            selectedAnnotationId = hit.Id;
            BeginAnnotationEdit(hit, AnnotationEditMode.Move, -1, imagePoint);
            pnlCanvas.Invalidate();
            UpdateCanvasCaptionEnhanced();
        }

        private void BeginAnnotationEdit(DatasetAnnotation annotation, AnnotationEditMode mode, int handleIndex, PointF imagePoint)
        {
            List<PointF> points;
            try { points = AnnotationGeometry.Parse(annotation.GeometryData); }
            catch { return; }

            annotationEditMode = mode;
            editingIdentity = GetIdentity(annotation.Id);
            editBeforeSnapshot = CloneAnnotation(annotation);
            editOriginalPoints.Clear();
            editOriginalPoints.AddRange(points);
            editWorkingPoints.Clear();
            editWorkingPoints.AddRange(points);
            editDragStartImagePoint = imagePoint;
            editHandleIndex = handleIndex;
            editChanged = false;
            pnlCanvas.Capture = true;
        }

        private void UpdateAnnotationEdit(PointF currentPoint)
        {
            if (annotationEditMode == AnnotationEditMode.None || editBeforeSnapshot == null || currentImage == null) return;

            if (annotationEditMode == AnnotationEditMode.Move)
                UpdateMoveEdit(currentPoint);
            else if (annotationEditMode == AnnotationEditMode.RectangleResize)
                UpdateRectangleResize(currentPoint);
            else if (annotationEditMode == AnnotationEditMode.PolygonVertex)
                UpdatePolygonVertex(currentPoint);

            editChanged = !string.Equals(
                AnnotationGeometry.Serialize(editWorkingPoints),
                AnnotationGeometry.Serialize(editOriginalPoints),
                StringComparison.Ordinal);
        }

        private void UpdateMoveEdit(PointF currentPoint)
        {
            float dx = currentPoint.X - editDragStartImagePoint.X;
            float dy = currentPoint.Y - editDragStartImagePoint.Y;
            RectangleF originalBounds = GetPointBounds(editOriginalPoints);
            dx = Math.Max(-originalBounds.Left, Math.Min(currentImage.Width - originalBounds.Right, dx));
            dy = Math.Max(-originalBounds.Top, Math.Min(currentImage.Height - originalBounds.Bottom, dy));

            editWorkingPoints.Clear();
            foreach (PointF point in editOriginalPoints)
                editWorkingPoints.Add(new PointF(point.X + dx, point.Y + dy));
        }

        private void UpdateRectangleResize(PointF currentPoint)
        {
            if (editOriginalPoints.Count != 2 || editHandleIndex < 0 || editHandleIndex > 7) return;
            RectangleF rectangle = MakeRectangle(editOriginalPoints[0], editOriginalPoints[1]);
            float left = rectangle.Left;
            float top = rectangle.Top;
            float right = rectangle.Right;
            float bottom = rectangle.Bottom;
            float x = Math.Max(0F, Math.Min(currentImage.Width, currentPoint.X));
            float y = Math.Max(0F, Math.Min(currentImage.Height, currentPoint.Y));

            switch (editHandleIndex)
            {
                case 0: left = x; top = y; break;
                case 1: top = y; break;
                case 2: right = x; top = y; break;
                case 3: right = x; break;
                case 4: right = x; bottom = y; break;
                case 5: bottom = y; break;
                case 6: left = x; bottom = y; break;
                case 7: left = x; break;
            }

            if (left > right - 1F) left = right - 1F;
            if (right < left + 1F) right = left + 1F;
            if (top > bottom - 1F) top = bottom - 1F;
            if (bottom < top + 1F) bottom = top + 1F;
            left = Math.Max(0F, left);
            top = Math.Max(0F, top);
            right = Math.Min(currentImage.Width, right);
            bottom = Math.Min(currentImage.Height, bottom);

            editWorkingPoints.Clear();
            editWorkingPoints.Add(new PointF(left, top));
            editWorkingPoints.Add(new PointF(right, bottom));
        }

        private void UpdatePolygonVertex(PointF currentPoint)
        {
            if (editHandleIndex < 0 || editHandleIndex >= editOriginalPoints.Count) return;
            editWorkingPoints.Clear();
            editWorkingPoints.AddRange(editOriginalPoints);
            editWorkingPoints[editHandleIndex] = new PointF(
                Math.Max(0F, Math.Min(currentImage.Width, currentPoint.X)),
                Math.Max(0F, Math.Min(currentImage.Height, currentPoint.Y)));
        }

        private void CommitAnnotationEdit()
        {
            if (annotationEditMode == AnnotationEditMode.None) return;
            pnlCanvas.Capture = false;
            if (!editChanged || editBeforeSnapshot == null || editingIdentity == null || editingIdentity.CurrentId <= 0)
            {
                ClearAnnotationEditState();
                pnlCanvas.Invalidate();
                return;
            }

            try
            {
                DatasetAnnotation before = CloneAnnotation(editBeforeSnapshot);
                before.Id = editingIdentity.CurrentId;
                DatasetAnnotation after = CloneAnnotation(editBeforeSnapshot);
                after.Id = editingIdentity.CurrentId;
                after.GeometryData = AnnotationGeometry.Serialize(editWorkingPoints);
                after = AppServices.AnnotationEditing.Update(after);
                selectedAnnotationId = after.Id;
                RegisterHistory(new AnnotationHistoryItem(
                    AnnotationHistoryKind.Update,
                    editingIdentity,
                    before,
                    CloneAnnotation(after)));
                ClearAnnotationEditState();
                RefreshCurrentAnnotations();
            }
            catch (Exception ex)
            {
                ClearAnnotationEditState();
                RefreshCurrentAnnotations();
                MessageBox.Show(this, ex.Message, "修改标注失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CancelAnnotationEdit()
        {
            pnlCanvas.Capture = false;
            ClearAnnotationEditState();
            pnlCanvas.Invalidate();
        }

        private void ClearAnnotationEditState()
        {
            annotationEditMode = AnnotationEditMode.None;
            editingIdentity = null;
            editBeforeSnapshot = null;
            editOriginalPoints.Clear();
            editWorkingPoints.Clear();
            editHandleIndex = -1;
            editChanged = false;
        }

        private void UndoLastAnnotationAction()
        {
            EnsureEditingContext();
            if (undoHistory.Count == 0 || currentImage == null) return;
            AnnotationHistoryItem item = undoHistory[undoHistory.Count - 1];
            try
            {
                ApplyHistoryItem(item, true);
                undoHistory.RemoveAt(undoHistory.Count - 1);
                redoHistory.Add(item);
                RefreshAfterHistory(item.Identity);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "撤销失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RedoLastAnnotationAction()
        {
            EnsureEditingContext();
            if (redoHistory.Count == 0 || currentImage == null) return;
            AnnotationHistoryItem item = redoHistory[redoHistory.Count - 1];
            try
            {
                ApplyHistoryItem(item, false);
                redoHistory.RemoveAt(redoHistory.Count - 1);
                undoHistory.Add(item);
                RefreshAfterHistory(item.Identity);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "重做失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplyHistoryItem(AnnotationHistoryItem item, bool undo)
        {
            CancelWorkingAnnotation();
            CancelAnnotationEdit();

            if (item.Kind == AnnotationHistoryKind.Create)
            {
                if (undo)
                    DeleteIdentityAnnotation(item.Identity);
                else
                    RecreateIdentityAnnotation(item.Identity, item.After);
                return;
            }

            if (item.Kind == AnnotationHistoryKind.Delete)
            {
                if (undo)
                    RecreateIdentityAnnotation(item.Identity, item.Before);
                else
                    DeleteIdentityAnnotation(item.Identity);
                return;
            }

            DatasetAnnotation snapshot = CloneAnnotation(undo ? item.Before : item.After);
            if (item.Identity.CurrentId <= 0)
                throw new InvalidOperationException("无法恢复标注修改：当前标注不存在。");
            snapshot.Id = item.Identity.CurrentId;
            AppServices.AnnotationEditing.Update(snapshot);
        }

        private void DeleteIdentityAnnotation(AnnotationIdentity identity)
        {
            if (identity == null || identity.CurrentId <= 0) return;
            AppServices.Datasets.DeleteAnnotation(currentImage.Id, identity.CurrentId);
            RebindIdentity(identity, 0);
        }

        private void RecreateIdentityAnnotation(AnnotationIdentity identity, DatasetAnnotation snapshot)
        {
            if (identity == null || snapshot == null) return;
            DatasetAnnotation recreated = AppServices.AnnotationEditing.Recreate(snapshot);
            RebindIdentity(identity, recreated.Id);
        }

        private void RefreshAfterHistory(AnnotationIdentity identity)
        {
            selectedAnnotationId = identity != null ? identity.CurrentId : 0;
            RefreshCurrentAnnotations();
            UpdateCanvasCaptionEnhanced();
        }

        private void RegisterHistory(AnnotationHistoryItem item)
        {
            if (item == null) return;
            undoHistory.Add(item);
            if (undoHistory.Count > MaxHistoryCount) undoHistory.RemoveAt(0);
            redoHistory.Clear();
            UpdateCanvasCaptionEnhanced();
        }

        private AnnotationIdentity GetIdentity(long annotationId)
        {
            AnnotationIdentity identity;
            if (annotationId > 0 && annotationIdentities.TryGetValue(annotationId, out identity)) return identity;
            identity = new AnnotationIdentity { CurrentId = annotationId };
            if (annotationId > 0) annotationIdentities[annotationId] = identity;
            return identity;
        }

        private void RebindIdentity(AnnotationIdentity identity, long newId)
        {
            if (identity == null) return;
            if (identity.CurrentId > 0) annotationIdentities.Remove(identity.CurrentId);
            identity.CurrentId = newId;
            if (newId > 0) annotationIdentities[newId] = identity;
        }

        private bool TryHitSelectionHandle(DatasetAnnotation annotation, Point canvasPoint, out int handleIndex, out AnnotationEditMode mode)
        {
            handleIndex = -1;
            mode = AnnotationEditMode.None;
            if (annotation == null || currentImage == null) return false;

            List<PointF> imagePoints = GetDisplayPoints(annotation);
            if (imagePoints == null) return false;
            RectangleF imageBounds = GetImageDisplayBounds();
            PointF canvas = canvasPoint;

            if (string.Equals(annotation.AnnotationType, "Rectangle", StringComparison.OrdinalIgnoreCase) && imagePoints.Count == 2)
            {
                PointF[] points = ToCanvasPoints(imagePoints, imageBounds);
                PointF[] handles = GetRectangleHandlePoints(MakeRectangle(points[0], points[1]));
                for (int i = 0; i < handles.Length; i++)
                {
                    if (Distance(canvas, handles[i]) <= SelectionHandleRadius + 3F)
                    {
                        handleIndex = i;
                        mode = AnnotationEditMode.RectangleResize;
                        return true;
                    }
                }
            }
            else if (string.Equals(annotation.AnnotationType, "Polygon", StringComparison.OrdinalIgnoreCase) && imagePoints.Count >= 3)
            {
                PointF[] points = ToCanvasPoints(imagePoints, imageBounds);
                for (int i = 0; i < points.Length; i++)
                {
                    if (Distance(canvas, points[i]) <= SelectionHandleRadius + 3F)
                    {
                        handleIndex = i;
                        mode = AnnotationEditMode.PolygonVertex;
                        return true;
                    }
                }
            }
            return false;
        }

        private DatasetAnnotation FindTopAnnotation(PointF imagePoint)
        {
            for (int i = currentAnnotations.Count - 1; i >= 0; i--)
            {
                DatasetAnnotation annotation = currentAnnotations[i];
                if (!annotation.IsVisible || !HitTest(annotation, imagePoint)) continue;
                return annotation;
            }
            return null;
        }

        private DatasetAnnotation GetSelectedAnnotation()
        {
            if (selectedAnnotationId <= 0) return null;
            foreach (DatasetAnnotation annotation in currentAnnotations)
            {
                if (annotation.Id == selectedAnnotationId) return annotation;
            }
            return null;
        }

        private List<PointF> GetDisplayPoints(DatasetAnnotation annotation)
        {
            if (annotation == null) return null;
            if (annotationEditMode != AnnotationEditMode.None && editingIdentity != null &&
                editingIdentity.CurrentId == annotation.Id && editWorkingPoints.Count > 0)
                return new List<PointF>(editWorkingPoints);
            try { return AnnotationGeometry.Parse(annotation.GeometryData); }
            catch { return null; }
        }

        private bool HitTest(DatasetAnnotation annotation, PointF point)
        {
            List<PointF> points;
            try { points = AnnotationGeometry.Parse(annotation.GeometryData); }
            catch { return false; }
            float tolerance = GetImageTolerance(6F);

            if (string.Equals(annotation.AnnotationType, "Rectangle", StringComparison.OrdinalIgnoreCase) && points.Count == 2)
            {
                RectangleF rectangle = MakeRectangle(points[0], points[1]);
                rectangle.Inflate(tolerance, tolerance);
                return rectangle.Contains(point);
            }
            if (string.Equals(annotation.AnnotationType, "Polygon", StringComparison.OrdinalIgnoreCase) && points.Count >= 3)
            {
                if (IsPointInPolygon(points, point)) return true;
                for (int i = 0; i < points.Count; i++)
                {
                    PointF next = points[(i + 1) % points.Count];
                    if (DistanceToSegment(point, points[i], next) <= tolerance) return true;
                }
                return false;
            }
            if (string.Equals(annotation.AnnotationType, "Brush", StringComparison.OrdinalIgnoreCase) && points.Count >= 2)
            {
                float brushTolerance = Math.Max(tolerance, annotation.BrushWidth / 2F + tolerance);
                for (int i = 1; i < points.Count; i++)
                {
                    if (DistanceToSegment(point, points[i - 1], points[i]) <= brushTolerance) return true;
                }
            }
            return false;
        }

        private float GetImageTolerance(float canvasPixels)
        {
            RectangleF bounds = GetImageDisplayBounds();
            if (currentImage == null || bounds.Width <= 0) return canvasPixels;
            return canvasPixels * currentImage.Width / bounds.Width;
        }

        private bool ShouldStartPan(MouseEventArgs e)
        {
            return e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && spacePanHeld);
        }

        private void BeginCanvasPan(Point location)
        {
            panningCanvas = true;
            panStartCanvas = location;
            panStartOffset = viewPan;
            pnlCanvas.Capture = true;
            pnlCanvas.Cursor = Cursors.Hand;
        }

        private void EndCanvasPan()
        {
            panningCanvas = false;
            pnlCanvas.Capture = false;
            UpdateCanvasCursor();
        }

        private void ResetCanvasView()
        {
            viewZoom = 1F;
            viewPan = PointF.Empty;
            pnlCanvas.Invalidate();
            UpdateCanvasCaptionEnhanced();
        }

        private void ClampViewPan()
        {
            if (currentBitmap == null || pnlCanvas.ClientSize.Width <= 0 || pnlCanvas.ClientSize.Height <= 0) return;
            RectangleF fit = GetFitImageBounds();
            float width = fit.Width * viewZoom;
            float height = fit.Height * viewZoom;
            float baseX = (pnlCanvas.ClientSize.Width - width) / 2F;
            float baseY = (pnlCanvas.ClientSize.Height - height) / 2F;
            const float visibleMargin = 32F;
            float minPanX = visibleMargin - width - baseX;
            float maxPanX = pnlCanvas.ClientSize.Width - visibleMargin - baseX;
            float minPanY = visibleMargin - height - baseY;
            float maxPanY = pnlCanvas.ClientSize.Height - visibleMargin - baseY;
            viewPan = new PointF(
                Math.Max(minPanX, Math.Min(maxPanX, viewPan.X)),
                Math.Max(minPanY, Math.Min(maxPanY, viewPan.Y)));
        }

        private RectangleF GetFitImageBounds()
        {
            if (currentBitmap == null || pnlCanvas.ClientSize.Width <= 0 || pnlCanvas.ClientSize.Height <= 0)
                return RectangleF.Empty;
            const float padding = 10F;
            float availableWidth = Math.Max(1F, pnlCanvas.ClientSize.Width - padding * 2F);
            float availableHeight = Math.Max(1F, pnlCanvas.ClientSize.Height - padding * 2F);
            float scale = Math.Min(availableWidth / currentBitmap.Width, availableHeight / currentBitmap.Height);
            float width = currentBitmap.Width * scale;
            float height = currentBitmap.Height * scale;
            return new RectangleF(
                (pnlCanvas.ClientSize.Width - width) / 2F,
                (pnlCanvas.ClientSize.Height - height) / 2F,
                width,
                height);
        }

        private RectangleF GetImageDisplayBounds()
        {
            RectangleF fit = GetFitImageBounds();
            if (fit.IsEmpty) return fit;
            float width = fit.Width * viewZoom;
            float height = fit.Height * viewZoom;
            return new RectangleF(
                (pnlCanvas.ClientSize.Width - width) / 2F + viewPan.X,
                (pnlCanvas.ClientSize.Height - height) / 2F + viewPan.Y,
                width,
                height);
        }

        private bool TryCanvasToImage(Point canvasPoint, out PointF imagePoint)
        {
            imagePoint = PointF.Empty;
            RectangleF bounds = GetImageDisplayBounds();
            if (currentImage == null || bounds.IsEmpty || !bounds.Contains(canvasPoint)) return false;
            imagePoint = new PointF(
                Math.Max(0F, Math.Min(currentImage.Width, (canvasPoint.X - bounds.X) * currentImage.Width / bounds.Width)),
                Math.Max(0F, Math.Min(currentImage.Height, (canvasPoint.Y - bounds.Y) * currentImage.Height / bounds.Height)));
            return true;
        }

        private PointF[] ToCanvasPoints(IList<PointF> imagePoints, RectangleF imageBounds)
        {
            PointF[] result = new PointF[imagePoints.Count];
            for (int i = 0; i < imagePoints.Count; i++)
            {
                result[i] = new PointF(
                    imageBounds.X + imagePoints[i].X * imageBounds.Width / Math.Max(1F, currentImage.Width),
                    imageBounds.Y + imagePoints[i].Y * imageBounds.Height / Math.Max(1F, currentImage.Height));
            }
            return result;
        }

        private void UpdateSelectCursor(Point canvasPoint, PointF? imagePoint)
        {
            if (spacePanHeld)
            {
                pnlCanvas.Cursor = Cursors.Hand;
                return;
            }

            DatasetAnnotation selected = GetSelectedAnnotation();
            int handleIndex;
            AnnotationEditMode handleMode;
            if (selected != null && TryHitSelectionHandle(selected, canvasPoint, out handleIndex, out handleMode))
            {
                if (handleMode == AnnotationEditMode.PolygonVertex)
                    pnlCanvas.Cursor = Cursors.Hand;
                else
                    pnlCanvas.Cursor = GetRectangleResizeCursor(handleIndex);
                return;
            }

            if (imagePoint.HasValue && FindTopAnnotation(imagePoint.Value) != null)
                pnlCanvas.Cursor = Cursors.SizeAll;
            else
                pnlCanvas.Cursor = Cursors.Default;
        }

        private static Cursor GetRectangleResizeCursor(int handleIndex)
        {
            switch (handleIndex)
            {
                case 0:
                case 4: return Cursors.SizeNWSE;
                case 2:
                case 6: return Cursors.SizeNESW;
                case 1:
                case 5: return Cursors.SizeNS;
                case 3:
                case 7: return Cursors.SizeWE;
                default: return Cursors.SizeAll;
            }
        }

        private void UpdateCanvasCursor()
        {
            if (spacePanHeld || panningCanvas)
                pnlCanvas.Cursor = Cursors.Hand;
            else if (string.Equals(activeTool, "Select", StringComparison.Ordinal))
                pnlCanvas.Cursor = Cursors.Default;
            else if (string.Equals(activeTool, "Eraser", StringComparison.Ordinal))
                pnlCanvas.Cursor = Cursors.Hand;
            else
                pnlCanvas.Cursor = Cursors.Cross;
        }

        private void UpdateEditingButtonStyle()
        {
            btnMaskEdit.BackColor = string.Equals(activeTool, "Select", StringComparison.Ordinal)
                ? Color.FromArgb(210, 225, 242)
                : UiTheme.Surface;
        }

        private void UpdateCanvasCaptionEnhanced()
        {
            if (currentImage == null) return;
            string instruction;
            switch (activeTool)
            {
                case "Select": instruction = "单击选择，拖动移动，拖控制点编辑；Delete 删除；双击恢复 Fit"; break;
                case "Polygon": instruction = "单击添加顶点，双击完成，右键取消"; break;
                case "Brush": instruction = "按住左键绘制笔迹，右键取消"; break;
                case "Eraser": instruction = "单击标注区域删除"; break;
                default: instruction = "按住左键拖拽矩形，右键取消"; break;
            }
            string zoom = Math.Abs(viewZoom - 1F) < 0.005F ? "Fit" : (viewZoom * 100F).ToString("0") + "%";
            string caption = "标注画布 | " + currentImage.Width + " × " + currentImage.Height +
                             " | " + DisplayDefinitionVersion(currentImage.ProductDefinitionVersion) +
                             " | " + zoom + " | " + instruction +
                             " | 滚轮缩放 / 中键或 Space+拖拽平移 / Ctrl+Z/Y";
            if (string.Equals(lastEnhancedCaption, caption, StringComparison.Ordinal)) return;
            lastEnhancedCaption = caption;
            grpCanvas.Text = caption;
        }

        private void EnsureEditingContext()
        {
            long imageId = currentImage == null ? 0 : currentImage.Id;
            if (editingImageId == imageId) return;

            editingImageId = imageId;
            selectedAnnotationId = 0;
            annotationIdentities.Clear();
            undoHistory.Clear();
            redoHistory.Clear();
            viewZoom = 1F;
            viewPan = PointF.Empty;
            panningCanvas = false;
            spacePanHeld = false;
            lastEnhancedCaption = null;
            ClearAnnotationEditState();
        }

        private Color GetAnnotationColor(DatasetAnnotation annotation)
        {
            Color[] palette =
            {
                Color.FromArgb(255, 82, 82), Color.FromArgb(64, 196, 255), Color.FromArgb(105, 240, 174),
                Color.FromArgb(255, 215, 64), Color.FromArgb(179, 136, 255), Color.FromArgb(255, 128, 171)
            };
            long key = annotation.CategoryId ?? (annotation.CategoryName ?? string.Empty).GetHashCode();
            int index = (int)(Math.Abs(key % palette.Length));
            return palette[index];
        }

        private void CancelWorkingAnnotation()
        {
            drawingAnnotation = false;
            workingPoints.Clear();
            hasHoverPoint = false;
            if (pnlCanvas != null) pnlCanvas.Invalidate();
        }

        private static PointF[] GetRectangleHandlePoints(RectangleF rectangle)
        {
            float centerX = rectangle.Left + rectangle.Width / 2F;
            float centerY = rectangle.Top + rectangle.Height / 2F;
            return new[]
            {
                new PointF(rectangle.Left, rectangle.Top),
                new PointF(centerX, rectangle.Top),
                new PointF(rectangle.Right, rectangle.Top),
                new PointF(rectangle.Right, centerY),
                new PointF(rectangle.Right, rectangle.Bottom),
                new PointF(centerX, rectangle.Bottom),
                new PointF(rectangle.Left, rectangle.Bottom),
                new PointF(rectangle.Left, centerY)
            };
        }

        private static RectangleF GetPointBounds(IList<PointF> points)
        {
            if (points == null || points.Count == 0) return RectangleF.Empty;
            float left = points[0].X;
            float top = points[0].Y;
            float right = points[0].X;
            float bottom = points[0].Y;
            for (int i = 1; i < points.Count; i++)
            {
                left = Math.Min(left, points[i].X);
                top = Math.Min(top, points[i].Y);
                right = Math.Max(right, points[i].X);
                bottom = Math.Max(bottom, points[i].Y);
            }
            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        private static RectangleF MakeRectangle(PointF first, PointF second)
        {
            return RectangleF.FromLTRB(
                Math.Min(first.X, second.X), Math.Min(first.Y, second.Y),
                Math.Max(first.X, second.X), Math.Max(first.Y, second.Y));
        }

        private static float Distance(PointF first, PointF second)
        {
            float dx = first.X - second.X;
            float dy = first.Y - second.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private static float DistanceToSegment(PointF point, PointF start, PointF end)
        {
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            if (Math.Abs(dx) < float.Epsilon && Math.Abs(dy) < float.Epsilon) return Distance(point, start);
            float t = ((point.X - start.X) * dx + (point.Y - start.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0F, Math.Min(1F, t));
            return Distance(point, new PointF(start.X + t * dx, start.Y + t * dy));
        }

        private static bool IsPointInPolygon(IList<PointF> polygon, PointF point)
        {
            bool inside = false;
            int j = polygon.Count - 1;
            for (int i = 0; i < polygon.Count; i++)
            {
                PointF a = polygon[i];
                PointF b = polygon[j];
                if ((a.Y > point.Y) != (b.Y > point.Y) &&
                    point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X)
                    inside = !inside;
                j = i;
            }
            return inside;
        }

        private static DatasetAnnotation CloneAnnotation(DatasetAnnotation source)
        {
            if (source == null) return null;
            return new DatasetAnnotation
            {
                Id = source.Id,
                DatasetImageId = source.DatasetImageId,
                CategoryId = source.CategoryId,
                CategoryCode = source.CategoryCode,
                CategoryName = source.CategoryName,
                AnnotationType = source.AnnotationType,
                GeometryData = source.GeometryData,
                BrushWidth = source.BrushWidth,
                Confidence = source.Confidence,
                IsVisible = source.IsVisible,
                CreatedAtUtc = source.CreatedAtUtc,
                UpdatedAtUtc = source.UpdatedAtUtc
            };
        }

        private enum AnnotationEditMode
        {
            None,
            Move,
            RectangleResize,
            PolygonVertex
        }

        private enum AnnotationHistoryKind
        {
            Create,
            Delete,
            Update
        }

        private sealed class AnnotationIdentity
        {
            public long CurrentId { get; set; }
        }

        private sealed class AnnotationHistoryItem
        {
            public AnnotationHistoryItem(
                AnnotationHistoryKind kind,
                AnnotationIdentity identity,
                DatasetAnnotation before,
                DatasetAnnotation after)
            {
                Kind = kind;
                Identity = identity;
                Before = before;
                After = after;
            }

            public AnnotationHistoryKind Kind { get; private set; }
            public AnnotationIdentity Identity { get; private set; }
            public DatasetAnnotation Before { get; private set; }
            public DatasetAnnotation After { get; private set; }
        }
    }
}
