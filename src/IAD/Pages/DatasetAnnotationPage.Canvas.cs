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
        private bool drawingAnnotation;
        private readonly List<PointF> workingPoints = new List<PointF>();
        private PointF hoverImagePoint;
        private bool hasHoverPoint;

        private void BindCanvasEvents()
        {
            pnlCanvas.Paint += pnlCanvas_Paint;
            pnlCanvas.MouseDown += pnlCanvas_MouseDown;
            pnlCanvas.MouseMove += pnlCanvas_MouseMove;
            pnlCanvas.MouseUp += pnlCanvas_MouseUp;
            pnlCanvas.MouseClick += pnlCanvas_MouseClick;
            pnlCanvas.MouseDoubleClick += pnlCanvas_MouseDoubleClick;
            pnlCanvas.MouseLeave += delegate { hasHoverPoint = false; pnlCanvas.Invalidate(); };
            pnlCanvas.KeyDown += pnlCanvas_KeyDown;
        }

        private void pnlCanvas_Paint(object sender, PaintEventArgs e)
        {
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

            using (Pen border = new Pen(Color.FromArgb(145, 255, 255, 255), 1F))
                e.Graphics.DrawRectangle(border, imageBounds.X, imageBounds.Y, imageBounds.Width, imageBounds.Height);
        }

        private void DrawAnnotation(Graphics graphics, DatasetAnnotation annotation, RectangleF imageBounds)
        {
            List<PointF> imagePoints;
            try { imagePoints = AnnotationGeometry.Parse(annotation.GeometryData); }
            catch { return; }

            PointF[] points = ToCanvasPoints(imagePoints, imageBounds);
            Color color = GetAnnotationColor(annotation);
            using (Pen pen = new Pen(color, 2F))
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(45, color)))
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

        private void pnlCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            pnlCanvas.Focus();
            if (e.Button == MouseButtons.Right)
            {
                CancelWorkingAnnotation();
                return;
            }
            if (e.Button != MouseButtons.Left || currentBitmap == null) return;

            PointF imagePoint;
            if (!TryCanvasToImage(e.Location, out imagePoint)) return;
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
            PointF imagePoint;
            bool inside = TryCanvasToImage(e.Location, out imagePoint);
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
            if (!string.Equals(activeTool, "Polygon", StringComparison.Ordinal) || e.Button != MouseButtons.Left) return;
            if (workingPoints.Count >= 3)
                SaveWorkingAnnotation("Polygon");
            else
                CancelWorkingAnnotation();
        }

        private void pnlCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                CancelWorkingAnnotation();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && string.Equals(activeTool, "Polygon", StringComparison.Ordinal) && workingPoints.Count >= 3)
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
                AppServices.Datasets.CreateAnnotation(
                    currentImage.Id,
                    category.Id,
                    annotationType,
                    AnnotationGeometry.Serialize(workingPoints),
                    (float)numLineWidth.Value,
                    (double)numThreshold.Value);
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
            for (int i = currentAnnotations.Count - 1; i >= 0; i--)
            {
                DatasetAnnotation annotation = currentAnnotations[i];
                if (!annotation.IsVisible || !HitTest(annotation, imagePoint)) continue;
                try
                {
                    AppServices.Datasets.DeleteAnnotation(currentImage.Id, annotation.Id);
                    RefreshCurrentAnnotations();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "删除标注失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }
        }

        private bool HitTest(DatasetAnnotation annotation, PointF point)
        {
            List<PointF> points;
            try { points = AnnotationGeometry.Parse(annotation.GeometryData); }
            catch { return false; }

            if (string.Equals(annotation.AnnotationType, "Rectangle", StringComparison.OrdinalIgnoreCase) && points.Count == 2)
            {
                RectangleF rectangle = MakeRectangle(points[0], points[1]);
                rectangle.Inflate(Math.Max(2F, annotation.BrushWidth), Math.Max(2F, annotation.BrushWidth));
                return rectangle.Contains(point);
            }
            if (string.Equals(annotation.AnnotationType, "Polygon", StringComparison.OrdinalIgnoreCase) && points.Count >= 3)
                return IsPointInPolygon(points, point);
            if (string.Equals(annotation.AnnotationType, "Brush", StringComparison.OrdinalIgnoreCase) && points.Count >= 2)
            {
                float tolerance = Math.Max(3F, annotation.BrushWidth / 2F + 2F);
                for (int i = 1; i < points.Count; i++)
                {
                    if (DistanceToSegment(point, points[i - 1], points[i]) <= tolerance) return true;
                }
            }
            return false;
        }

        private RectangleF GetImageDisplayBounds()
        {
            if (currentBitmap == null || pnlCanvas.ClientSize.Width <= 0 || pnlCanvas.ClientSize.Height <= 0)
                return RectangleF.Empty;
            const float padding = 10F;
            float availableWidth = Math.Max(1F, pnlCanvas.ClientSize.Width - padding * 2F);
            float availableHeight = Math.Max(1F, pnlCanvas.ClientSize.Height - padding * 2F);
            float scale = Math.Min(availableWidth / currentBitmap.Width, availableHeight / currentBitmap.Height);
            float width = currentBitmap.Width * scale;
            float height = currentBitmap.Height * scale;
            return new RectangleF((pnlCanvas.ClientSize.Width - width) / 2F, (pnlCanvas.ClientSize.Height - height) / 2F, width, height);
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
    }
}
