using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IAD.Infrastructure.Storage;
using IAD.Models;
using IAD.Security;
using IAD.Services;

namespace IAD.Pages
{
    public partial class OnlineInspectionPage : UserControl
    {
        private sealed class QueueItem { public string Path { get; set; } public string Status { get; set; } public DataGridViewRow Row { get; set; } }
        private bool runtimeInitialized;
        private bool running;
        private CancellationTokenSource cancellation;
        private readonly List<QueueItem> queue = new List<QueueItem>();
        private readonly List<InspectionResult> sessionResults = new List<InspectionResult>();
        private PictureBox preview;
        private bool clearAfterStop;

        public OnlineInspectionPage() { InitializeComponent(); }

        public void InitializeRuntime()
        {
            if (runtimeInitialized) return;
            runtimeInitialized = true;
            ConfigureRuntime();
            BindEvents();
            ClearDemoData();
            RefreshBackend();
        }

        private void ConfigureRuntime()
        {
            dgvQueue.MultiSelect=false; dgvQueue.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
            dgvResults.MultiSelect=false; dgvResults.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
            dgvQueueCol1.HeaderText="图片"; dgvQueueCol2.HeaderText="状态";
            dgvResultsCol1.HeaderText="记录"; dgvResultsCol2.HeaderText="判定"; dgvResultsCol3.HeaderText="瑕疵数";
            dgvDefectsCol1.HeaderText="类别"; dgvDefectsCol2.HeaderText="置信度"; dgvDefectsCol3.HeaderText="面积"; dgvDefectsCol4.HeaderText="位置 / 规则";
            productGrid.Visible=false;
            preview = new PictureBox { Dock=DockStyle.Fill, SizeMode=PictureBoxSizeMode.Zoom, BackColor=Color.FromArgb(30,30,30) };
            grpCanvas.Controls.Add(preview);
            preview.BringToFront();
            btnStart.Text="开始离线检测"; btnPause.Text="停止"; btnBatchInspect.Text="导入文件夹"; btnExport.Text="导出本次 CSV";
            btnPause.Enabled=false;
        }

        private void BindEvents()
        {
            btnLoadImage.Click += delegate { LoadImages(); };
            btnBatchInspect.Click += delegate { LoadFolder(); };
            btnStart.Click += delegate { StartInspection(); };
            btnPause.Click += delegate { StopInspection(); };
            btnExport.Click += delegate { ExportSession(); };
            dgvResults.SelectionChanged += delegate { ShowSelectedResult(); };
            AppSession.CurrentProductChanged += delegate
            {
                if (IsDisposed) return;
                if (running) { clearAfterStop=true; StopInspection(); }
                else ClearQueueAndResults();
                RefreshBackend();
            };
            VisibleChanged += delegate { if (Visible) RefreshBackend(); };
        }

        private void ClearDemoData()
        {
            dgvQueue.Rows.Clear(); dgvResults.Rows.Clear(); dgvDefects.Rows.Clear(); dgvNgStats.Rows.Clear();
            lblAcquire.Text="等待图片"; lblPreprocess.Text="等待"; lblLocate.Text="离线图片"; lblDetect.Text="等待"; lblJudge.Text="等待";
            lblMetrics.Text="队列 0 · 完成 0";
        }

        private void RefreshBackend()
        {
            long productId = AppSession.CurrentProductId;
            if (productId <= 0) { lblRecipe.Text="未选择产品"; lblModel.Text="-"; return; }
            try
            {
                InspectionRecipe recipe = AppServices.Recipes.GetActiveRecipe(productId);
                InferenceModel model = recipe != null && recipe.ModelId.HasValue ? AppServices.Models.GetModel(recipe.ModelId.Value) : null;
                lblRecipe.Text=recipe == null ? "未启用" : recipe.RecipeName;
                lblModel.Text=model == null ? "未绑定" : model.Version + " / " + model.ModelType;
                lblBackend.Text="ONNX Runtime"; lblDevice.Text="CPU x64";
            }
            catch (Exception ex) { lblRecipe.Text=ex.Message; }
        }

        private void LoadImages()
        {
            using (OpenFileDialog dialog = new OpenFileDialog { Filter="图片|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|所有文件|*.*", Multiselect=true, Title="选择待检测图片" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK) AddFiles(dialog.FileNames);
            }
        }

        private void LoadFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog { Description="选择包含待检测图片的文件夹" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                string[] extensions={".png",".jpg",".jpeg",".bmp",".tif",".tiff"};
                AddFiles(Directory.EnumerateFiles(dialog.SelectedPath).Where(p => extensions.Contains(Path.GetExtension(p).ToLowerInvariant())).ToArray());
            }
        }

        private void AddFiles(IEnumerable<string> paths)
        {
            foreach (string path in paths)
            {
                if (queue.Any(q => string.Equals(q.Path, path, StringComparison.OrdinalIgnoreCase) && q.Status != "已完成")) continue;
                QueueItem item = new QueueItem { Path=path, Status="等待" };
                int index=dgvQueue.Rows.Add(Path.GetFileName(path), item.Status);
                item.Row=dgvQueue.Rows[index]; item.Row.Tag=item; queue.Add(item);
            }
            lblAcquire.Text=queue.Count + " 张图片";
            UpdateMetrics();
        }

        public async void StartInspection()
        {
            if (running) return;
            long productId=AppSession.CurrentProductId;
            if (productId<=0) { MessageBox.Show(this,"请先选择产品。","离线检测",MessageBoxButtons.OK,MessageBoxIcon.Information); return; }
            List<QueueItem> pending=queue.Where(q => q.Status=="等待" || q.Status=="失败" || q.Status=="已停止").ToList();
            if (pending.Count==0) { MessageBox.Show(this,"请先导入待检测图片。","离线检测",MessageBoxButtons.OK,MessageBoxIcon.Information); return; }
            running=true; cancellation=new CancellationTokenSource(); btnStart.Enabled=false; btnPause.Enabled=true;
            lblPreprocess.Text="运行中"; lblDetect.Text="ONNX 推理中";
            string batch=DateTime.Now.ToString("yyyyMMdd-HHmmss");
            try
            {
                foreach (QueueItem item in pending)
                {
                    cancellation.Token.ThrowIfCancellationRequested();
                    SetQueueStatus(item,"检测中");
                    InspectionResult result=await Task.Run(() => AppServices.OfflineInspection.Inspect(productId,item.Path,batch,AppSession.CurrentRole,cancellation.Token));
                    sessionResults.Add(result); SetQueueStatus(item,result.OverallResult=="ERROR" ? "失败" : "已完成"); AddResult(result);
                }
                lblDetect.Text="本批完成";
            }
            catch (OperationCanceledException)
            {
                foreach (QueueItem item in pending.Where(p => p.Status=="等待" || p.Status=="检测中")) SetQueueStatus(item,"已停止");
                lblDetect.Text="已停止";
            }
            finally
            {
                running=false; btnStart.Enabled=true; btnPause.Enabled=false; lblPreprocess.Text="等待"; UpdateMetrics();
                if (clearAfterStop) { clearAfterStop=false; ClearQueueAndResults(); }
            }
        }

        public void StopInspection() { if (cancellation != null && !cancellation.IsCancellationRequested) cancellation.Cancel(); }

        private void SetQueueStatus(QueueItem item,string status) { item.Status=status; if (item.Row!=null && !item.Row.IsNewRow) item.Row.Cells[1].Value=status; }

        private void AddResult(InspectionResult result)
        {
            int index=dgvResults.Rows.Add("#"+result.Id,result.OverallResult,result.Defects.Count);
            dgvResults.Rows[index].Tag=result;
            dgvResults.ClearSelection(); dgvResults.Rows[index].Selected=true;
            lblJudge.Text=result.OverallResult + (result.ErrorMessage==null ? "" : " · "+result.ErrorMessage);
            UpdateNgStats(); UpdateMetrics();
        }

        private void ShowSelectedResult()
        {
            if (dgvResults.SelectedRows.Count==0) return;
            InspectionResult summary=dgvResults.SelectedRows[0].Tag as InspectionResult;
            if (summary==null) return;
            InspectionResult result=AppServices.Results.GetResult(summary.Id) ?? summary;
            dgvDefects.Rows.Clear();
            foreach (DefectInstance defect in result.Defects)
                dgvDefects.Rows.Add(defect.CategoryName ?? defect.CategoryCode,defect.Confidence.ToString("P1"),defect.Area.ToString("0.##"),"("+defect.X.ToString("0")+","+defect.Y.ToString("0")+") · "+defect.RuleDecision);
            ShowImage(result.AnnotatedImagePath ?? result.ArchivedImagePath ?? result.OriginalImagePath);
        }

        private void ShowImage(string path)
        {
            if (preview.Image!=null) { Image old=preview.Image; preview.Image=null; old.Dispose(); }
            string full=ResolvePath(path);
            if (string.IsNullOrWhiteSpace(full) || !File.Exists(full)) return;
            using (FileStream stream=new FileStream(full,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
            using (Image image=Image.FromStream(stream)) preview.Image=new Bitmap(image);
        }

        private static string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            if (Path.IsPathRooted(path)) return path;
            return Path.Combine(ProjectStoragePaths.RootPath,path);
        }

        private void UpdateNgStats()
        {
            dgvNgStats.Rows.Clear();
            var groups=sessionResults.SelectMany(r=>r.Defects).Where(d=>d.Result=="NG").GroupBy(d=>d.CategoryName??d.CategoryCode).OrderByDescending(g=>g.Count()).ToList();
            int total=Math.Max(1,groups.Sum(g=>g.Count()));
            foreach (var group in groups) dgvNgStats.Rows.Add(group.Key,group.Count(),((double)group.Count()/total).ToString("P1"));
        }

        private void UpdateMetrics()
        {
            int ok=sessionResults.Count(r=>r.OverallResult=="OK"), ng=sessionResults.Count(r=>r.OverallResult=="NG"), error=sessionResults.Count(r=>r.OverallResult=="ERROR");
            lblMetrics.Text="队列 "+queue.Count+" · 完成 "+sessionResults.Count+" · OK "+ok+" · NG "+ng+" · ERROR "+error;
        }

        private void ExportSession()
        {
            if (sessionResults.Count==0) return;
            using (SaveFileDialog dialog=new SaveFileDialog { Filter="CSV 文件|*.csv", FileName="inspection_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".csv" })
            {
                if (dialog.ShowDialog(this)!=DialogResult.OK) return;
                StringBuilder csv=new StringBuilder("ResultId,Batch,Image,Result,DefectCount,Model,ElapsedMs,Time,Error\r\n");
                foreach (InspectionResult result in sessionResults) csv.AppendLine(string.Join(",",Csv(result.Id),Csv(result.BatchCode),Csv(result.OriginalImagePath),Csv(result.OverallResult),Csv(result.Defects.Count),Csv(result.ModelVersion),Csv(result.InferenceMilliseconds),Csv(result.FinishedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")),Csv(result.ErrorMessage)));
                File.WriteAllText(dialog.FileName,csv.ToString(),new UTF8Encoding(true));
            }
        }

        private static string Csv(object value) { string text=Convert.ToString(value)??""; return "\""+text.Replace("\"","\"\"")+"\""; }
        private void ClearQueueAndResults() { queue.Clear(); sessionResults.Clear(); ClearDemoData(); ShowImage(null); }
    }
}
