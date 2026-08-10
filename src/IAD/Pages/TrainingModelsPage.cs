using System;
using System.Drawing;
using System.IO;
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
        private long loadedProductId;
        private long loadedRevision = -1;

        public TrainingModelsPage() { InitializeComponent(); }

        public void InitializeRuntime()
        {
            if (runtimeInitialized) return;
            runtimeInitialized = true;
            BuildModelToolbar();
            ConfigureModelGrid();
            BindEvents();
            ClearTrainingPlaceholders();
            RefreshModels();
        }

        private void BuildModelToolbar()
        {
            FlowLayoutPanel actions = new FlowLayoutPanel { Dock=DockStyle.Top, Height=38, Padding=new Padding(4,3,4,3), BackColor=Color.WhiteSmoke };
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
            return new Button { Text=text, Width=width, Height=29, FlatStyle=FlatStyle.Flat, BackColor=Color.White };
        }

        private void ConfigureModelGrid()
        {
            dgvModelLibrary.ReadOnly = true;
            dgvModelLibrary.MultiSelect = false;
            dgvModelLibrary.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvModelLibraryCol1.HeaderText = "版本";
            dgvModelLibraryCol2.HeaderText = "输出格式";
            dgvModelLibraryCol3.HeaderText = "输入尺寸";
            dgvModelLibraryCol4.HeaderText = "导入时间";
            dgvModelLibraryCol5.HeaderText = "阈值";
            dgvModelLibraryCol6.HeaderText = "状态";
            dgvModelLibraryCol7.HeaderText = "SHA256";
            dgvModelLibraryCol8.HeaderText = "类别顺序";
        }

        private void BindEvents()
        {
            btnImportModel.Click += delegate { ImportModel(); };
            btnActivateModel.Click += delegate { ActivateSelected(); };
            btnDeleteModel.Click += delegate { DeleteSelected(); };
            btnRefreshModels.Click += delegate { RefreshModels(); };
            AppSession.CurrentProductChanged += delegate { if (!IsDisposed) { loadedProductId=0; RefreshModels(); } };
            VisibleChanged += delegate { if (Visible && (loadedProductId!=AppSession.CurrentProductId || loadedRevision!=InspectionConfigurationRevisionTracker.GetRevision(AppSession.CurrentProductId))) RefreshModels(); };
        }

        private void ClearTrainingPlaceholders()
        {
            dgvDatasetSplit.Rows.Clear(); dgvTrainingQueue.Rows.Clear(); dgvBenchmark.Rows.Clear();
            txtTrainingLog.Text = "当前版本提供 ONNX 离线模型导入、校验、启用和推理。\r\n训练 Worker 尚未接入；请从 PyTorch、TensorFlow 等训练环境导出 ONNX 后在下方模型库导入。";
            lblAcceptance.Text = "离线模型";
        }

        private void ImportModel()
        {
            long productId = AppSession.CurrentProductId;
            loadedProductId=productId; loadedRevision=InspectionConfigurationRevisionTracker.GetRevision(productId);
            if (productId <= 0) { MessageBox.Show(this, "请先选择产品。", "模型导入", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using (OpenFileDialog dialog = new OpenFileDialog { Filter="ONNX 模型 (*.onnx)|*.onnx", Multiselect=false, Title="选择 ONNX 模型" })
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

        public void RefreshTrainingJobs() { RefreshModels(); }

        private void RefreshModels()
        {
            if (!runtimeInitialized || IsDisposed) return;
            dgvModelLibrary.Rows.Clear();
            long productId = AppSession.CurrentProductId;
            if (productId <= 0) { txtTrainingLog.Text = "请先选择产品，再导入该产品使用的 ONNX 模型。"; return; }
            foreach (InferenceModel model in AppServices.Models.GetModels(productId))
            {
                int index = dgvModelLibrary.Rows.Add(model.Version, model.ModelType, model.InputWidth + " × " + model.InputHeight,
                    model.CreatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), model.ConfidenceThreshold.ToString("0.00"),
                    model.IsActive ? "已启用" : "未启用", model.Sha256 == null ? "" : model.Sha256.Substring(0, Math.Min(12, model.Sha256.Length)) + "…", model.Labels);
                dgvModelLibrary.Rows[index].Tag = model;
            }
        }
    }
}
