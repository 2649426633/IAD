using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using IAD.Models;

namespace IAD.Services
{
    internal static class TemplateMatchingEngine
    {
        private static readonly double[] SearchScales = { 0.8D, 1D, 1.25D };

        public static IList<DefectRecognitionCandidate> Generate(
            DatasetService datasets,
            IList<DatasetImage> images,
            IList<DefectPrototypeSample> prototypes,
            IList<DefectHardNegative> hardNegatives,
            long productId,
            long categoryId,
            double threshold,
            int topK)
        {
            if (datasets == null) throw new ArgumentNullException("datasets");
            if (prototypes == null || prototypes.Count == 0)
                throw new InvalidOperationException("当前类别没有正样本。请先在“数据集标注”中确认至少一个该类别标注。");

            List<TemplateFeature> positiveFeatures = LoadPrototypeFeatures(datasets, prototypes);
            if (positiveFeatures.Count == 0)
                throw new InvalidOperationException("正样本图像均无法读取，无法生成识别候选。");
            List<FeatureVector> negativeFeatures = LoadHardNegativeFeatures(datasets, images, hardNegatives);
            positiveFeatures = BuildCategoryPrototype(positiveFeatures);
            negativeFeatures = BuildNegativePrototype(negativeFeatures);
            List<ScoredRegion> scored = new List<ScoredRegion>();
            int candidatePoolLimit = Math.Max(200, topK * 50);

            foreach (DatasetImage image in images)
            {
                string imagePath = datasets.GetImagePath(image);
                if (!System.IO.File.Exists(imagePath)) continue;

                using (GrayImage source = GrayImage.Load(imagePath))
                {
                    List<RectangleF> existing = GetExistingCategoryBounds(datasets.GetAnnotations(image.Id), categoryId);
                    HashSet<string> scannedSizes = new HashSet<string>(StringComparer.Ordinal);
                    foreach (TemplateFeature prototype in positiveFeatures)
                    {
                        foreach (double scale in SearchScales)
                        {
                            int width = Math.Max(8, Math.Min(source.Width, (int)Math.Round(prototype.Width * scale)));
                            int height = Math.Max(8, Math.Min(source.Height, (int)Math.Round(prototype.Height * scale)));
                            string sizeKey = width + "x" + height;
                            if (!scannedSizes.Add(sizeKey)) continue;
                            ScanSize(source, image, width, height, existing, positiveFeatures, negativeFeatures,
                                threshold, scored, candidatePoolLimit);
                        }
                    }
                }
            }

            scored.Sort(delegate (ScoredRegion left, ScoredRegion right) { return right.Score.CompareTo(left.Score); });
            List<DefectRecognitionCandidate> result = new List<DefectRecognitionCandidate>();
            DateTime now = DateTime.UtcNow;
            foreach (ScoredRegion item in scored)
            {
                bool overlaps = false;
                foreach (DefectRecognitionCandidate accepted in result)
                {
                    if (accepted.DatasetImageId != item.Image.Id) continue;
                    if (IntersectionOverUnion(ParseRectangle(accepted.GeometryData), item.Bounds) > 0.35D)
                    {
                        overlaps = true;
                        break;
                    }
                }
                if (overlaps) continue;

                result.Add(new DefectRecognitionCandidate
                {
                    ProductId = productId,
                    CategoryId = categoryId,
                    DatasetImageId = item.Image.Id,
                    SourceFileName = item.Image.FileName,
                    Similarity = item.Score,
                    GeometryData = SerializeRectangle(item.Bounds),
                    Status = "待确认",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
                if (result.Count >= topK) break;
            }
            return result;
        }

        private static List<TemplateFeature> BuildCategoryPrototype(IList<TemplateFeature> samples)
        {
            int width = 0;
            int height = 0;
            List<FeatureVector> features = new List<FeatureVector>();
            foreach (TemplateFeature sample in samples)
            {
                width += sample.Width;
                height += sample.Height;
                features.Add(sample.Feature);
            }
            return new List<TemplateFeature>
            {
                new TemplateFeature
                {
                    Width = Math.Max(8, width / samples.Count),
                    Height = Math.Max(8, height / samples.Count),
                    Feature = FeatureVector.Average(features)
                }
            };
        }

        private static List<FeatureVector> BuildNegativePrototype(IList<FeatureVector> samples)
        {
            if (samples == null || samples.Count == 0) return new List<FeatureVector>();
            return new List<FeatureVector> { FeatureVector.Average(samples) };
        }

        public static string RefineBounds(string imagePath, string geometryData)
        {
            RectangleF bounds = ParseRectangle(geometryData);
            using (GrayImage image = GrayImage.Load(imagePath))
            {
                Rectangle region = Rectangle.Round(bounds);
                region.Intersect(new Rectangle(0, 0, image.Width, image.Height));
                if (region.Width < 4 || region.Height < 4) return geometryData;

                int step = Math.Max(1, Math.Min(region.Width, region.Height) / 128);
                double borderMean = BorderMean(image, region, step);
                double borderDeviation = BorderDeviation(image, region, step, borderMean);
                double differenceThreshold = Math.Max(12D, borderDeviation * 1.5D);
                int left = region.Right;
                int top = region.Bottom;
                int right = region.Left;
                int bottom = region.Top;
                int hits = 0;

                for (int y = region.Top; y < region.Bottom; y += step)
                {
                    for (int x = region.Left; x < region.Right; x += step)
                    {
                        if (Math.Abs(image.Get(x, y) - borderMean) < differenceThreshold) continue;
                        left = Math.Min(left, x);
                        top = Math.Min(top, y);
                        right = Math.Max(right, x);
                        bottom = Math.Max(bottom, y);
                        hits++;
                    }
                }

                if (hits < 4 || right - left < 2 || bottom - top < 2) return geometryData;
                int padding = Math.Max(2, step * 2);
                RectangleF refined = RectangleF.FromLTRB(
                    Math.Max(0, left - padding), Math.Max(0, top - padding),
                    Math.Min(image.Width, right + padding), Math.Min(image.Height, bottom + padding));
                return SerializeRectangle(refined);
            }
        }

        public static RectangleF ParseRectangle(string geometryData)
        {
            List<PointF> points = AnnotationGeometry.Parse(geometryData);
            if (points.Count < 2) throw new ArgumentException("候选区域格式无效。", "geometryData");
            float left = float.MaxValue;
            float top = float.MaxValue;
            float right = float.MinValue;
            float bottom = float.MinValue;
            foreach (PointF point in points)
            {
                left = Math.Min(left, point.X);
                top = Math.Min(top, point.Y);
                right = Math.Max(right, point.X);
                bottom = Math.Max(bottom, point.Y);
            }
            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        public static string SerializeRectangle(RectangleF bounds)
        {
            return AnnotationGeometry.Serialize(new[]
            {
                new PointF(bounds.Left, bounds.Top),
                new PointF(bounds.Right, bounds.Bottom)
            });
        }

        private static List<TemplateFeature> LoadPrototypeFeatures(DatasetService datasets, IList<DefectPrototypeSample> prototypes)
        {
            List<TemplateFeature> result = new List<TemplateFeature>();
            foreach (DefectPrototypeSample sample in prototypes)
            {
                try
                {
                    RectangleF bounds = GetAnnotationBounds(sample.Annotation);
                    using (GrayImage image = GrayImage.Load(datasets.GetImagePath(sample.Image)))
                    {
                        result.Add(new TemplateFeature
                        {
                            Width = Math.Max(8, (int)Math.Round(bounds.Width)),
                            Height = Math.Max(8, (int)Math.Round(bounds.Height)),
                            Feature = FeatureVector.Extract(image, bounds)
                        });
                    }
                }
                catch
                {
                    // 单个损坏样本不阻断其他可用原型。
                }
            }
            return result;
        }

        private static List<FeatureVector> LoadHardNegativeFeatures(
            DatasetService datasets,
            IList<DatasetImage> images,
            IList<DefectHardNegative> hardNegatives)
        {
            Dictionary<long, DatasetImage> imageById = new Dictionary<long, DatasetImage>();
            foreach (DatasetImage image in images) imageById[image.Id] = image;
            List<FeatureVector> result = new List<FeatureVector>();
            foreach (DefectHardNegative item in hardNegatives)
            {
                DatasetImage image;
                if (!imageById.TryGetValue(item.DatasetImageId, out image)) continue;
                try
                {
                    using (GrayImage source = GrayImage.Load(datasets.GetImagePath(image)))
                        result.Add(FeatureVector.Extract(source, ParseRectangle(item.GeometryData)));
                }
                catch
                {
                    // 忽略文件缺失或区域损坏的负样本。
                }
            }
            return result;
        }

        private static void ScanSize(
            GrayImage source,
            DatasetImage image,
            int windowWidth,
            int windowHeight,
            IList<RectangleF> existing,
            IList<TemplateFeature> positives,
            IList<FeatureVector> negatives,
            double threshold,
            List<ScoredRegion> scored,
            int candidatePoolLimit)
        {
            int strideX = Math.Max(16, windowWidth / 2);
            int strideY = Math.Max(16, windowHeight / 2);
            List<int> xs = BuildPositions(source.Width, windowWidth, strideX);
            List<int> ys = BuildPositions(source.Height, windowHeight, strideY);
            foreach (int y in ys)
            {
                foreach (int x in xs)
                {
                    RectangleF bounds = new RectangleF(x, y, windowWidth, windowHeight);
                    if (OverlapsExisting(bounds, existing)) continue;
                    FeatureVector feature = FeatureVector.Extract(source, bounds);
                    double positiveScore = 0;
                    foreach (TemplateFeature prototype in positives)
                        positiveScore = Math.Max(positiveScore, FeatureVector.Similarity(prototype.Feature, feature));

                    double negativeScore = 0;
                    foreach (FeatureVector negative in negatives)
                        negativeScore = Math.Max(negativeScore, FeatureVector.Similarity(negative, feature));
                    double adjusted = positiveScore - Math.Max(0D, negativeScore - 0.70D) * 0.60D;
                    adjusted = Math.Max(0D, Math.Min(1D, adjusted));
                    if (adjusted < threshold) continue;
                    scored.Add(new ScoredRegion { Image = image, Bounds = bounds, Score = adjusted });
                    if (scored.Count >= candidatePoolLimit * 2)
                    {
                        scored.Sort(delegate (ScoredRegion left, ScoredRegion right) { return right.Score.CompareTo(left.Score); });
                        scored.RemoveRange(candidatePoolLimit, scored.Count - candidatePoolLimit);
                    }
                }
            }
        }

        private static List<int> BuildPositions(int imageSize, int windowSize, int stride)
        {
            List<int> values = new List<int>();
            int last = Math.Max(0, imageSize - windowSize);
            for (int value = 0; value <= last; value += stride) values.Add(value);
            if (values.Count == 0 || values[values.Count - 1] != last) values.Add(last);
            return values;
        }

        private static List<RectangleF> GetExistingCategoryBounds(IList<DatasetAnnotation> annotations, long categoryId)
        {
            List<RectangleF> result = new List<RectangleF>();
            foreach (DatasetAnnotation annotation in annotations)
            {
                if (annotation.CategoryId != categoryId) continue;
                try { result.Add(GetAnnotationBounds(annotation)); }
                catch { }
            }
            return result;
        }

        private static RectangleF GetAnnotationBounds(DatasetAnnotation annotation)
        {
            RectangleF bounds = ParseRectangle(annotation.GeometryData);
            if (string.Equals(annotation.AnnotationType, "Brush", StringComparison.OrdinalIgnoreCase))
                bounds.Inflate(Math.Max(1F, annotation.BrushWidth / 2F), Math.Max(1F, annotation.BrushWidth / 2F));
            return bounds;
        }

        private static bool OverlapsExisting(RectangleF bounds, IList<RectangleF> existing)
        {
            foreach (RectangleF item in existing)
            {
                if (IntersectionOverUnion(bounds, item) > 0.20D) return true;
            }
            return false;
        }

        private static double IntersectionOverUnion(RectangleF left, RectangleF right)
        {
            RectangleF intersection = RectangleF.Intersect(left, right);
            if (intersection.Width <= 0 || intersection.Height <= 0) return 0D;
            double intersectionArea = intersection.Width * intersection.Height;
            double union = left.Width * left.Height + right.Width * right.Height - intersectionArea;
            return union <= 0 ? 0D : intersectionArea / union;
        }

        private static double BorderMean(GrayImage image, Rectangle bounds, int step)
        {
            double sum = 0;
            int count = 0;
            for (int x = bounds.Left; x < bounds.Right; x += step)
            {
                sum += image.Get(x, bounds.Top) + image.Get(x, bounds.Bottom - 1);
                count += 2;
            }
            for (int y = bounds.Top; y < bounds.Bottom; y += step)
            {
                sum += image.Get(bounds.Left, y) + image.Get(bounds.Right - 1, y);
                count += 2;
            }
            return count == 0 ? 0D : sum / count;
        }

        private static double BorderDeviation(GrayImage image, Rectangle bounds, int step, double mean)
        {
            double sum = 0;
            int count = 0;
            for (int x = bounds.Left; x < bounds.Right; x += step)
            {
                sum += Math.Abs(image.Get(x, bounds.Top) - mean) + Math.Abs(image.Get(x, bounds.Bottom - 1) - mean);
                count += 2;
            }
            for (int y = bounds.Top; y < bounds.Bottom; y += step)
            {
                sum += Math.Abs(image.Get(bounds.Left, y) - mean) + Math.Abs(image.Get(bounds.Right - 1, y) - mean);
                count += 2;
            }
            return count == 0 ? 0D : sum / count;
        }

        private sealed class TemplateFeature
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public FeatureVector Feature { get; set; }
        }

        private sealed class ScoredRegion
        {
            public DatasetImage Image { get; set; }
            public RectangleF Bounds { get; set; }
            public double Score { get; set; }
        }

        private sealed class FeatureVector
        {
            private const int GridSize = 8;
            public double[] Values { get; private set; }
            public double Mean { get; private set; }
            public double Deviation { get; private set; }

            public static FeatureVector Extract(GrayImage image, RectangleF bounds)
            {
                RectangleF clamped = RectangleF.Intersect(bounds, new RectangleF(0, 0, image.Width, image.Height));
                if (clamped.Width < 1 || clamped.Height < 1)
                    throw new ArgumentException("模板区域超出图片范围。", "bounds");
                double[] raw = new double[GridSize * GridSize];
                double sum = 0;
                int index = 0;
                for (int y = 0; y < GridSize; y++)
                {
                    for (int x = 0; x < GridSize; x++)
                    {
                        int sampleX = Math.Min(image.Width - 1, Math.Max(0,
                            (int)(clamped.Left + (x + 0.5D) * clamped.Width / GridSize)));
                        int sampleY = Math.Min(image.Height - 1, Math.Max(0,
                            (int)(clamped.Top + (y + 0.5D) * clamped.Height / GridSize)));
                        raw[index] = image.Get(sampleX, sampleY) / 255D;
                        sum += raw[index];
                        index++;
                    }
                }

                double mean = sum / raw.Length;
                double variance = 0;
                foreach (double value in raw) variance += (value - mean) * (value - mean);
                double deviation = Math.Sqrt(variance / raw.Length);
                double divisor = Math.Max(0.02D, deviation);
                double[] normalized = new double[raw.Length];
                for (int i = 0; i < raw.Length; i++) normalized[i] = (raw[i] - mean) / divisor;
                return new FeatureVector { Values = normalized, Mean = mean, Deviation = deviation };
            }

            public static double Similarity(FeatureVector left, FeatureVector right)
            {
                double dot = 0;
                double leftLength = 0;
                double rightLength = 0;
                for (int i = 0; i < left.Values.Length; i++)
                {
                    dot += left.Values[i] * right.Values[i];
                    leftLength += left.Values[i] * left.Values[i];
                    rightLength += right.Values[i] * right.Values[i];
                }
                double correlation = leftLength <= 0 || rightLength <= 0
                    ? 0D
                    : dot / Math.Sqrt(leftLength * rightLength);
                double structure = Math.Max(0D, Math.Min(1D, (correlation + 1D) / 2D));
                double meanSimilarity = 1D - Math.Min(1D, Math.Abs(left.Mean - right.Mean));
                double deviationSimilarity = 1D - Math.Min(1D, Math.Abs(left.Deviation - right.Deviation) * 4D);
                return Math.Max(0D, Math.Min(1D, structure * 0.80D + meanSimilarity * 0.10D + deviationSimilarity * 0.10D));
            }

            public static FeatureVector Average(IList<FeatureVector> samples)
            {
                if (samples == null || samples.Count == 0)
                    throw new ArgumentException("原型样本不能为空。", "samples");
                double[] values = new double[samples[0].Values.Length];
                double mean = 0;
                double deviation = 0;
                foreach (FeatureVector sample in samples)
                {
                    for (int i = 0; i < values.Length; i++) values[i] += sample.Values[i];
                    mean += sample.Mean;
                    deviation += sample.Deviation;
                }
                for (int i = 0; i < values.Length; i++) values[i] /= samples.Count;
                return new FeatureVector
                {
                    Values = values,
                    Mean = mean / samples.Count,
                    Deviation = deviation / samples.Count
                };
            }
        }

        private sealed class GrayImage : IDisposable
        {
            private readonly byte[] values;
            public int Width { get; private set; }
            public int Height { get; private set; }

            private GrayImage(int width, int height, byte[] values)
            {
                Width = width;
                Height = height;
                this.values = values;
            }

            public byte Get(int x, int y)
            {
                return values[y * Width + x];
            }

            public static GrayImage Load(string path)
            {
                using (Image source = Image.FromFile(path))
                using (Bitmap bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap)) graphics.DrawImageUnscaled(source, 0, 0);
                    Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    BitmapData data = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                    try
                    {
                        int byteCount = Math.Abs(data.Stride) * data.Height;
                        byte[] sourceBytes = new byte[byteCount];
                        Marshal.Copy(data.Scan0, sourceBytes, 0, byteCount);
                        byte[] gray = new byte[bitmap.Width * bitmap.Height];
                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            int row = data.Stride > 0 ? y * data.Stride : (bitmap.Height - 1 - y) * -data.Stride;
                            for (int x = 0; x < bitmap.Width; x++)
                            {
                                int offset = row + x * 3;
                                gray[y * bitmap.Width + x] = (byte)Math.Min(255,
                                    (sourceBytes[offset] * 29 + sourceBytes[offset + 1] * 150 + sourceBytes[offset + 2] * 77) >> 8);
                            }
                        }
                        return new GrayImage(bitmap.Width, bitmap.Height, gray);
                    }
                    finally
                    {
                        bitmap.UnlockBits(data);
                    }
                }
            }

            public void Dispose()
            {
                // 仅托管数组，无需释放非托管资源。
            }
        }
    }
}
