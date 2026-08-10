using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace IAD.Services
{
    internal static class AnnotationGeometry
    {
        public static string Serialize(IList<PointF> points)
        {
            if (points == null || points.Count == 0) return string.Empty;
            string[] values = new string[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                values[i] = points[i].X.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                            points[i].Y.ToString("0.###", CultureInfo.InvariantCulture);
            }
            return string.Join(";", values);
        }

        public static List<PointF> Parse(string geometryData)
        {
            if (string.IsNullOrWhiteSpace(geometryData))
                throw new ArgumentException("标注几何数据不能为空。", "geometryData");

            List<PointF> points = new List<PointF>();
            string[] pairs = geometryData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string pair in pairs)
            {
                string[] values = pair.Split(',');
                float x;
                float y;
                if (values.Length != 2 ||
                    !float.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x) ||
                    !float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y) ||
                    float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(y) || float.IsInfinity(y))
                    throw new ArgumentException("标注几何数据格式不正确。", "geometryData");
                points.Add(new PointF(x, y));
            }

            if (points.Count == 0)
                throw new ArgumentException("标注几何数据不能为空。", "geometryData");
            return points;
        }

        public static string ValidateAndNormalize(string annotationType, string geometryData, int imageWidth, int imageHeight)
        {
            if (imageWidth <= 0 || imageHeight <= 0)
                throw new ArgumentException("数据集图片尺寸无效。", "imageWidth");

            List<PointF> points = Parse(geometryData);
            string type = string.IsNullOrWhiteSpace(annotationType) ? string.Empty : annotationType.Trim();
            if (string.Equals(type, "Rectangle", StringComparison.OrdinalIgnoreCase))
            {
                if (points.Count != 2)
                    throw new ArgumentException("矩形标注必须包含两个对角点。", "geometryData");
                float left = Math.Min(points[0].X, points[1].X);
                float top = Math.Min(points[0].Y, points[1].Y);
                float right = Math.Max(points[0].X, points[1].X);
                float bottom = Math.Max(points[0].Y, points[1].Y);
                if (right - left < 1F || bottom - top < 1F)
                    throw new ArgumentException("矩形标注尺寸过小。", "geometryData");
                points[0] = new PointF(left, top);
                points[1] = new PointF(right, bottom);
            }
            else if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
            {
                if (points.Count < 3)
                    throw new ArgumentException("多边形标注至少需要三个顶点。", "geometryData");
                if (Math.Abs(SignedArea(points)) < 0.5D)
                    throw new ArgumentException("多边形标注面积过小。", "geometryData");
            }
            else if (string.Equals(type, "Brush", StringComparison.OrdinalIgnoreCase))
            {
                if (points.Count < 2)
                    throw new ArgumentException("画笔标注至少需要两个采样点。", "geometryData");
            }
            else
            {
                throw new ArgumentException("不支持的标注类型：" + type, "annotationType");
            }

            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].X < 0 || points[i].Y < 0 || points[i].X > imageWidth || points[i].Y > imageHeight)
                    throw new ArgumentException("标注超出图像边界。", "geometryData");
            }
            return Serialize(points);
        }

        public static string Clamp(string geometryData, int imageWidth, int imageHeight)
        {
            List<PointF> points = Parse(geometryData);
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new PointF(
                    Math.Max(0F, Math.Min(imageWidth, points[i].X)),
                    Math.Max(0F, Math.Min(imageHeight, points[i].Y)));
            }
            return Serialize(points);
        }

        public static string RepairToBounds(
            string annotationType,
            string geometryData,
            float brushWidth,
            int imageWidth,
            int imageHeight)
        {
            if (imageWidth <= 0 || imageHeight <= 0)
                throw new ArgumentException("数据集图片尺寸无效。", "imageWidth");

            string type = string.IsNullOrWhiteSpace(annotationType) ? string.Empty : annotationType.Trim();
            List<PointF> points = Parse(geometryData);

            if (string.Equals(type, "Rectangle", StringComparison.OrdinalIgnoreCase))
                return RepairRectangle(points, imageWidth, imageHeight);

            if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
                return RepairPolygon(points, imageWidth, imageHeight);

            if (string.Equals(type, "Brush", StringComparison.OrdinalIgnoreCase))
                return RepairBrush(points, brushWidth, imageWidth, imageHeight);

            throw new ArgumentException("不支持的标注类型：" + type, "annotationType");
        }

        private static string RepairRectangle(IList<PointF> points, int imageWidth, int imageHeight)
        {
            if (points.Count != 2)
                throw new ArgumentException("矩形标注必须包含两个对角点。", "geometryData");

            float left = Math.Max(0F, Math.Min(points[0].X, points[1].X));
            float top = Math.Max(0F, Math.Min(points[0].Y, points[1].Y));
            float right = Math.Min(imageWidth, Math.Max(points[0].X, points[1].X));
            float bottom = Math.Min(imageHeight, Math.Max(points[0].Y, points[1].Y));

            if (right - left < 1F || bottom - top < 1F)
                throw new InvalidOperationException("矩形标注与图像有效区域没有足够的交集，无法自动修复。");

            return ValidateAndNormalize(
                "Rectangle",
                Serialize(new[] { new PointF(left, top), new PointF(right, bottom) }),
                imageWidth,
                imageHeight);
        }

        private static string RepairPolygon(IList<PointF> points, int imageWidth, int imageHeight)
        {
            if (points.Count < 3)
                throw new ArgumentException("多边形标注至少需要三个顶点。", "geometryData");

            List<PointF> clipped = new List<PointF>(points);
            clipped = ClipPolygon(clipped, ClipEdge.Left, 0F);
            clipped = ClipPolygon(clipped, ClipEdge.Right, imageWidth);
            clipped = ClipPolygon(clipped, ClipEdge.Top, 0F);
            clipped = ClipPolygon(clipped, ClipEdge.Bottom, imageHeight);
            RemoveDuplicatePolygonPoints(clipped);

            if (clipped.Count < 3 || Math.Abs(SignedArea(clipped)) < 0.5D)
                throw new InvalidOperationException("多边形标注裁剪后没有有效区域，无法自动修复。");

            return ValidateAndNormalize("Polygon", Serialize(clipped), imageWidth, imageHeight);
        }

        private static string RepairBrush(IList<PointF> points, float brushWidth, int imageWidth, int imageHeight)
        {
            if (points.Count < 2)
                throw new ArgumentException("画笔标注至少需要两个采样点。", "geometryData");
            if (brushWidth < 1F || float.IsNaN(brushWidth) || float.IsInfinity(brushWidth))
                throw new ArgumentException("画笔宽度必须大于等于 1。", "brushWidth");

            float radius = brushWidth / 2F;
            float minX = radius;
            float maxX = imageWidth - radius;
            float minY = radius;
            float maxY = imageHeight - radius;
            if (maxX < minX || maxY < minY)
                throw new InvalidOperationException("画笔宽度大于图像可用尺寸，无法自动修复边界。");

            List<PointF> repaired = new List<PointF>(points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                repaired.Add(new PointF(
                    Math.Max(minX, Math.Min(maxX, points[i].X)),
                    Math.Max(minY, Math.Min(maxY, points[i].Y))));
            }

            RemoveConsecutiveDuplicates(repaired);
            if (repaired.Count < 2)
                throw new InvalidOperationException("画笔标注修复后有效轨迹不足，无法自动修复。");

            return ValidateAndNormalize("Brush", Serialize(repaired), imageWidth, imageHeight);
        }

        private enum ClipEdge
        {
            Left,
            Right,
            Top,
            Bottom
        }

        private static List<PointF> ClipPolygon(IList<PointF> input, ClipEdge edge, float boundary)
        {
            List<PointF> output = new List<PointF>();
            if (input == null || input.Count == 0) return output;

            PointF previous = input[input.Count - 1];
            bool previousInside = IsInside(previous, edge, boundary);
            for (int i = 0; i < input.Count; i++)
            {
                PointF current = input[i];
                bool currentInside = IsInside(current, edge, boundary);

                if (currentInside)
                {
                    if (!previousInside)
                        output.Add(IntersectBoundary(previous, current, edge, boundary));
                    output.Add(current);
                }
                else if (previousInside)
                {
                    output.Add(IntersectBoundary(previous, current, edge, boundary));
                }

                previous = current;
                previousInside = currentInside;
            }
            return output;
        }

        private static bool IsInside(PointF point, ClipEdge edge, float boundary)
        {
            switch (edge)
            {
                case ClipEdge.Left: return point.X >= boundary;
                case ClipEdge.Right: return point.X <= boundary;
                case ClipEdge.Top: return point.Y >= boundary;
                case ClipEdge.Bottom: return point.Y <= boundary;
                default: return false;
            }
        }

        private static PointF IntersectBoundary(PointF start, PointF end, ClipEdge edge, float boundary)
        {
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;

            if (edge == ClipEdge.Left || edge == ClipEdge.Right)
            {
                if (Math.Abs(dx) < float.Epsilon) return new PointF(boundary, start.Y);
                float t = (boundary - start.X) / dx;
                return new PointF(boundary, start.Y + t * dy);
            }

            if (Math.Abs(dy) < float.Epsilon) return new PointF(start.X, boundary);
            float verticalT = (boundary - start.Y) / dy;
            return new PointF(start.X + verticalT * dx, boundary);
        }

        private static void RemoveDuplicatePolygonPoints(IList<PointF> points)
        {
            RemoveConsecutiveDuplicates(points);
            if (points.Count > 1 && NearlyEqual(points[0], points[points.Count - 1]))
                points.RemoveAt(points.Count - 1);
        }

        private static void RemoveConsecutiveDuplicates(IList<PointF> points)
        {
            for (int i = points.Count - 1; i > 0; i--)
            {
                if (NearlyEqual(points[i], points[i - 1])) points.RemoveAt(i);
            }
        }

        private static bool NearlyEqual(PointF first, PointF second)
        {
            return Math.Abs(first.X - second.X) < 0.001F && Math.Abs(first.Y - second.Y) < 0.001F;
        }

        private static double SignedArea(IList<PointF> points)
        {
            double area = 0;
            for (int i = 0; i < points.Count; i++)
            {
                PointF current = points[i];
                PointF next = points[(i + 1) % points.Count];
                area += current.X * next.Y - next.X * current.Y;
            }
            return area / 2D;
        }
    }
}
