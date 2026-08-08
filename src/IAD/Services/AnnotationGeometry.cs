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
