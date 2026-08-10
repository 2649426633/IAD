using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IAD.Models;
using IAD.Security;
using IAD.Services;
using IAD.Shell;

namespace IAD.Pages
{
    public partial class TrainingModelsPage : UserControl
    {
        private bool runtimeInitialized;
        private Button btnImportModel;
        private Button btnActivateModel;
        private Button btnDeleteModel;
        private Button btnRefreshModels;
        private Button btnCheckEnvironment;
        private Button btnInstallEnvironment;
        private Button btnStartTraining;
        private Button btnCancelTraining;
        private Button btnOpenRunFolder;
        private CancellationTokenSource operationCancellation;
        private bool operationInProgress;
        private long loadedProductId;
        private long loadedRevision = -1;

        public TrainingModelsPage() { InitializeComponent(); }

        public void InitializeRuntime()
        {
            if (runtimeInitialized) return;
            runtimeInitialized = true;
            ConfigureTrainingControls();
            BuildTrainingToolbar();
            BuildModelToolbar();
            ConfigureGrids();
            BindEvents();
            ResetMetrics();
            RefreshAll();
        }

        private void ConfigureTrainingControls()
        {
            cboArchitecture.Items.Clear();
            cboArchitecture.Items.AddRange(new object[] { "yolo26n.pt", "yolo26s.pt", "yolo26m.pt", "yolo26l.pt" });
            cboArchitecture.SelectedIndex = 0;
            cboTileSize.Items.Clear();
            cboTileSize.Items.AddRange(new object[] { "640 × 640", "1024 × 1024", "1280 × 1280" });
            cboTileSize.SelectedIndex = 0;
            numBatchSize.Minimum = 1;
            numBatchSize.Maximum = 128;
            numBatchSize.Value = 8;
            numEpoch.Maximum = 2000;
            numEpoch.Value = 100;
            txtLearningRate.Text = "0.01";
            txtAugmentation.Text = "YOLO 默认工业增强（翻转 / 缩放 / 颜色扰动）";
            txtAugmentation.ReadOnly = true;
            cboDevice.Items.Clear();
            cboDevice.Items.AddRange(new object[] { "自动选择", "GPU", "CPU" });
            cboDevice.SelectedIndex = 0;
            lblValMiouKey.Text = "mAP50-95";
            lblAcceptanceKey.Text = "训练结果";
            grpBenchmark.Text = "YOLO 模型基准（验证集）";
        }

        private void BuildTrainingToolbar()
        {
            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(4, 3, 4, 3),
                BackColor = Color.WhiteSmoke,
                WrapContents = false
            };
            btnStartTraining = CreateButton("开始训练", 88);
            btnCancelTraining = CreateButton("停止", 68);
            btnCheckEnvironment = CreateButton("检查环境", 88);
            btnInstallEnvironment = CreateButton("安装环境", 88);
            btnOpenRunFolder = CreateButton("打开目录", 88);
            btnCancelTraining.Enabled = false;
            actions.Controls.AddRange(new Control[] { btnStartTraining, btnCancelTraining, btnCheckEnvironment, btnInstallEnvironment, btnOpenRunFolder });
            grpTrainingQueue.Controls.Add(actions);
            actions.BringToFront();
        }

        private void BuildModelToolbar()
        {
            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 38,
                Padding = new Padding(4, 3, 4, 3),
                BackColor = Color.WhiteSmoke,
                WrapContents = false
            };
            btnImportModel = CreateButton("导入 ONNX", 100);
            btnActivateModel = CreateButton("设为启用", 90);
            btnDeleteModel = CreateButton("删除模型", 90);
            btnRefreshModels = CreateButton("刷新", 72);
            actions.Controls.AddRange(new Control[] { btnImportModel, btnActivateModel, btnDeleteModel, btnRefreshModels });
            grpModelLibrary.Controls.Add(actions);
            actions.BringToFront();
        }

        private static Button CreateButton(string text, int width)
        {
            return new Button { Text = text, Width = width, Height = 29, FlatStyle = FlatStyle.Flat, BackColor = Color.White };
        }

        private void ConfigureGrids()
        {
            ConfigureReadOnlyGrid(dgvModelLibrary);
            ConfigureReadOnlyGrid(dgvTrainingQueue);
            ConfigureReadOnlyGrid(dgvDatasetSplit);
            ConfigureReadOnlyGrid(dgvBenchmark);
            dgvModelLibraryCol1.HeaderText = "版本";
            dgvModelLibraryCol2.HeaderText = "输出格式";
            dgvModelLibraryCol3.HeaderText = "输入尺寸";
            dgvModelLibraryCol4.HeaderText = "创建时间";
            dgvModelLibraryCol5.HeaderText = "置信度";
            dgvModelLibraryCol6.HeaderText = "状态";
            dgvModelLibraryCol7.HeaderText = "SHA256";
            dgvModelLibraryCol8.HeaderText = "类别顺序";
            dgvBenchmarkCol1.HeaderText = "训练任务";
            dgvBenchmarkCol2.HeaderText = "Recall";
            dgvBenchmarkCol3.HeaderText = "Precision";
            dgvBenchmarkCol4.HeaderText = "F1";
            dgvBenchmarkCol5.HeaderText = "mAP50-95";
            dgvBenchmarkCol6.HeaderText = "mAP50";
            dgvBenchmarkCol7.HeaderText = "推理 ms";
            dgvBenchmarkCol8.HeaderText = "模型";
        }

        private static void ConfigureReadOnlyGrid(DataGridView grid)
        {
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
        }

        private void BindEvents()
        {
            btnImportModel.Click += delegate { ImportModel(); };
            btnActivateModel.Click += delegate { ActivateSelected(); };
            btnDeleteModel.Click += delegate { DeleteSelected(); };
            btnRefreshModels.Click += delegate { RefreshAll(); };
            btnCheckEnvironment.Click += async delegate { await CheckEnvironmentAsync(); };
            btnInstallEnvironment.Click += async delegate { await InstallEnvironmentAsync(); };
            btnStartTraining.Click += async delegate { await StartTrainingAsync(); };
            btnCancelTraining.Click += delegate { CancelOperation(); };
            btnOpenRunFolder.Click += delegate { OpenSelectedRunFolder(); };
            dgvTrainingQueue.SelectionChanged += delegate { ShowSelectedRunMetrics(); };
            AppSession.CurrentProductChanged += delegate
            {
                if (!IsDisposed)
                {
                    loadedProductId = 0;
                    if (IsHandleCreated) BeginInvoke(new MethodInvoker(RefreshAll));
                }
            };
            VisibleChanged += delegate
            {
                if (Visible && (loadedProductId != AppSession.CurrentProductId ||
                    loadedRevision != ProductDataRevisionTracker.GetRevision(AppSession.CurrentProductId))) RefreshAll();
            };
            Disposed += delegate { CancelOperation(); };
        }

        private IProgress<YoloTrainingProgress> CreateProgress()
        {
            return new Progress<YoloTrainingProgress>(item =>
            {
                AppendLog(item.Message);
                if (item.Epoch.HasValue && item.TotalEpochs.HasValue)
                    lblAcceptance.Text = "训练中 " + item.Epoch.Value + "/" + item.TotalEpochs.Value;
                else if (!string.IsNullOrWhiteSpace(item.Status)) lblAcceptance.Text = DisplayRunStatus(item.Status);
            });
        }

        private async Task CheckEnvironmentAsync()
        {
            if (operationInProgress) return;
            BeginOperation();
            AppendLog("正在检查 YOLO 训练环境……");
            try
            {
                YoloEnvironmentStatus status = await AppServices.YoloTraining.CheckEnvironmentAsync(CreateProgress(), operationCancellation.Token);
                string detail = "Python " + (status.PythonVersion ?? "-") + " | Ultralytics " + (status.UltralyticsVersion ?? "-") +
                    " | Torch " + (status.TorchVersion ?? "-") + " | " + (status.DeviceName ?? "CPU");
                AppendLog(detail);
                MessageBox.Show(this, status.IsReady ? "训练环境已就绪。\r\n" + detail : "训练环境尚未就绪。\r\n" + status.ErrorMessage,
                    "YOLO 环境检查", MessageBoxButtons.OK, status.IsReady ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (OperationCanceledException) { AppendLog("环境检查已停止。"); }
            finally { EndOperation(); }
        }

        private async Task InstallEnvironmentAsync()
        {
            if (operationInProgress) return;
            if (MessageBox.Show(this, "将为当前训练 Python 安装 Ultralytics、ONNX 等依赖。是否继续？", "安装 YOLO 训练环境",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            BeginOperation();
            try
            {
                await AppServices.YoloTraining.InstallEnvironmentAsync(CreateProgress(), operationCancellation.Token);
                YoloEnvironmentStatus status = await AppServices.YoloTraining.CheckEnvironmentAsync(CreateProgress(), operationCancellation.Token);
                if (!status.IsReady) throw new InvalidOperationException(status.ErrorMessage);
                MessageBox.Show(this, "YOLO 训练环境安装完成。", "安装环境", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException) { AppendLog("环境安装已停止。"); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "安装环境失败", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { EndOperation(); }
        }

        private async Task StartTrainingAsync()
        {
            if (operationInProgress) return;
            long productId = AppSession.CurrentProductId;
            if (productId <= 0)
            {
                MessageBox.Show(this, "请先选择产品。", "YOLO 训练", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            YoloTrainingRequest request;
            try { request = CreateTrainingRequest(productId); }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "训练配置", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show(this,
                "将使用当前产品中已审核通过/确认正常的数据训练 " + request.ModelVariant + "。\r\n训练完成后会自动导出 ONNX 并设为启用模型。是否开始？",
                "开始 YOLO 训练", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            txtTrainingLog.Clear();
            ResetMetrics();
            BeginOperation();
            try
            {
                YoloTrainingRun run = await AppServices.YoloTraining.TrainAsync(request, CreateProgress(), operationCancellation.Token);
                RefreshAll();
                SelectRun(run.RunCode);
                MessageBox.Show(this,
                    "YOLO 训练完成。\r\nmAP50-95：" + run.Map5095.ToString("0.0000") + "\r\nONNX 模型已自动入库并启用。",
                    "训练完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException) { AppendLog("训练已停止。"); RefreshAll(); }
            catch (Exception ex)
            {
                RefreshAll();
                MessageBox.Show(this, ex.Message, "YOLO 训练失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { EndOperation(); }
        }

        private YoloTrainingRequest CreateTrainingRequest(long productId)
        {
            string sizeText = Convert.ToString(cboTileSize.SelectedItem);
            int separator = string.IsNullOrWhiteSpace(sizeText) ? -1 : sizeText.IndexOf(' ');
            int imageSize;
            if (!int.TryParse(separator < 0 ? sizeText : sizeText.Substring(0, separator), out imageSize)) throw new ArgumentException("输入尺寸无效。");
            double learningRate;
            if (!double.TryParse(txtLearningRate.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out learningRate) &&
                !double.TryParse(txtLearningRate.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out learningRate))
                throw new ArgumentException("学习率无效。");
            string device = Convert.ToString(cboDevice.SelectedItem);
            if (device == "GPU") device = "0";
            else if (device == "CPU") device = "cpu";
            else device = "auto";
            return new YoloTrainingRequest
            {
                ProductId = productId,
                ModelVariant = Convert.ToString(cboArchitecture.SelectedItem),
                ImageSize = imageSize,
                BatchSize = (int)numBatchSize.Value,
                Epochs = (int)numEpoch.Value,
                LearningRate = learningRate,
                Device = device,
                Seed = 42
            };
        }

        private void BeginOperation()
        {
            operationInProgress = true;
            operationCancellation = new CancellationTokenSource();
            btnStartTraining.Enabled = false;
            btnInstallEnvironment.Enabled = false;
            btnCheckEnvironment.Enabled = false;
            btnCancelTraining.Enabled = true;
        }

        private void EndOperation()
        {
            operationInProgress = false;
            if (operationCancellation != null) operationCancellation.Dispose();
            operationCancellation = null;
            if (IsDisposed) return;
            btnStartTraining.Enabled = AppSession.CurrentProductId > 0;
            btnInstallEnvironment.Enabled = true;
            btnCheckEnvironment.Enabled = true;
            btnCancelTraining.Enabled = false;
        }

        private void CancelOperation()
        {
            if (operationCancellation != null && !operationCancellation.IsCancellationRequested)
            {
                AppendLog("正在停止当前操作……");
                operationCancellation.Cancel();
            }
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendLog), message);
                return;
            }
            string line = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message.Trim();
            if (txtTrainingLog.TextLength > 180000) txtTrainingLog.Text = txtTrainingLog.Text.Substring(txtTrainingLog.TextLength - 120000);
            txtTrainingLog.AppendText((txtTrainingLog.TextLength == 0 ? string.Empty : Environment.NewLine) + line);
            txtTrainingLog.SelectionStart = txtTrainingLog.TextLength;
            txtTrainingLog.ScrollToCaret();
        }

        private void ImportModel()
        {
            long productId = AppSession.CurrentProductId;
            loadedProductId = productId;
            loadedRevision = InspectionConfigurationRevisionTracker.GetRevision(productId);
            if (productId <= 0) { MessageBox.Show(this, "请先选择产品。", "模型导入", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using (OpenFileDialog dialog = new OpenFileDialog { Filter = "ONNX 模型 (*.onnx)|*.onnx", Multiselect = false, Title = "选择 ONNX 模型" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                using (ModelImportDialog settings = new ModelImportDialog(dialog.FileName, AppServices.Products.GetDefectCategories(productId)))
                {
                    if (settings.ShowDialog(this) != DialogResult.OK) return;
                    try
                    {
                        UseWaitCursor = true;
                        AppServices.Models.Import(dialog.FileName, settings.CreateDefinition(productId));
                        RefreshModels();
                        MessageBox.Show(this, "模型已校验并导入项目工作区。", "模型导入", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex) { MessageBox.Show(this, ex.Message, "模型导入失败", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    finally { UseWaitCursor = false; }
                }
            }
        }

        private InferenceModel SelectedModel()
        {
            return dgvModelLibrary.SelectedRows.Count == 0 ? null : dgvModelLibrary.SelectedRows[0].Tag as InferenceModel;
        }

        private YoloTrainingRun SelectedRun()
        {
            return dgvTrainingQueue.SelectedRows.Count == 0 ? null : dgvTrainingQueue.SelectedRows[0].Tag as YoloTrainingRun;
        }

        private void ActivateSelected()
        {
            InferenceModel model = SelectedModel();
            if (model == null) return;
            try { AppServices.Models.Activate(model.ProductId, model.Id); RefreshModels(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "启用模型", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void DeleteSelected()
        {
            InferenceModel model = SelectedModel();
            if (model == null) return;
            if (MessageBox.Show(this, "删除模型 “" + model.ModelName + "”？已被 Recipe 引用的模型不会被删除。", "删除模型", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try { AppServices.Models.Delete(model.ProductId, model.Id); RefreshModels(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "删除模型", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        public void RefreshTrainingJobs() { RefreshAll(); }

        private void RefreshAll()
        {
            if (!runtimeInitialized || IsDisposed) return;
            loadedProductId = AppSession.CurrentProductId;
            loadedRevision = ProductDataRevisionTracker.GetRevision(loadedProductId);
            RefreshDatasetSplit();
            RefreshRuns();
            RefreshModels();
            if (!operationInProgress) btnStartTraining.Enabled = loadedProductId > 0;
        }

        private void RefreshDatasetSplit()
        {
            dgvDatasetSplit.Rows.Clear();
            if (loadedProductId <= 0) return;
            IList<DatasetImage> images = AppServices.Datasets.GetImages(loadedProductId);
            int totalImages = images.Count;
            Dictionary<string, int> imageCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> instanceCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DatasetImage image in images)
            {
                string split = NormalizeSplit(image.DatasetSplit);
                imageCounts[split] = GetCount(imageCounts, split) + 1;
                int instances = AppServices.Datasets.GetAnnotations(image.Id).Count + AppServices.Masks.GetMasks(image.Id).Count;
                instanceCounts[split] = GetCount(instanceCounts, split) + instances;
            }
            int totalInstances = instanceCounts.Values.Sum();
            AddSplitRow("训练集", DatasetSplit.Train, totalImages, totalInstances, imageCounts, instanceCounts);
            AddSplitRow("验证集", DatasetSplit.Validation, totalImages, totalInstances, imageCounts, instanceCounts);
            AddSplitRow("测试集", DatasetSplit.Test, totalImages, totalInstances, imageCounts, instanceCounts);
            AddSplitRow("未划分", DatasetSplit.Unassigned, totalImages, totalInstances, imageCounts, instanceCounts);
        }

        private void AddSplitRow(string title, string split, int totalImages, int totalInstances,
            IDictionary<string, int> imageCounts, IDictionary<string, int> instanceCounts)
        {
            int imageCount = GetCount(imageCounts, split);
            int instanceCount = GetCount(instanceCounts, split);
            dgvDatasetSplit.Rows.Add(title, imageCount, totalImages == 0 ? "0%" : (imageCount / (double)totalImages).ToString("P0"),
                instanceCount, totalInstances == 0 ? "0%" : (instanceCount / (double)totalInstances).ToString("P0"));
        }

        private void RefreshRuns()
        {
            string selectedCode = SelectedRun() == null ? null : SelectedRun().RunCode;
            dgvTrainingQueue.Rows.Clear();
            dgvBenchmark.Rows.Clear();
            if (loadedProductId <= 0) { ResetMetrics(); return; }
            foreach (YoloTrainingRun run in AppServices.YoloTraining.GetRuns(loadedProductId))
            {
                int index = dgvTrainingQueue.Rows.Add(run.RunCode, run.ModelVariant, DatasetName(run.DatasetDirectory),
                    run.CreatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), DisplayRunStatus(run.Status), "普通");
                dgvTrainingQueue.Rows[index].Tag = run;
                if (run.Status == "Completed")
                    dgvBenchmark.Rows.Add(run.RunCode, run.Recall.ToString("0.0000"), run.Precision.ToString("0.0000"),
                        run.F1.ToString("0.0000"), run.Map5095.ToString("0.0000"), run.Map50.ToString("0.0000"),
                        run.InferenceMilliseconds.ToString("0.00"), run.ModelVariant);
            }
            if (!string.IsNullOrWhiteSpace(selectedCode)) SelectRun(selectedCode);
            if (dgvTrainingQueue.SelectedRows.Count == 0 && dgvTrainingQueue.Rows.Count > 0) dgvTrainingQueue.Rows[0].Selected = true;
            ShowSelectedRunMetrics();
        }

        private void RefreshModels()
        {
            dgvModelLibrary.Rows.Clear();
            if (loadedProductId <= 0)
            {
                if (txtTrainingLog.TextLength == 0) AppendLog("请先选择产品，再检查数据集并开始 YOLO 训练。");
                return;
            }
            foreach (InferenceModel model in AppServices.Models.GetModels(loadedProductId))
            {
                int index = dgvModelLibrary.Rows.Add(model.Version, model.ModelType, model.InputWidth + " × " + model.InputHeight,
                    model.CreatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), model.ConfidenceThreshold.ToString("0.00"),
                    model.IsActive ? "已启用" : "未启用", model.Sha256 == null ? "" : model.Sha256.Substring(0, Math.Min(12, model.Sha256.Length)) + "…", model.Labels);
                dgvModelLibrary.Rows[index].Tag = model;
            }
        }

        private void ShowSelectedRunMetrics()
        {
            YoloTrainingRun run = SelectedRun();
            if (run == null) { ResetMetrics(); return; }
            lblValMiou.Text = run.Status == "Completed" ? run.Map5095.ToString("0.0000") : "-";
            lblF1.Text = run.Status == "Completed" ? run.F1.ToString("0.0000") : "-";
            lblRecall.Text = run.Status == "Completed" ? run.Recall.ToString("0.0000") : "-";
            lblPrecision.Text = run.Status == "Completed" ? run.Precision.ToString("0.0000") : "-";
            lblAcceptance.Text = DisplayRunStatus(run.Status);
        }

        private void ResetMetrics()
        {
            lblValMiou.Text = "-";
            lblF1.Text = "-";
            lblRecall.Text = "-";
            lblPrecision.Text = "-";
            lblAcceptance.Text = "待训练";
        }

        private void SelectRun(string runCode)
        {
            foreach (DataGridViewRow row in dgvTrainingQueue.Rows)
            {
                YoloTrainingRun run = row.Tag as YoloTrainingRun;
                row.Selected = run != null && string.Equals(run.RunCode, runCode, StringComparison.OrdinalIgnoreCase);
                if (row.Selected) dgvTrainingQueue.CurrentCell = row.Cells[0];
            }
        }

        private void OpenSelectedRunFolder()
        {
            YoloTrainingRun run = SelectedRun();
            string path = run == null ? ProjectStoragePathForRuns() : run.RunDirectory;
            if (!Directory.Exists(path)) { MessageBox.Show(this, "训练目录不存在。", "打开目录", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = "\"" + path + "\"", UseShellExecute = true });
        }

        private static string ProjectStoragePathForRuns()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workspace", "TrainingRuns");
        }

        private static int GetCount(IDictionary<string, int> values, string key)
        {
            int value;
            return values.TryGetValue(key, out value) ? value : 0;
        }

        private static string NormalizeSplit(string value)
        {
            if (string.Equals(value, DatasetSplit.Train, StringComparison.OrdinalIgnoreCase)) return DatasetSplit.Train;
            if (string.Equals(value, DatasetSplit.Validation, StringComparison.OrdinalIgnoreCase)) return DatasetSplit.Validation;
            if (string.Equals(value, DatasetSplit.Test, StringComparison.OrdinalIgnoreCase)) return DatasetSplit.Test;
            return DatasetSplit.Unassigned;
        }

        private static string DatasetName(string path)
        {
            return string.IsNullOrWhiteSpace(path) ? "待导出" : new DirectoryInfo(path).Name;
        }

        private static string DisplayRunStatus(string status)
        {
            if (status == "Preparing") return "准备数据";
            if (status == "Running") return "训练中";
            if (status == "Completed") return "已完成";
            if (status == "Cancelled") return "已停止";
            if (status == "Failed") return "失败";
            if (status == "Installing") return "安装环境";
            if (status == "Ready") return "环境就绪";
            return string.IsNullOrWhiteSpace(status) ? "等待" : status;
        }
    }
}
