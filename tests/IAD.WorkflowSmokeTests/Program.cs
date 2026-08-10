using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using IAD.Models;
using IAD.Services;

namespace IAD.WorkflowSmokeTests
{
    internal static class Program
    {
        private static int Main()
        {
            try
            {
                Run();
                Console.WriteLine("IAD dataset workflow smoke tests passed.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static void Run()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string workspace = Path.Combine(basePath, "Workspace");
            string input = Path.Combine(basePath, "TestInput");
            string exports = Path.Combine(basePath, "TestExports");
            RecreateDirectory(workspace);
            RecreateDirectory(input);
            RecreateDirectory(exports);

            string defectImagePath = Path.Combine(input, "defect.png");
            string normalImagePath = Path.Combine(input, "normal.png");
            CreateImage(defectImagePath, true);
            CreateImage(normalImagePath, false);

            AppServices.Initialize();
            VerifyMaskRefinement();
            Product product = AppServices.Products.CreateProduct("WF-001", "工作流测试产品", null);
            DefectCategory category = AppServices.Products.SaveDefectCategory(new DefectCategory
            {
                ProductId = product.Id,
                CategoryCode = "SCRATCH",
                CategoryName = "划痕",
                DefectType = "Surface",
                DetectionStrategy = "Manual",
                DefaultThreshold = 0.5D,
                MinArea = 1D,
                MinLength = 0D,
                DisplayOrder = 1,
                IsEnabled = true
            });

            DatasetImageImportResult firstImport = AppServices.Datasets.ImportImageChecked(product.Id, defectImagePath);
            DatasetImageImportResult duplicateImport = AppServices.Datasets.ImportImageChecked(product.Id, defectImagePath);
            DatasetImage normalImage = AppServices.Datasets.ImportImage(product.Id, normalImagePath);
            Assert(!firstImport.IsDuplicate, "首次图片导入不应判定为重复。");
            Assert(duplicateImport.IsDuplicate && duplicateImport.Image.Id == firstImport.Image.Id, "SHA-256 重复图片检测失败。");

            DatasetAnnotation annotation = AppServices.Datasets.CreateAnnotation(
                firstImport.Image.Id,
                category.Id,
                "Rectangle",
                AnnotationGeometry.Serialize(new[] { new PointF(8F, 8F), new PointF(30F, 28F) }),
                1F,
                1D);
            Assert(annotation.Id > 0, "矢量标注创建失败。");

            using (Bitmap mask = new Bitmap(64, 64))
            using (Graphics graphics = Graphics.FromImage(mask))
            {
                graphics.Clear(Color.Transparent);
                graphics.FillRectangle(Brushes.White, 10, 10, 12, 10);
                AppServices.Masks.SaveMask(firstImport.Image.Id, category.Id, mask);
            }

            DatasetImageQuality quality = AppServices.DatasetWorkflow.EvaluateImage(firstImport.Image.Id);
            Assert(quality.CanApprove && quality.VectorAnnotationCount == 1 && quality.MaskCount == 1, "真实质量检查未识别有效矢量标注和 Mask。");
            AppServices.DatasetWorkflow.SetReviewStatus(product.Id, new[] { firstImport.Image.Id }, DatasetReviewStatus.Approved, "测试通过", "smoke-test");
            AppServices.DatasetWorkflow.SetReviewStatus(product.Id, new[] { normalImage.Id }, DatasetReviewStatus.Normal, "确认正常", "smoke-test");
            AppServices.DatasetWorkflow.AssignSplits(product.Id, 50, 0, 42);

            DatasetQualityReport report = AppServices.DatasetWorkflow.EvaluateProduct(product.Id);
            Assert(report.ErrorCount == 0 && report.ImageCount == 2, "产品级发布质量门禁失败。");
            DatasetVersion version1 = AppServices.Datasets.CreateVersion(product.Id, "smoke v1");
            Assert(version1.ImageCount == 2 && version1.AnnotationCount == 1 && version1.MaskCount == 1, "数据集版本统计不正确。");

            DatasetExportResult exported = AppServices.DatasetWorkflow.ExportCurrent(product.Id, new DatasetExportOptions
            {
                DestinationDirectory = exports,
                ExportCoco = true,
                ExportYolo = true,
                ExportMasks = true,
                ApprovedOnly = true,
                RequireQualityGate = true
            });
            Assert(exported.ImageCount == 2 && exported.AnnotationCount == 1 && exported.MaskCount == 1, "训练数据导出统计不正确。");
            Assert(File.Exists(Path.Combine(exported.OutputDirectory, "annotations", "instances.json")), "COCO JSON 未生成。");
            Assert(File.Exists(Path.Combine(exported.OutputDirectory, "dataset.yaml")), "YOLO dataset.yaml 未生成。");
            Assert(File.Exists(Path.Combine(exported.OutputDirectory, "yolo-segmentation", "dataset.yaml")), "YOLO-Seg 数据集未生成。");
            Assert(Directory.GetFiles(Path.Combine(exported.OutputDirectory, "masks"), "*.png", SearchOption.AllDirectories).Length == 1, "Mask PNG 未导出。");

            AppServices.Datasets.CreateAnnotation(
                firstImport.Image.Id,
                category.Id,
                "Rectangle",
                AnnotationGeometry.Serialize(new[] { new PointF(35F, 35F), new PointF(48F, 48F) }),
                1F,
                1D);
            AppServices.DatasetWorkflow.SetReviewStatus(product.Id, new[] { firstImport.Image.Id }, DatasetReviewStatus.Approved, "增加标注", "smoke-test");
            DatasetVersion version2 = AppServices.Datasets.CreateVersion(product.Id, "smoke v2");
            DatasetVersionComparison comparison = AppServices.DatasetWorkflow.CompareVersions(product.Id, version1.Id, version2.Id);
            Assert(comparison.AddedAnnotations == 1, "版本比较未识别新增标注。");

            AppServices.Datasets.RestoreVersion(product.Id, version1.Id);
            Assert(AppServices.Datasets.GetAnnotations(firstImport.Image.Id).Count == 1, "历史版本恢复没有还原矢量标注。");
            Assert(AppServices.Masks.GetMasks(firstImport.Image.Id).Count == 1, "历史版本恢复没有还原 Mask。");

            Product cocoProduct = AppServices.Products.CreateProduct("WF-COCO", "COCO 回流测试", null);
            DatasetImportResult cocoImport = AppServices.DatasetWorkflow.ImportCoco(
                cocoProduct.Id,
                Path.Combine(exported.OutputDirectory, "annotations", "instances.json"));
            Assert(cocoImport.ImageCount == 2 && cocoImport.AnnotationCount == 1 && cocoImport.MaskCount == 1, "COCO/Mask 回流导入失败。");

            Product yoloProduct = AppServices.Products.CreateProduct("WF-YOLO", "YOLO 回流测试", null);
            DatasetImportResult yoloImport = AppServices.DatasetWorkflow.ImportYolo(yoloProduct.Id, exported.OutputDirectory);
            Assert(yoloImport.ImageCount == 2 && yoloImport.AnnotationCount == 1 && yoloImport.MaskCount == 1, "YOLO/Mask 回流导入失败。");

            Product yoloSegProduct = AppServices.Products.CreateProduct("WF-YOLOSEG", "YOLO-Seg 回流测试", null);
            DatasetImportResult yoloSegImport = AppServices.DatasetWorkflow.ImportYolo(
                yoloSegProduct.Id,
                Path.Combine(exported.OutputDirectory, "yolo-segmentation"));
            Assert(yoloSegImport.ImageCount == 2 && yoloSegImport.AnnotationCount == 1, "YOLO-Seg 回流导入失败。");

            DatasetImage imageWithDisposableMask = null;
            DatasetMask disposableMask = null;
            foreach (DatasetImage image in AppServices.Datasets.GetImages(yoloProduct.Id))
            {
                IList<DatasetMask> imageMasks = AppServices.Masks.GetMasks(image.Id);
                if (imageMasks.Count == 0) continue;
                imageWithDisposableMask = image;
                disposableMask = imageMasks[0];
                break;
            }
            Assert(imageWithDisposableMask != null && disposableMask != null, "没有找到用于删除清理测试的 Mask。");
            string disposableMaskPath = AppServices.Masks.GetMaskPath(disposableMask);
            Assert(File.Exists(disposableMaskPath), "删除前 Mask PNG 不存在。");
            AppServices.Datasets.DeleteImage(yoloProduct.Id, imageWithDisposableMask.Id);
            Assert(!File.Exists(disposableMaskPath), "删除未发布图片后没有立即清理 Mask PNG。");
        }

        private static void CreateImage(string path, bool defect)
        {
            using (Bitmap bitmap = new Bitmap(64, 64))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.FromArgb(28, 28, 28));
                if (defect) graphics.FillRectangle(Brushes.White, 8, 8, 22, 20);
                bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private static void VerifyMaskRefinement()
        {
            using (Bitmap source = new Bitmap(64, 64))
            using (Bitmap seed = new Bitmap(64, 64))
            using (Graphics sourceGraphics = Graphics.FromImage(source))
            using (Graphics seedGraphics = Graphics.FromImage(seed))
            {
                sourceGraphics.Clear(Color.FromArgb(28, 28, 28));
                sourceGraphics.FillRectangle(Brushes.White, 8, 8, 22, 20);
                seedGraphics.Clear(Color.Transparent);
                seedGraphics.FillRectangle(Brushes.White, 14, 14, 4, 4);

                using (Bitmap refined = AppServices.MaskRefinement.Refine(source, seed))
                {
                    int foreground = 0;
                    for (int y = 0; y < refined.Height; y++)
                        for (int x = 0; x < refined.Width; x++)
                            if (refined.GetPixel(x, y).A > 0) foreground++;
                    Assert(foreground > 16, "CPU Mask 精修没有从种子扩展到缺陷边缘。");
                }
            }
        }

        private static void RecreateDirectory(string path)
        {
            string fullPath = Path.GetFullPath(path);
            string basePath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("测试目录超出测试输出目录：" + fullPath);
            if (Directory.Exists(fullPath)) Directory.Delete(fullPath, true);
            Directory.CreateDirectory(fullPath);
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
