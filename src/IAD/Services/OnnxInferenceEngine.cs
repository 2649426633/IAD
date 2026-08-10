using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using IAD.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace IAD.Services
{
    internal sealed class InferencePrediction
    {
        public int ClassIndex { get; set; }
        public string Label { get; set; }
        public double Confidence { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    internal sealed class OnnxInferenceEngine
    {
        private sealed class PreprocessedInput
        {
            public DenseTensor<float> Tensor { get; set; }
            public double Scale { get; set; }
            public double PadX { get; set; }
            public double PadY { get; set; }
        }

        private readonly InferenceModelService modelService;
        private readonly object syncRoot = new object();
        private readonly Dictionary<long, InferenceSession> sessions = new Dictionary<long, InferenceSession>();

        public OnnxInferenceEngine(InferenceModelService modelService)
        {
            this.modelService = modelService ?? throw new ArgumentNullException("modelService");
        }

        public IList<InferencePrediction> Run(InferenceModel model, Bitmap source)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (source == null) throw new ArgumentNullException("source");
            InferenceSession session = GetSession(model);
            bool classification = string.Equals(model.ModelType, "Classification", StringComparison.OrdinalIgnoreCase);
            PreprocessedInput input = CreateInput(source, model.InputWidth, model.InputHeight, !classification);
            List<NamedOnnxValue> inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(model.InputName, input.Tensor) };
            using (IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs, new[] { model.OutputName }))
            {
                DisposableNamedOnnxValue output = results.First();
                Tensor<float> tensor = output.AsTensor<float>();
                int[] dimensions = tensor.Dimensions.ToArray();
                float[] values = tensor.ToArray();
                string[] labels = ParseLabels(model.Labels);
                if (classification)
                    return ParseClassification(values, labels, model.ConfidenceThreshold, source.Width, source.Height);
                return ParseYolo(values, dimensions, labels, model, source.Width, source.Height, input);
            }
        }

        private InferenceSession GetSession(InferenceModel model)
        {
            lock (syncRoot)
            {
                InferenceSession session;
                if (sessions.TryGetValue(model.Id, out session)) return session;
                string path = modelService.ResolveModelPath(model);
                session = new InferenceSession(path);
                sessions.Add(model.Id, session);
                return session;
            }
        }

        private static PreprocessedInput CreateInput(Bitmap source, int width, int height, bool preserveAspectRatio)
        {
            if (width <= 0 || height <= 0) throw new InvalidOperationException("模型输入尺寸无效。");
            using (Bitmap resized = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                double scale = preserveAspectRatio ? Math.Min(width / (double)source.Width, height / (double)source.Height) : 1D;
                int drawWidth = preserveAspectRatio ? Math.Max(1, (int)Math.Round(source.Width * scale)) : width;
                int drawHeight = preserveAspectRatio ? Math.Max(1, (int)Math.Round(source.Height * scale)) : height;
                int padX = preserveAspectRatio ? (width - drawWidth) / 2 : 0;
                int padY = preserveAspectRatio ? (height - drawHeight) / 2 : 0;
                using (Graphics graphics = Graphics.FromImage(resized))
                {
                    graphics.Clear(preserveAspectRatio ? Color.FromArgb(114, 114, 114) : Color.Black);
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                    graphics.DrawImage(source, padX, padY, drawWidth, drawHeight);
                }
                Rectangle rectangle = new Rectangle(0, 0, width, height);
                BitmapData bits = resized.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                try
                {
                    int stride = Math.Abs(bits.Stride);
                    byte[] pixels = new byte[stride * height];
                    Marshal.Copy(bits.Scan0, pixels, 0, pixels.Length);
                    float[] data = new float[3 * width * height];
                    int plane = width * height;
                    for (int y = 0; y < height; y++)
                    {
                        int row = bits.Stride >= 0 ? y * stride : (height - 1 - y) * stride;
                        for (int x = 0; x < width; x++)
                        {
                            int sourceIndex = row + x * 3;
                            int targetIndex = y * width + x;
                            data[targetIndex] = pixels[sourceIndex + 2] / 255f;
                            data[plane + targetIndex] = pixels[sourceIndex + 1] / 255f;
                            data[plane * 2 + targetIndex] = pixels[sourceIndex] / 255f;
                        }
                    }
                    return new PreprocessedInput
                    {
                        Tensor = new DenseTensor<float>(data, new[] { 1, 3, height, width }),
                        Scale = preserveAspectRatio ? scale : 1D,
                        PadX = padX,
                        PadY = padY
                    };
                }
                finally { resized.UnlockBits(bits); }
            }
        }

        private static IList<InferencePrediction> ParseClassification(float[] values, string[] labels, double threshold, int width, int height)
        {
            if (values == null || values.Length == 0) throw new InvalidOperationException("分类模型输出为空。");
            double[] scores = values.Select(v => (double)v).ToArray();
            double sum = scores.Sum();
            bool probabilities = scores.All(v => v >= 0 && v <= 1) && sum >= 0.98 && sum <= 1.02;
            if (!probabilities)
            {
                double max = scores.Max();
                double expSum = scores.Sum(v => Math.Exp(v - max));
                scores = scores.Select(v => Math.Exp(v - max) / expSum).ToArray();
            }
            int index = 0;
            for (int i = 1; i < scores.Length; i++) if (scores[i] > scores[index]) index = i;
            string label = index < labels.Length ? labels[index] : "class_" + index;
            if (scores[index] < threshold || IsNormalLabel(label)) return new List<InferencePrediction>();
            return new List<InferencePrediction>
            {
                new InferencePrediction { ClassIndex=index, Label=label, Confidence=scores[index], X=0, Y=0, Width=width, Height=height }
            };
        }

        private static IList<InferencePrediction> ParseYolo(float[] values, int[] dimensions, string[] labels, InferenceModel model, int sourceWidth, int sourceHeight, PreprocessedInput input)
        {
            if (dimensions.Length != 3 || dimensions[0] != 1) throw new InvalidOperationException("YOLO 输出必须是 [1,N,C] 或 [1,C,N] 三维张量。");
            bool modern = string.Equals(model.ModelType, "YoloV8", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(model.ModelType, "Yolo26", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(model.ModelType, "Yolo26", StringComparison.OrdinalIgnoreCase) && dimensions[2] == 6 && dimensions[1] >= 10)
                throw new InvalidOperationException("当前 YOLO26 ONNX 是端到端输出，请使用 end2end=False 重新导出后再导入。");
            bool transposed = modern && dimensions[1] < dimensions[2];
            int rows = transposed ? dimensions[2] : dimensions[1];
            int attributes = transposed ? dimensions[1] : dimensions[2];
            int classOffset = modern ? 4 : 5;
            if (attributes <= classOffset) throw new InvalidOperationException("YOLO 输出类别维度无效。");
            List<InferencePrediction> predictions = new List<InferencePrediction>();
            for (int row = 0; row < rows; row++)
            {
                Func<int, float> get = delegate(int attribute)
                {
                    return transposed ? values[attribute * rows + row] : values[row * attributes + attribute];
                };
                double objectness = modern ? 1D : get(4);
                int classIndex = 0;
                double classScore = get(classOffset);
                for (int i = classOffset + 1; i < attributes; i++)
                {
                    double score = get(i);
                    if (score > classScore) { classScore = score; classIndex = i - classOffset; }
                }
                double confidence = objectness * classScore;
                if (confidence < model.ConfidenceThreshold) continue;
                double cx = get(0), cy = get(1), boxWidth = get(2), boxHeight = get(3);
                bool normalized = Math.Abs(cx) <= 1 && Math.Abs(cy) <= 1 && boxWidth <= 1 && boxHeight <= 1;
                if (normalized)
                {
                    cx *= model.InputWidth;
                    cy *= model.InputHeight;
                    boxWidth *= model.InputWidth;
                    boxHeight *= model.InputHeight;
                }
                double scale = input.Scale <= 0 ? 1D : input.Scale;
                double left = Math.Max(0D, Math.Min(sourceWidth, (cx - boxWidth / 2D - input.PadX) / scale));
                double top = Math.Max(0D, Math.Min(sourceHeight, (cy - boxHeight / 2D - input.PadY) / scale));
                double right = Math.Max(0D, Math.Min(sourceWidth, (cx + boxWidth / 2D - input.PadX) / scale));
                double bottom = Math.Max(0D, Math.Min(sourceHeight, (cy + boxHeight / 2D - input.PadY) / scale));
                double x = left;
                double y = top;
                double width = Math.Max(0D, right - left);
                double height = Math.Max(0D, bottom - top);
                if (width <= 0D || height <= 0D) continue;
                predictions.Add(new InferencePrediction
                {
                    ClassIndex=classIndex, Label=classIndex < labels.Length ? labels[classIndex] : "class_" + classIndex,
                    Confidence=confidence, X=x, Y=y, Width=width, Height=height
                });
            }
            return NonMaximumSuppression(predictions, model.NmsThreshold);
        }

        private static IList<InferencePrediction> NonMaximumSuppression(IEnumerable<InferencePrediction> source, double threshold)
        {
            List<InferencePrediction> kept = new List<InferencePrediction>();
            foreach (IGrouping<int, InferencePrediction> group in source.GroupBy(p => p.ClassIndex))
            {
                List<InferencePrediction> remaining = group.OrderByDescending(p => p.Confidence).ToList();
                while (remaining.Count > 0)
                {
                    InferencePrediction best = remaining[0];
                    kept.Add(best);
                    remaining.RemoveAt(0);
                    remaining.RemoveAll(p => IntersectionOverUnion(best, p) > threshold);
                }
            }
            return kept;
        }

        private static double IntersectionOverUnion(InferencePrediction a, InferencePrediction b)
        {
            double left = Math.Max(a.X, b.X), top = Math.Max(a.Y, b.Y);
            double right = Math.Min(a.X + a.Width, b.X + b.Width), bottom = Math.Min(a.Y + a.Height, b.Y + b.Height);
            double intersection = Math.Max(0, right - left) * Math.Max(0, bottom - top);
            double union = a.Width * a.Height + b.Width * b.Height - intersection;
            return union <= 0 ? 0 : intersection / union;
        }

        internal static string[] ParseLabels(string value)
        {
            return (value ?? string.Empty).Split(new[] { ',', ';', '\r', '\n', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim()).Where(v => v.Length > 0).ToArray();
        }

        private static bool IsNormalLabel(string label)
        {
            return string.Equals(label, "normal", StringComparison.OrdinalIgnoreCase) || string.Equals(label, "ok", StringComparison.OrdinalIgnoreCase) || label == "正常" || label == "良品";
        }
    }
}
