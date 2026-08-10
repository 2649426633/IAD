using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using IAD.Infrastructure.Storage;
using IAD.Models;
using IAD.Repositories;

namespace IAD.Services
{
    public sealed class DatasetMaskService
    {
        private readonly IDatasetRepository datasets;
        private readonly IDefectCategoryRepository categories;
        private readonly IDatasetMaskRepository masks;

        internal DatasetMaskService(
            IDatasetRepository datasets,
            IDefectCategoryRepository categories,
            IDatasetMaskRepository masks)
        {
            this.datasets = datasets ?? throw new ArgumentNullException("datasets");
            this.categories = categories ?? throw new ArgumentNullException("categories");
            this.masks = masks ?? throw new ArgumentNullException("masks");
        }

        public IList<DatasetMask> GetMasks(long imageId)
        {
            EnsureImage(imageId);
            return masks.GetByImage(imageId);
        }

        public DatasetMask GetMask(long imageId, long categoryId)
        {
            DatasetImage image = EnsureImage(imageId);
            EnsureCategory(image, categoryId);
            return masks.GetByImageAndCategory(imageId, categoryId);
        }

        public Bitmap LoadEditableBitmap(DatasetMask mask)
        {
            if (mask == null) throw new ArgumentNullException("mask");
            string path = ResolveMaskPath(mask.RelativePath);
            if (!File.Exists(path))
                throw new FileNotFoundException("Mask PNG 文件不存在。", path);

            using (Bitmap source = new Bitmap(path))
            {
                if (source.Width != mask.Width || source.Height != mask.Height)
                    throw new InvalidDataException("Mask PNG 尺寸与数据库记录不一致。");
                return ToEditableBitmap(source);
            }
        }

        public string GetMaskPath(DatasetMask mask)
        {
            if (mask == null) throw new ArgumentNullException("mask");
            return ResolveMaskPath(mask.RelativePath);
        }

        public DatasetMask SaveMask(long imageId, long categoryId, Bitmap editableBitmap)
        {
            if (editableBitmap == null) throw new ArgumentNullException("editableBitmap");
            DatasetImage image = EnsureImage(imageId);
            DefectCategory category = EnsureCategory(image, categoryId);
            if (editableBitmap.Width != image.Width || editableBitmap.Height != image.Height)
                throw new InvalidOperationException(
                    "Mask 尺寸必须与原图完全一致。原图=" + image.Width + "×" + image.Height +
                    "，Mask=" + editableBitmap.Width + "×" + editableBitmap.Height + "。");

            DatasetMask current = masks.GetByImageAndCategory(imageId, categoryId);
            int revision = current == null ? 1 : checked(current.Revision + 1);
            string relativePath = BuildRelativePath(imageId, categoryId, revision);
            string targetPath = ResolveMaskPath(relativePath);
            string directory = Path.GetDirectoryName(targetPath);
            if (string.IsNullOrWhiteSpace(directory)) throw new InvalidOperationException("Mask 存储目录无效。");
            Directory.CreateDirectory(directory);

            string tempPath = targetPath + ".tmp-" + Guid.NewGuid().ToString("N");
            long pixelCount = SaveBinaryPng(editableBitmap, tempPath);

            if (pixelCount == 0)
            {
                TryDeleteFile(tempPath);
                if (current != null) DeleteMask(imageId, categoryId);
                else RefreshImageStatus(imageId);
                return null;
            }

            File.Move(tempPath, targetPath);
            DateTime now = DateTime.UtcNow;
            string previousPath = current == null ? null : current.RelativePath;
            DatasetMask result = current ?? new DatasetMask
            {
                DatasetImageId = imageId,
                CreatedAtUtc = now,
                IsVisible = true
            };
            result.CategoryId = category.Id;
            result.CategoryCode = category.CategoryCode;
            result.CategoryName = category.CategoryName;
            result.RelativePath = relativePath;
            result.Width = image.Width;
            result.Height = image.Height;
            result.Revision = revision;
            result.PixelCount = pixelCount;
            result.UpdatedAtUtc = now;

            try
            {
                if (result.Id <= 0)
                    result.Id = masks.Insert(result);
                else
                    masks.Update(result);
                datasets.UpdateImageStatus(image.Id, "已标注", now);
            }
            catch
            {
                TryDeleteFile(targetPath);
                throw;
            }

            DeleteUnreferencedPreviousFile(previousPath, relativePath);
            return result;
        }

        public void DeleteMask(long imageId, long categoryId)
        {
            DatasetImage image = EnsureImage(imageId);
            EnsureCategory(image, categoryId);
            DatasetMask current = masks.GetByImageAndCategory(imageId, categoryId);
            if (current == null)
            {
                RefreshImageStatus(imageId);
                return;
            }

            masks.Delete(current.Id, imageId);
            if (!masks.IsRelativePathReferencedByVersion(current.RelativePath))
                TryDeleteFile(ResolveMaskPath(current.RelativePath));
            RefreshImageStatus(imageId);
        }

        public DatasetMask RasterizeAnnotations(long imageId, long categoryId)
        {
            DatasetImage image = EnsureImage(imageId);
            EnsureCategory(image, categoryId);
            IList<DatasetAnnotation> annotations = datasets.GetAnnotationsByImage(imageId);
            List<DatasetAnnotation> selected = new List<DatasetAnnotation>();
            foreach (DatasetAnnotation annotation in annotations)
            {
                if (annotation.CategoryId.HasValue && annotation.CategoryId.Value == categoryId)
                    selected.Add(annotation);
            }

            if (selected.Count == 0)
                throw new InvalidOperationException("当前图片在所选类别下没有可栅格化的 Rectangle / Polygon / Brush 标注。");

            using (Bitmap bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.PixelOffsetMode = PixelOffsetMode.None;
                graphics.CompositingMode = CompositingMode.SourceCopy;

                foreach (DatasetAnnotation annotation in selected)
                    RasterizeAnnotation(graphics, annotation);

                return SaveMask(imageId, categoryId, bitmap);
            }
        }

        public void CleanupOrphanFiles()
        {
            ProjectStoragePaths.EnsureCreated();
            if (!Directory.Exists(ProjectStoragePaths.MasksPath)) return;

            HashSet<string> referenced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string relativePath in masks.GetAllReferencedRelativePaths())
            {
                try { referenced.Add(ResolveMaskPath(relativePath)); }
                catch { }
            }

            foreach (string path in Directory.GetFiles(ProjectStoragePaths.MasksPath, "*.png", SearchOption.AllDirectories))
            {
                string fullPath = Path.GetFullPath(path);
                if (!referenced.Contains(fullPath)) TryDeleteFile(fullPath);
            }

            foreach (string path in Directory.GetFiles(ProjectStoragePaths.MasksPath, "*.tmp-*", SearchOption.AllDirectories))
                TryDeleteFile(path);
        }

        private void RefreshImageStatus(long imageId)
        {
            bool hasVectorAnnotation = datasets.GetAnnotationsByImage(imageId).Count > 0;
            bool hasMask = masks.GetByImage(imageId).Count > 0;
            datasets.UpdateImageStatus(imageId, hasVectorAnnotation || hasMask ? "已标注" : "未标注", DateTime.UtcNow);
        }

        private DatasetImage EnsureImage(long imageId)
        {
            DatasetImage image = imageId > 0 ? datasets.GetImageById(imageId) : null;
            if (image == null) throw new InvalidOperationException("数据集图片不存在。Id=" + imageId);
            return image;
        }

        private DefectCategory EnsureCategory(DatasetImage image, long categoryId)
        {
            DefectCategory category = categoryId > 0 ? categories.GetById(categoryId) : null;
            if (category == null || category.ProductId != image.ProductId || !category.IsEnabled)
                throw new InvalidOperationException("当前 Mask 类别不存在、未启用或不属于所选产品。");
            return category;
        }

        private static string BuildRelativePath(long imageId, long categoryId, int revision)
        {
            string fileName = "mask_r" + revision.ToString("D5") + "_" +
                              DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + ".png";
            return Path.Combine("Masks", "Image_" + imageId, "Category_" + categoryId, fileName);
        }

        private static string ResolveMaskPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new InvalidOperationException("Mask 路径为空。");
            string root = Path.GetFullPath(ProjectStoragePaths.MasksPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string fullPath = Path.GetFullPath(Path.Combine(ProjectStoragePaths.RootPath, relativePath));
            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Mask 路径超出 Workspace/Masks 目录。");
            return fullPath;
        }

        private void DeleteUnreferencedPreviousFile(string previousRelativePath, string newRelativePath)
        {
            if (string.IsNullOrWhiteSpace(previousRelativePath) ||
                string.Equals(previousRelativePath, newRelativePath, StringComparison.OrdinalIgnoreCase)) return;
            if (masks.IsRelativePathReferencedByVersion(previousRelativePath)) return;
            TryDeleteFile(ResolveMaskPath(previousRelativePath));
        }

        private static void RasterizeAnnotation(Graphics graphics, DatasetAnnotation annotation)
        {
            List<PointF> points = AnnotationGeometry.Parse(annotation.GeometryData);
            if (string.Equals(annotation.AnnotationType, "Rectangle", StringComparison.OrdinalIgnoreCase) && points.Count == 2)
            {
                float left = Math.Min(points[0].X, points[1].X);
                float top = Math.Min(points[0].Y, points[1].Y);
                float width = Math.Abs(points[1].X - points[0].X);
                float height = Math.Abs(points[1].Y - points[0].Y);
                graphics.FillRectangle(Brushes.White, left, top, width, height);
                return;
            }

            if (string.Equals(annotation.AnnotationType, "Polygon", StringComparison.OrdinalIgnoreCase) && points.Count >= 3)
            {
                graphics.FillPolygon(Brushes.White, points.ToArray());
                return;
            }

            if (string.Equals(annotation.AnnotationType, "Brush", StringComparison.OrdinalIgnoreCase) && points.Count >= 2)
            {
                using (Pen pen = new Pen(Color.White, Math.Max(1F, annotation.BrushWidth)))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    pen.LineJoin = LineJoin.Round;
                    graphics.DrawLines(pen, points.ToArray());
                }
            }
        }

        private static Bitmap ToEditableBitmap(Bitmap source)
        {
            Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(result))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImageUnscaled(source, 0, 0);
            }

            Rectangle rectangle = new Rectangle(0, 0, result.Width, result.Height);
            BitmapData data = result.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                int bytes = Math.Abs(data.Stride) * result.Height;
                byte[] buffer = new byte[bytes];
                Marshal.Copy(data.Scan0, buffer, 0, bytes);
                for (int y = 0; y < result.Height; y++)
                {
                    int row = y * Math.Abs(data.Stride);
                    for (int x = 0; x < result.Width; x++)
                    {
                        int index = row + x * 4;
                        int brightness = Math.Max(buffer[index], Math.Max(buffer[index + 1], buffer[index + 2]));
                        bool foreground = brightness >= 128;
                        buffer[index] = 255;
                        buffer[index + 1] = 255;
                        buffer[index + 2] = 255;
                        buffer[index + 3] = foreground ? (byte)255 : (byte)0;
                    }
                }
                Marshal.Copy(buffer, 0, data.Scan0, bytes);
            }
            finally
            {
                result.UnlockBits(data);
            }
            return result;
        }

        private static long SaveBinaryPng(Bitmap source, string path)
        {
            Bitmap editable = source.PixelFormat == PixelFormat.Format32bppArgb
                ? new Bitmap(source)
                : ToEditableBitmap(source);
            try
            {
                using (Bitmap binary = new Bitmap(editable.Width, editable.Height, PixelFormat.Format8bppIndexed))
                {
                    ColorPalette palette = binary.Palette;
                    for (int i = 0; i < palette.Entries.Length; i++)
                        palette.Entries[i] = Color.FromArgb(i, i, i);
                    binary.Palette = palette;

                    Rectangle rectangle = new Rectangle(0, 0, editable.Width, editable.Height);
                    BitmapData sourceData = editable.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    BitmapData targetData = binary.LockBits(rectangle, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                    long pixelCount = 0;
                    try
                    {
                        int sourceStride = Math.Abs(sourceData.Stride);
                        int targetStride = Math.Abs(targetData.Stride);
                        byte[] sourceBytes = new byte[sourceStride * editable.Height];
                        byte[] targetBytes = new byte[targetStride * editable.Height];
                        Marshal.Copy(sourceData.Scan0, sourceBytes, 0, sourceBytes.Length);

                        for (int y = 0; y < editable.Height; y++)
                        {
                            int sourceRow = y * sourceStride;
                            int targetRow = y * targetStride;
                            for (int x = 0; x < editable.Width; x++)
                            {
                                int sourceIndex = sourceRow + x * 4;
                                bool foreground = sourceBytes[sourceIndex + 3] >= 128;
                                targetBytes[targetRow + x] = foreground ? (byte)255 : (byte)0;
                                if (foreground) pixelCount++;
                            }
                        }
                        Marshal.Copy(targetBytes, 0, targetData.Scan0, targetBytes.Length);
                    }
                    finally
                    {
                        editable.UnlockBits(sourceData);
                        binary.UnlockBits(targetData);
                    }

                    binary.Save(path, ImageFormat.Png);
                    return pixelCount;
                }
            }
            finally
            {
                editable.Dispose();
            }
        }

        private static void TryDeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // 运行时清理失败不影响当前标注数据；后续 CleanupOrphanFiles 会再次尝试。
            }
        }
    }
}
