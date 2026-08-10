using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IAD.Infrastructure.Storage;
using IAD.Models;
using IAD.Security;
using IAD.Services;

namespace IAD.Pages
{
    public partial class TraceabilityPage : UserControl
    {
        private sealed class Choice<T>
        {
            public string Text { get; set; }
            public T Value { get; set; }
            public override string ToString() { return Text; }
        }

        private bool runtimeInitialized;
        private PictureBox preview;
        private readonly List<InspectionResult> currentResults = new List<InspectionResult>();
        private long loadedProductId;
        private long loadedRevision = -1;

        public TraceabilityPage() { InitializeComponent(); }

        public void InitializeRuntime()
        {
            if (runtimeInitialized) return;
            runtimeInitialized=true;
            ConfigureRuntime();
            BindEvents();
            ReloadFilters();
            QueryRecords();
        }

        private void ConfigureRuntime()
        {
            txtDateRange.Text=DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd")+" ~ "+DateTime.Today.ToString("yyyy-MM-dd");
            dgvRecords.ReadOnly=true; dgvRecords.MultiSelect=false; dgvRecords.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
            string[] headers={"记录ID","图片","时间","批次","模型","瑕疵数","耗时(ms)","操作员","Recipe","判定"};
            for (int i=0;i<headers.Length;i++) dgvRecords.Columns[i].HeaderText=headers[i];
            dgvCurrentDefects.ReadOnly=true;
            pnlPreview.Controls.Clear();
            preview=new PictureBox { Dock=DockStyle.Fill, SizeMode=PictureBoxSizeMode.Zoom, BackColor=Color.FromArgb(28,28,28) };
            pnlPreview.Controls.Add(preview);
            btnExportCsv.Text="导出查询 CSV";
            btnExportPdf.Enabled=false; btnExportZip.Enabled=false; btnExportBatch.Enabled=false; btnPrint.Enabled=false;
            ToolTip tip=new ToolTip();
            tip.SetToolTip(btnExportPdf,"当前版本提供 CSV 导出"); tip.SetToolTip(btnExportZip,"当前版本提供 CSV 导出");
        }

        private void BindEvents()
        {
            btnQuery.Click += delegate { QueryRecords(); };
            btnExportCsv.Click += delegate { ExportCsv(); };
            dgvRecords.SelectionChanged += delegate { ShowSelected(); };
            AppSession.CurrentProductChanged += delegate { if (!IsDisposed) { loadedProductId=0; ReloadFilters(); QueryRecords(); } };
            VisibleChanged += delegate
            {
                long productId=AppSession.CurrentProductId;
                if (!Visible) return;
                if (loadedProductId!=productId) { ReloadFilters(); QueryRecords(); }
                else if (loadedRevision!=InspectionResultRevisionTracker.GetRevision(productId)) QueryRecords();
            };
        }

        private void ReloadFilters()
        {
            if (!runtimeInitialized) return;
            long productId=AppSession.CurrentProductId;
            object oldStatus=cboStatus.SelectedItem;
            cboStatus.Items.Clear(); cboStatus.Items.AddRange(new object[]{"全部","OK","NG","ERROR"}); cboStatus.SelectedIndex=0;
            cboClass.Items.Clear(); cboClass.Items.Add(new Choice<string>{Text="全部类别",Value=null});
            cboRecipe.Items.Clear(); cboRecipe.Items.Add(new Choice<long?>{Text="全部 Recipe",Value=null});
            if (productId>0)
            {
                foreach (DefectCategory category in AppServices.Products.GetDefectCategories(productId)) cboClass.Items.Add(new Choice<string>{Text=category.CategoryName+" ["+category.CategoryCode+"]",Value=category.CategoryCode});
                foreach (InspectionRecipe recipe in AppServices.Recipes.GetRecipes(productId)) cboRecipe.Items.Add(new Choice<long?>{Text=recipe.RecipeName+(recipe.IsActive?"（启用）":""),Value=recipe.Id});
            }
            cboClass.SelectedIndex=0; cboRecipe.SelectedIndex=0;
        }

        public void QueryRecords()
        {
            if (!runtimeInitialized) return;
            try
            {
                DateTime? from,to;
                ParseDateRange(txtDateRange.Text,out from,out to);
                string status=Convert.ToString(cboStatus.SelectedItem);
                Choice<string> category=cboClass.SelectedItem as Choice<string>;
                Choice<long?> recipe=cboRecipe.SelectedItem as Choice<long?>;
                InspectionResultQuery query=new InspectionResultQuery
                {
                    ProductId=AppSession.CurrentProductId>0?(long?)AppSession.CurrentProductId:null,
                    FromUtc=from.HasValue?from.Value.ToUniversalTime():(DateTime?)null,
                    ToUtc=to.HasValue?to.Value.ToUniversalTime():(DateTime?)null,
                    OverallResult=status=="全部"?null:status, CategoryCode=category==null?null:category.Value,
                    RecipeId=recipe==null?null:recipe.Value, Keyword=txtKeyword.Text, Limit=500
                };
                currentResults.Clear(); currentResults.AddRange(AppServices.Results.QueryResults(query));
                loadedProductId=AppSession.CurrentProductId; loadedRevision=InspectionResultRevisionTracker.GetRevision(loadedProductId);
                dgvRecords.Rows.Clear();
                foreach (InspectionResult result in currentResults)
                {
                    InspectionRecipe itemRecipe=result.RecipeId.HasValue?AppServices.Recipes.GetRecipe(result.RecipeId.Value):null;
                    int row=dgvRecords.Rows.Add(result.Id,Path.GetFileName(result.OriginalImagePath??result.SourceImagePath),result.FinishedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                        result.BatchCode,result.ModelVersion,result.DefectCount,result.InferenceMilliseconds,result.OperatorName,itemRecipe==null?"-":itemRecipe.RecipeName,result.OverallResult);
                    dgvRecords.Rows[row].Tag=result;
                }
                if (dgvRecords.Rows.Count>0) { dgvRecords.Rows[0].Selected=true; ShowSelected(); } else ClearDetails();
                grpRecords.Text="检测记录（"+currentResults.Count+"）";
            }
            catch (Exception ex) { MessageBox.Show(this,ex.Message,"查询失败",MessageBoxButtons.OK,MessageBoxIcon.Error); }
        }

        private void ShowSelected()
        {
            if (dgvRecords.SelectedRows.Count==0) return;
            InspectionResult summary=dgvRecords.SelectedRows[0].Tag as InspectionResult;
            if (summary==null) return;
            InspectionResult result=AppServices.Results.GetResult(summary.Id);
            if (result==null) return;
            Product product=AppServices.Products.GetProduct(result.ProductId);
            InspectionRecipe recipe=result.RecipeId.HasValue?AppServices.Recipes.GetRecipe(result.RecipeId.Value):null;
            lblOriginal.Text=Path.GetFileName(result.OriginalImagePath??result.SourceImagePath);
            lblProduct.Text=product==null?result.ProductId.ToString():product.ProductName;
            lblMask.Text=Path.GetFileName(result.AnnotatedImagePath??result.ArchivedImagePath);
            lblInstances.Text=result.Defects.Count.ToString();
            lblClassDetail.Text=string.Join("、",result.Defects.Select(d=>d.CategoryName??d.CategoryCode).Distinct().ToArray());
            lblProbability.Text=result.Defects.Count==0?"-":result.Defects.Max(d=>d.Confidence).ToString("P2");
            lblState.Text=result.OverallResult; lblRecipeDetail.Text=recipe==null?"-":recipe.RecipeName;
            lblModel.Text=result.ModelVersion??"-"; lblOperator.Text=result.OperatorName??"system";
            dgvCurrentDefects.Rows.Clear();
            int index=1;
            foreach (DefectInstance defect in result.Defects)
                dgvCurrentDefects.Rows.Add(index++,defect.CategoryName??defect.CategoryCode,"("+defect.X.ToString("0")+","+defect.Y.ToString("0")+")",defect.Width.ToString("0.##")+" × "+defect.Height.ToString("0.##"),defect.Confidence.ToString("P2"),defect.RuleDecision,defect.Result);
            txtAudit.Text="开始："+result.StartedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")+"\r\n"+
                          "结束："+result.FinishedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")+"\r\n"+
                          "推理耗时："+result.InferenceMilliseconds+" ms\r\n"+
                          "模型："+(result.ModelVersion??"-")+"\r\n规则："+(result.RuleVersion??"-")+"\r\n"+
                          (string.IsNullOrWhiteSpace(result.ErrorMessage)?"结果已归档，原始判定不可变。":"错误："+result.ErrorMessage);
            ShowImage(result.AnnotatedImagePath??result.ArchivedImagePath??result.OriginalImagePath);
        }

        private void ShowImage(string path)
        {
            if (preview.Image!=null) { Image old=preview.Image; preview.Image=null; old.Dispose(); }
            if (string.IsNullOrWhiteSpace(path)) return;
            string full=Path.IsPathRooted(path)?path:Path.Combine(ProjectStoragePaths.RootPath,path);
            if (!File.Exists(full)) return;
            using (FileStream stream=new FileStream(full,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
            using (Image image=Image.FromStream(stream)) preview.Image=new Bitmap(image);
        }

        private void ClearDetails()
        {
            lblOriginal.Text=lblProduct.Text=lblMask.Text=lblInstances.Text=lblClassDetail.Text=lblProbability.Text=lblState.Text=lblRecipeDetail.Text=lblModel.Text=lblOperator.Text="-";
            dgvCurrentDefects.Rows.Clear(); txtAudit.Text="没有符合条件的检测记录。"; ShowImage(null);
        }

        private void ExportCsv()
        {
            if (currentResults.Count==0) return;
            using (SaveFileDialog dialog=new SaveFileDialog { Filter="CSV 文件|*.csv",FileName="traceability_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".csv" })
            {
                if (dialog.ShowDialog(this)!=DialogResult.OK) return;
                StringBuilder csv=new StringBuilder("ResultId,Image,Time,Batch,Result,DefectCount,Model,Rule,ElapsedMs,Operator,Error\r\n");
                foreach (InspectionResult result in currentResults) csv.AppendLine(string.Join(",",Csv(result.Id),Csv(result.OriginalImagePath??result.SourceImagePath),Csv(result.FinishedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")),Csv(result.BatchCode),Csv(result.OverallResult),Csv(result.DefectCount),Csv(result.ModelVersion),Csv(result.RuleVersion),Csv(result.InferenceMilliseconds),Csv(result.OperatorName),Csv(result.ErrorMessage)));
                File.WriteAllText(dialog.FileName,csv.ToString(),new UTF8Encoding(true));
                MessageBox.Show(this,"已导出 "+currentResults.Count+" 条记录。","导出完成",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }

        private static void ParseDateRange(string text,out DateTime? from,out DateTime? to)
        {
            from=null; to=null;
            if (string.IsNullOrWhiteSpace(text)) return;
            string[] parts=text.Split(new[]{'~','至'},StringSplitOptions.RemoveEmptyEntries);
            DateTime value;
            if (parts.Length>0 && DateTime.TryParse(parts[0].Trim(),CultureInfo.CurrentCulture,DateTimeStyles.None,out value)) from=value;
            if (parts.Length>1 && DateTime.TryParse(parts[1].Trim(),CultureInfo.CurrentCulture,DateTimeStyles.None,out value)) to=value.Date.AddDays(1).AddTicks(-1);
            if (!from.HasValue && !to.HasValue) throw new ArgumentException("日期范围格式应为：yyyy-MM-dd ~ yyyy-MM-dd");
        }
        private static string Csv(object value) { string text=Convert.ToString(value)??""; return "\""+text.Replace("\"","\"\"")+"\""; }
    }
}
