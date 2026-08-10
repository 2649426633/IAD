using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace IAD.Services
{
    public sealed class MaskRefinementService
    {
        public Bitmap Refine(Bitmap sourceImage, Bitmap seedMask)
        {
            if (sourceImage == null) throw new ArgumentNullException("sourceImage");
            if (seedMask == null) throw new ArgumentNullException("seedMask");
            if (sourceImage.Width != seedMask.Width || sourceImage.Height != seedMask.Height)
                throw new InvalidOperationException("原图与 Mask 尺寸必须一致。");

            int width = sourceImage.Width;
            int height = sourceImage.Height;
            byte[] source = ReadArgb(sourceImage);
            byte[] mask = ReadArgb(seedMask);
            bool[] seeds = new bool[width * height];
            int left = width;
            int top = height;
            int right = -1;
            int bottom = -1;
            long foregroundR = 0;
            long foregroundG = 0;
            long foregroundB = 0;
            int foregroundCount = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixel = y * width + x;
                    int offset = pixel * 4;
                    bool isForeground = mask[offset + 3] > 0 && (mask[offset] > 0 || mask[offset + 1] > 0 || mask[offset + 2] > 0);
                    if (!isForeground) continue;
                    seeds[pixel] = true;
                    left = Math.Min(left, x);
                    top = Math.Min(top, y);
                    right = Math.Max(right, x);
                    bottom = Math.Max(bottom, y);
                    foregroundB += source[offset];
                    foregroundG += source[offset + 1];
                    foregroundR += source[offset + 2];
                    foregroundCount++;
                }
            }
            if (foregroundCount == 0) throw new InvalidOperationException("当前 Mask 没有前景种子，请先涂抹少量缺陷区域。");

            int seedWidth = right - left + 1;
            int seedHeight = bottom - top + 1;
            int margin = Math.Max(8, Math.Max(seedWidth, seedHeight));
            int roiLeft = Math.Max(0, left - margin);
            int roiTop = Math.Max(0, top - margin);
            int roiRight = Math.Min(width - 1, right + margin);
            int roiBottom = Math.Min(height - 1, bottom + margin);

            double fgR = foregroundR / (double)foregroundCount;
            double fgG = foregroundG / (double)foregroundCount;
            double fgB = foregroundB / (double)foregroundCount;
            double bgR;
            double bgG;
            double bgB;
            SampleBackground(source, width, roiLeft, roiTop, roiRight, roiBottom, out bgR, out bgG, out bgB);

            bool[] candidates = new bool[width * height];
            for (int y = roiTop; y <= roiBottom; y++)
            {
                for (int x = roiLeft; x <= roiRight; x++)
                {
                    int pixel = y * width + x;
                    if (seeds[pixel]) { candidates[pixel] = true; continue; }
                    int offset = pixel * 4;
                    double b = source[offset];
                    double g = source[offset + 1];
                    double r = source[offset + 2];
                    double foregroundDistance = ColorDistance(r, g, b, fgR, fgG, fgB);
                    double backgroundDistance = ColorDistance(r, g, b, bgR, bgG, bgB);
                    candidates[pixel] = foregroundDistance <= backgroundDistance * 0.90D;
                }
            }

            candidates = SmoothCandidates(candidates, seeds, width, height, roiLeft, roiTop, roiRight, roiBottom);
            bool[] connected = KeepSeedConnected(candidates, seeds, width, height, roiLeft, roiTop, roiRight, roiBottom);
            return CreateMaskBitmap(connected, width, height);
        }

        private static void SampleBackground(
            byte[] source,
            int width,
            int left,
            int top,
            int right,
            int bottom,
            out double red,
            out double green,
            out double blue)
        {
            long sumR = 0;
            long sumG = 0;
            long sumB = 0;
            int count = 0;
            for (int y = top; y <= bottom; y++)
            {
                for (int x = left; x <= right; x++)
                {
                    if (x > left + 2 && x < right - 2 && y > top + 2 && y < bottom - 2) continue;
                    int offset = (y * width + x) * 4;
                    sumB += source[offset];
                    sumG += source[offset + 1];
                    sumR += source[offset + 2];
                    count++;
                }
            }
            if (count == 0) count = 1;
            red = sumR / (double)count;
            green = sumG / (double)count;
            blue = sumB / (double)count;
        }

        private static double ColorDistance(double r, double g, double b, double targetR, double targetG, double targetB)
        {
            double dr = r - targetR;
            double dg = g - targetG;
            double db = b - targetB;
            return dr * dr * 0.30D + dg * dg * 0.59D + db * db * 0.11D;
        }

        private static bool[] SmoothCandidates(
            bool[] source,
            bool[] seeds,
            int width,
            int height,
            int left,
            int top,
            int right,
            int bottom)
        {
            bool[] current = source;
            for (int pass = 0; pass < 2; pass++)
            {
                bool[] next = (bool[])current.Clone();
                for (int y = Math.Max(1, top); y <= Math.Min(height - 2, bottom); y++)
                {
                    for (int x = Math.Max(1, left); x <= Math.Min(width - 2, right); x++)
                    {
                        int pixel = y * width + x;
                        if (seeds[pixel]) { next[pixel] = true; continue; }
                        int neighbours = 0;
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                                if (current[(y + dy) * width + x + dx]) neighbours++;
                        next[pixel] = neighbours >= 5;
                    }
                }
                current = next;
            }
            return current;
        }

        private static bool[] KeepSeedConnected(
            bool[] candidates,
            bool[] seeds,
            int width,
            int height,
            int left,
            int top,
            int right,
            int bottom)
        {
            bool[] result = new bool[width * height];
            Queue<int> pending = new Queue<int>();
            for (int pixel = 0; pixel < seeds.Length; pixel++)
            {
                if (!seeds[pixel]) continue;
                result[pixel] = true;
                pending.Enqueue(pixel);
            }
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
            while (pending.Count > 0)
            {
                int pixel = pending.Dequeue();
                int x = pixel % width;
                int y = pixel / width;
                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];
                    if (nx < left || nx > right || ny < top || ny > bottom || nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    int neighbour = ny * width + nx;
                    if (result[neighbour] || !candidates[neighbour]) continue;
                    result[neighbour] = true;
                    pending.Enqueue(neighbour);
                }
            }
            return result;
        }

        private static byte[] ReadArgb(Bitmap source)
        {
            using (Bitmap converted = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics graphics = Graphics.FromImage(converted)) graphics.DrawImageUnscaled(source, 0, 0);
                Rectangle rectangle = new Rectangle(0, 0, converted.Width, converted.Height);
                BitmapData data = converted.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    byte[] packed = new byte[converted.Width * converted.Height * 4];
                    byte[] row = new byte[Math.Abs(data.Stride)];
                    for (int y = 0; y < converted.Height; y++)
                    {
                        IntPtr rowPointer = IntPtr.Add(data.Scan0, y * data.Stride);
                        Marshal.Copy(rowPointer, row, 0, row.Length);
                        Buffer.BlockCopy(row, 0, packed, y * converted.Width * 4, converted.Width * 4);
                    }
                    return packed;
                }
                finally
                {
                    converted.UnlockBits(data);
                }
            }
        }

        private static Bitmap CreateMaskBitmap(bool[] foreground, int width, int height)
        {
            Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Rectangle rectangle = new Rectangle(0, 0, width, height);
            BitmapData data = result.LockBits(rectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                byte[] row = new byte[Math.Abs(data.Stride)];
                for (int y = 0; y < height; y++)
                {
                    Array.Clear(row, 0, row.Length);
                    for (int x = 0; x < width; x++)
                    {
                        if (!foreground[y * width + x]) continue;
                        int offset = x * 4;
                        row[offset] = 255;
                        row[offset + 1] = 255;
                        row[offset + 2] = 255;
                        row[offset + 3] = 255;
                    }
                    Marshal.Copy(row, 0, IntPtr.Add(data.Scan0, y * data.Stride), row.Length);
                }
            }
            finally
            {
                result.UnlockBits(data);
            }
            return result;
        }
    }
}
