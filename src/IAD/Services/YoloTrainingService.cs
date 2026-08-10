using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using IAD.Infrastructure.Storage;
using IAD.Models;

namespace IAD.Services
{
    internal sealed class YoloTrainingService
    {
        private const string RunFileName = "run.json";
        private readonly ProductService products;
        private readonly DatasetWorkflowService workflow;
        private readonly InferenceModelService models;

        internal YoloTrainingService(ProductService products, DatasetWorkflowService workflow, InferenceModelService models)
        {
            this.products = products ?? throw new ArgumentNullException("products");
            this.workflow = workflow ?? throw new ArgumentNullException("workflow");
            this.models = models ?? throw new ArgumentNullException("models");
        }

        public string ResolvePythonExecutable()
        {
            string configured = Environment.GetEnvironmentVariable("IAD_YOLO_PYTHON");
            if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured)) return configured;

            string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] candidates =
            {
                Path.Combine(profile, "anaconda3", "envs", "pytorch", "python.exe"),
                Path.Combine(profile, "miniconda3", "envs", "pytorch", "python.exe"),
                Path.Combine(profile, "anaconda3", "python.exe"),
                Path.Combine(profile, "miniconda3", "python.exe")
            };
            foreach (string candidate in candidates) if (File.Exists(candidate)) return candidate;
            return "python";
        }

        public IList<YoloTrainingRun> GetRuns(long productId)
        {
            List<YoloTrainingRun> result = new List<YoloTrainingRun>();
            if (!Directory.Exists(ProjectStoragePaths.TrainingRunsPath)) return result;
            foreach (string path in Directory.GetFiles(ProjectStoragePaths.TrainingRunsPath, RunFileName, SearchOption.AllDirectories))
            {
                try
                {
                    YoloTrainingRun run = Deserialize<YoloTrainingRun>(File.ReadAllText(path, Encoding.UTF8));
                    if (run != null && run.ProductId == productId) result.Add(run);
                }
                catch { }
            }
            return result.OrderByDescending(item => item.CreatedAtUtc).ToList();
        }

        public async Task<YoloEnvironmentStatus> CheckEnvironmentAsync(IProgress<YoloTrainingProgress> progress, CancellationToken cancellationToken)
        {
            string resultPath = Path.Combine(ProjectStoragePaths.CachePath, "yolo_environment_" + Guid.NewGuid().ToString("N") + ".json");
            try
            {
                string python = ResolvePythonExecutable();
                Report(progress, null, "Environment", "正在检查 Python、Ultralytics 和训练设备……");
                int exitCode = await RunProcessAsync(python,
                    "-u " + Quote(WorkerScriptPath) + " --check --result " + Quote(resultPath),
                    ProjectStoragePaths.RootPath, line => Report(progress, null, "Environment", line), cancellationToken).ConfigureAwait(false);
                YoloEnvironmentStatus status = File.Exists(resultPath)
                    ? Deserialize<YoloEnvironmentStatus>(File.ReadAllText(resultPath, Encoding.UTF8))
                    : new YoloEnvironmentStatus { PythonExecutable = python, ErrorMessage = "训练环境没有返回检查结果。" };
                status.PythonExecutable = python;
                status.IsReady = exitCode == 0 && status.IsReady;
                if (!status.IsReady && string.IsNullOrWhiteSpace(status.ErrorMessage)) status.ErrorMessage = "YOLO 训练环境检查失败，退出码 " + exitCode + "。";
                return status;
            }
            catch (Exception ex)
            {
                return new YoloEnvironmentStatus { IsReady = false, PythonExecutable = ResolvePythonExecutable(), ErrorMessage = ex.Message };
            }
            finally
            {
                try { if (File.Exists(resultPath)) File.Delete(resultPath); } catch { }
            }
        }

        public async Task InstallEnvironmentAsync(IProgress<YoloTrainingProgress> progress, CancellationToken cancellationToken)
        {
            string python = ResolvePythonExecutable();
            Report(progress, null, "Installing", "开始安装 YOLO 训练依赖，这可能需要几分钟……");
            int exitCode = await RunProcessAsync(python,
                "-u -m pip install --disable-pip-version-check -r " + Quote(RequirementsPath),
                ProjectStoragePaths.RootPath, line => Report(progress, null, "Installing", line), cancellationToken).ConfigureAwait(false);
            if (exitCode != 0) throw new InvalidOperationException("训练依赖安装失败，退出码 " + exitCode + "。请查看实时日志。 ");
            Report(progress, null, "Ready", "YOLO 训练依赖安装完成。");
        }

        public async Task<YoloTrainingRun> TrainAsync(YoloTrainingRequest request, IProgress<YoloTrainingProgress> progress, CancellationToken cancellationToken)
        {
            ValidateRequest(request);
            Product product = products.GetProduct(request.ProductId);
            if (product == null) throw new InvalidOperationException("请先选择有效产品。");

            string runCode = "YOLO_" + MakeSafe(product.ProductCode) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            string runDirectory = Path.Combine(ProjectStoragePaths.TrainingRunsPath, runCode);
            Directory.CreateDirectory(runDirectory);
            YoloTrainingRun run = new YoloTrainingRun
            {
                RunCode = runCode,
                ProductId = request.ProductId,
                ProductCode = product.ProductCode,
                ModelVariant = request.ModelVariant,
                ImageSize = request.ImageSize,
                BatchSize = request.BatchSize,
                Epochs = request.Epochs,
                LearningRate = request.LearningRate,
                Device = NormalizeDevice(request.Device),
                Status = "Preparing",
                RunDirectory = runDirectory,
                CreatedAtUtc = DateTime.UtcNow
            };
            SaveRun(run);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                Report(progress, runCode, "Preparing", "正在执行数据质量门禁并导出 YOLO 数据集……");
                DatasetExportResult exported = await Task.Run(() => workflow.ExportCurrent(request.ProductId, new DatasetExportOptions
                {
                    DestinationDirectory = Path.Combine(runDirectory, "dataset"),
                    ExportYolo = true,
                    ExportCoco = false,
                    ExportMasks = false,
                    ApprovedOnly = true,
                    RequireQualityGate = true
                }), cancellationToken).ConfigureAwait(false);
                run.DatasetDirectory = exported.OutputDirectory;
                ValidateDataset(run.DatasetDirectory);
                string trainingYaml = CreateAbsoluteDatasetYaml(run.DatasetDirectory, runDirectory);

                YoloEnvironmentStatus environment = await CheckEnvironmentAsync(progress, cancellationToken).ConfigureAwait(false);
                if (!environment.IsReady)
                    throw new InvalidOperationException("YOLO 训练环境未就绪：" + environment.ErrorMessage + Environment.NewLine +
                        "请点击“安装环境”，或用 IAD_YOLO_PYTHON 环境变量指定可用的 Python。");

                run.Status = "Running";
                run.StartedAtUtc = DateTime.UtcNow;
                SaveRun(run);
                Report(progress, runCode, "Running", "开始训练 " + request.ModelVariant + "，设备：" + run.Device + "。");

                string workerResult = Path.Combine(runDirectory, "worker-result.json");
                string arguments = BuildTrainingArguments(request, run, trainingYaml, workerResult);
                int exitCode;
                using (StreamWriter log = new StreamWriter(Path.Combine(runDirectory, "train.log"), true, new UTF8Encoding(false)) { AutoFlush = true })
                {
                    object logSync = new object();
                    exitCode = await RunProcessAsync(environment.PythonExecutable, arguments, runDirectory,
                        line =>
                        {
                            lock (logSync) log.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "] " + line);
                            ReportWorkerLine(progress, runCode, request.Epochs, line);
                        }, cancellationToken).ConfigureAwait(false);
                }
                run.ExitCode = exitCode;
                if (exitCode != 0) throw new InvalidOperationException("YOLO 训练进程异常结束，退出码 " + exitCode + "。请查看训练日志。");
                if (!File.Exists(workerResult)) throw new InvalidOperationException("YOLO 训练完成但没有生成结果清单。");

                WorkerResult result = Deserialize<WorkerResult>(File.ReadAllText(workerResult, Encoding.UTF8));
                if (result == null || !result.Success) throw new InvalidOperationException(result == null ? "训练结果无效。" : result.ErrorMessage);
                if (!File.Exists(result.OnnxPath)) throw new FileNotFoundException("训练完成但没有找到导出的 ONNX 模型。", result.OnnxPath);

                run.BestWeightsPath = result.BestWeightsPath;
                run.OnnxPath = result.OnnxPath;
                run.Precision = result.Precision;
                run.Recall = result.Recall;
                run.Map50 = result.Map50;
                run.Map5095 = result.Map5095;
                run.F1 = result.Precision + result.Recall <= 0 ? 0 : 2D * result.Precision * result.Recall / (result.Precision + result.Recall);
                run.InferenceMilliseconds = result.InferenceMilliseconds;

                string labels = ResolveProductLabels(request.ProductId, result.Labels);
                InferenceModel imported = await Task.Run(() => models.Import(result.OnnxPath, new InferenceModel
                {
                    ProductId = request.ProductId,
                    ModelCode = runCode,
                    ModelName = request.ModelVariant + " 自动训练模型",
                    Version = DateTime.Now.ToString("yyyy.MM.dd-HHmmss", CultureInfo.InvariantCulture),
                    ModelType = "Yolo26",
                    InputWidth = request.ImageSize,
                    InputHeight = request.ImageSize,
                    Labels = labels,
                    ConfidenceThreshold = 0.25D,
                    NmsThreshold = 0.45D,
                    IsActive = true
                }), cancellationToken).ConfigureAwait(false);
                run.ModelId = imported.Id;
                run.Status = "Completed";
                run.CompletedAtUtc = DateTime.UtcNow;
                SaveRun(run);
                Report(progress, runCode, "Completed", "训练完成，ONNX 模型已自动校验、入库并设为启用。");
                return run;
            }
            catch (OperationCanceledException)
            {
                run.Status = "Cancelled";
                run.CompletedAtUtc = DateTime.UtcNow;
                run.ErrorMessage = "用户停止训练。";
                SaveRun(run);
                Report(progress, runCode, "Cancelled", run.ErrorMessage);
                throw;
            }
            catch (Exception ex)
            {
                run.Status = "Failed";
                run.CompletedAtUtc = DateTime.UtcNow;
                run.ErrorMessage = ex.Message;
                SaveRun(run);
                Report(progress, runCode, "Failed", ex.Message);
                throw;
            }
        }

        private string ResolveProductLabels(long productId, string[] workerLabels)
        {
            IList<DefectCategory> categories = products.GetDefectCategories(productId);
            List<string> labels = new List<string>();
            foreach (string source in workerLabels ?? new string[0])
            {
                DefectCategory category = categories.FirstOrDefault(item =>
                    string.Equals(item.CategoryCode, source, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item.CategoryName, source, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item.CategoryCode + "_" + item.CategoryName, source, StringComparison.OrdinalIgnoreCase));
                labels.Add(category == null ? source : category.CategoryCode);
            }
            if (labels.Count == 0) labels.AddRange(categories.Where(item => item.IsEnabled).OrderBy(item => item.DisplayOrder).Select(item => item.CategoryCode));
            return string.Join(",", labels);
        }

        private static string BuildTrainingArguments(YoloTrainingRequest request, YoloTrainingRun run, string yamlPath, string resultPath)
        {
            StringBuilder value = new StringBuilder();
            value.Append("-u ").Append(Quote(WorkerScriptPath));
            value.Append(" --model ").Append(Quote(request.ModelVariant));
            value.Append(" --data ").Append(Quote(yamlPath));
            value.Append(" --epochs ").Append(request.Epochs.ToString(CultureInfo.InvariantCulture));
            value.Append(" --imgsz ").Append(request.ImageSize.ToString(CultureInfo.InvariantCulture));
            value.Append(" --batch ").Append(request.BatchSize.ToString(CultureInfo.InvariantCulture));
            value.Append(" --lr0 ").Append(request.LearningRate.ToString("0.########", CultureInfo.InvariantCulture));
            value.Append(" --device ").Append(Quote(run.Device));
            value.Append(" --seed ").Append(request.Seed.ToString(CultureInfo.InvariantCulture));
            value.Append(" --project ").Append(Quote(Path.Combine(run.RunDirectory, "output")));
            value.Append(" --weights-dir ").Append(Quote(Path.Combine(ProjectStoragePaths.CachePath, "YoloWeights")));
            value.Append(" --name train --result ").Append(Quote(resultPath));
            return value.ToString();
        }

        private static void ValidateRequest(YoloTrainingRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (request.ProductId <= 0) throw new ArgumentException("请先选择产品。");
            if (string.IsNullOrWhiteSpace(request.ModelVariant) || !Regex.IsMatch(request.ModelVariant, @"^yolo26[nslmx]\.pt$", RegexOptions.IgnoreCase))
                throw new ArgumentException("请选择受支持的 YOLO26 预训练模型。");
            if (request.ImageSize < 320 || request.ImageSize > 2048 || request.ImageSize % 32 != 0) throw new ArgumentException("输入尺寸必须是 320–2048 之间且为 32 的倍数。");
            if (request.BatchSize < 1 || request.BatchSize > 128) throw new ArgumentException("Batch Size 必须在 1–128 之间。");
            if (request.Epochs < 1 || request.Epochs > 2000) throw new ArgumentException("Epoch 必须在 1–2000 之间。");
            if (request.LearningRate <= 0 || request.LearningRate > 1 || double.IsNaN(request.LearningRate)) throw new ArgumentException("学习率必须在 0–1 之间。");
        }

        private static void ValidateDataset(string datasetDirectory)
        {
            string train = Path.Combine(datasetDirectory, "images", "train");
            string validation = Path.Combine(datasetDirectory, "images", "val");
            int trainCount = CountImages(train);
            int validationCount = CountImages(validation);
            if (trainCount == 0) throw new InvalidOperationException("训练集为空。请在“数据集管理”中完成划分。");
            if (validationCount == 0) throw new InvalidOperationException("验证集为空。请在“数据集管理”中至少划分 1 张验证图片。");
            if (CountPositiveLabels(Path.Combine(datasetDirectory, "labels", "train")) == 0)
                throw new InvalidOperationException("训练集中没有缺陷标注。请至少加入 1 张已审核的缺陷图片。");
            if (CountPositiveLabels(Path.Combine(datasetDirectory, "labels", "val")) == 0)
                throw new InvalidOperationException("验证集中没有缺陷标注，无法计算 mAP。请至少加入 1 张已审核的缺陷图片。");
        }

        private static int CountImages(string directory)
        {
            if (!Directory.Exists(directory)) return 0;
            HashSet<string> extensions = new HashSet<string>(new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff" }, StringComparer.OrdinalIgnoreCase);
            return Directory.GetFiles(directory).Count(path => extensions.Contains(Path.GetExtension(path)));
        }

        private static int CountPositiveLabels(string directory)
        {
            if (!Directory.Exists(directory)) return 0;
            int count = 0;
            foreach (string path in Directory.GetFiles(directory, "*.txt"))
            {
                try { if (File.ReadAllLines(path, Encoding.UTF8).Any(line => !string.IsNullOrWhiteSpace(line))) count++; }
                catch { }
            }
            return count;
        }

        private static string CreateAbsoluteDatasetYaml(string datasetDirectory, string runDirectory)
        {
            string source = Path.Combine(datasetDirectory, "dataset.yaml");
            if (!File.Exists(source)) throw new FileNotFoundException("YOLO dataset.yaml 未生成。", source);
            string[] lines = File.ReadAllLines(source, Encoding.UTF8);
            string root = datasetDirectory.Replace("\\", "/").Replace("'", "''");
            bool replaced = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].TrimStart().StartsWith("path:", StringComparison.OrdinalIgnoreCase)) continue;
                lines[i] = "path: '" + root + "'";
                replaced = true;
                break;
            }
            if (!replaced) lines = new[] { "path: '" + root + "'" }.Concat(lines).ToArray();
            string destination = Path.Combine(runDirectory, "training-dataset.yaml");
            File.WriteAllLines(destination, lines, new UTF8Encoding(false));
            return destination;
        }

        private static string NormalizeDevice(string value)
        {
            if (string.Equals(value, "GPU", StringComparison.OrdinalIgnoreCase) || value == "0") return "0";
            if (string.Equals(value, "CPU", StringComparison.OrdinalIgnoreCase)) return "cpu";
            return string.IsNullOrWhiteSpace(value) ? "auto" : value.Trim().ToLowerInvariant();
        }

        private static void ReportWorkerLine(IProgress<YoloTrainingProgress> progress, string runCode, int totalEpochs, string line)
        {
            int epoch;
            Match match = Regex.Match(line ?? string.Empty, @"^\s*(\d+)\s*/\s*(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out epoch))
                Report(progress, runCode, "Running", line, epoch, totalEpochs);
            else Report(progress, runCode, "Running", line);
        }

        private static async Task<int> RunProcessAsync(string fileName, string arguments, string workingDirectory, Action<string> output, CancellationToken cancellationToken)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            startInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
            startInfo.EnvironmentVariables["PYTHONUNBUFFERED"] = "1";
            startInfo.EnvironmentVariables["YOLO_CONFIG_DIR"] = Path.Combine(ProjectStoragePaths.CachePath, "Ultralytics");

            using (Process process = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
            {
                TaskCompletionSource<int> completion = new TaskCompletionSource<int>();
                process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e) { if (!string.IsNullOrWhiteSpace(e.Data)) output(e.Data); };
                process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e) { if (!string.IsNullOrWhiteSpace(e.Data)) output(e.Data); };
                process.Exited += delegate { try { completion.TrySetResult(process.ExitCode); } catch (Exception ex) { completion.TrySetException(ex); } };
                try
                {
                    if (!process.Start()) throw new InvalidOperationException("无法启动 Python 训练进程。");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("无法启动 Python：" + fileName + "。" + ex.Message, ex);
                }
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                using (cancellationToken.Register(delegate
                {
                    try { if (!process.HasExited) process.Kill(); } catch { }
                    completion.TrySetCanceled();
                }))
                {
                    int exitCode = await completion.Task.ConfigureAwait(false);
                    process.WaitForExit();
                    return exitCode;
                }
            }
        }

        private static void SaveRun(YoloTrainingRun run)
        {
            Directory.CreateDirectory(run.RunDirectory);
            string path = Path.Combine(run.RunDirectory, RunFileName);
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, Serialize(run), new UTF8Encoding(false));
            File.Copy(temporary, path, true);
            File.Delete(temporary);
        }

        private static string Serialize(object value) { return new JavaScriptSerializer { MaxJsonLength = int.MaxValue }.Serialize(value); }
        private static T Deserialize<T>(string value) { return new JavaScriptSerializer { MaxJsonLength = int.MaxValue }.Deserialize<T>(value); }
        private static string Quote(string value) { return "\"" + (value ?? string.Empty).Replace("\"", "\\\"") + "\""; }
        private static string MakeSafe(string value)
        {
            string result = string.IsNullOrWhiteSpace(value) ? "PRODUCT" : value.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars()) result = result.Replace(invalid, '_');
            return result.Replace(' ', '_');
        }

        private static void Report(IProgress<YoloTrainingProgress> progress, string runCode, string status, string message, int? epoch = null, int? totalEpochs = null)
        {
            if (progress == null || string.IsNullOrWhiteSpace(message)) return;
            progress.Report(new YoloTrainingProgress { RunCode = runCode, Status = status, Message = message, Epoch = epoch, TotalEpochs = totalEpochs });
        }

        private static string WorkerScriptPath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Training", "yolo_worker.py"); } }
        private static string RequirementsPath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Training", "requirements-yolo.txt"); } }

        private sealed class WorkerResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string BestWeightsPath { get; set; }
            public string OnnxPath { get; set; }
            public string[] Labels { get; set; }
            public double Precision { get; set; }
            public double Recall { get; set; }
            public double Map50 { get; set; }
            public double Map5095 { get; set; }
            public double InferenceMilliseconds { get; set; }
        }
    }
}
