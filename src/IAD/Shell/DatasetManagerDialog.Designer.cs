using System.Drawing;
using System.Windows.Forms;
using IAD.Models;

namespace IAD.Shell
{
    partial class DatasetManagerDialog
    {
        private TabControl tabs;
        private TabPage tabWorkflow;
        private TabPage tabVersions;
        private DataGridView dgvImages;
        private DataGridView dgvVersions;
        private Label lblSummary;
        private TextBox txtReviewComment;
        private NumericUpDown numTrain;
        private NumericUpDown numValidation;
        private NumericUpDown numSeed;
        private TextBox txtDestination;
        private CheckBox chkCoco;
        private CheckBox chkYolo;
        private CheckBox chkMasks;
        private CheckBox chkApprovedOnly;
        private CheckBox chkQualityGate;

        private void InitializeComponent()
        {
            this.tabs = new TabControl();
            this.tabWorkflow = new TabPage();
            this.tabVersions = new TabPage();
            this.dgvImages = CreateGrid();
            this.dgvVersions = CreateGrid();
            this.lblSummary = new Label();
            this.txtReviewComment = new TextBox();
            this.numTrain = CreatePercent(70);
            this.numValidation = CreatePercent(20);
            this.numSeed = new NumericUpDown { Minimum = 0, Maximum = 999999, Value = 42, Width = 80 };
            this.txtDestination = new TextBox { Width = 420 };
            this.chkCoco = new CheckBox { Text = "COCO", Checked = true, AutoSize = true };
            this.chkYolo = new CheckBox { Text = "YOLO", Checked = true, AutoSize = true };
            this.chkMasks = new CheckBox { Text = "Mask PNG", Checked = true, AutoSize = true };
            this.chkApprovedOnly = new CheckBox { Text = "仅导出已通过/正常", Checked = true, AutoSize = true };
            this.chkQualityGate = new CheckBox { Text = "导出前执行质量门禁", Checked = true, AutoSize = true };

            this.SuspendLayout();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.ClientSize = new Size(1120, 700);
            this.MinimumSize = new Size(900, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Microsoft YaHei UI", 9F);

            this.tabs.Dock = DockStyle.Fill;
            this.tabs.TabPages.Add(this.tabWorkflow);
            this.tabs.TabPages.Add(this.tabVersions);
            this.tabWorkflow.Text = "审核与划分";
            this.tabVersions.Text = "版本与导出";

            TableLayoutPanel workflow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(8) };
            workflow.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            workflow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            workflow.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            workflow.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            this.lblSummary.Dock = DockStyle.Fill;
            this.lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            workflow.Controls.Add(this.lblSummary, 0, 0);

            this.dgvImages.Dock = DockStyle.Fill;
            this.dgvImages.MultiSelect = true;
            this.dgvImages.Columns.Add("FileName", "文件");
            this.dgvImages.Columns.Add("Review", "审核状态");
            this.dgvImages.Columns.Add("Split", "划分");
            this.dgvImages.Columns.Add("Labels", "矢量 / Mask");
            this.dgvImages.Columns.Add("Score", "质量分");
            this.dgvImages.Columns.Add("Issue", "首要问题");
            this.dgvImages.Columns.Add("Reviewer", "复核人");
            this.dgvImages.Columns[0].FillWeight = 150F;
            this.dgvImages.Columns[5].FillWeight = 180F;
            workflow.Controls.Add(this.dgvImages, 0, 1);

            FlowLayoutPanel reviewBar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            reviewBar.Controls.Add(new Label { Text = "复核意见", AutoSize = true, Margin = new Padding(0, 8, 4, 0) });
            this.txtReviewComment.Width = 220;
            reviewBar.Controls.Add(this.txtReviewComment);
            reviewBar.Controls.Add(CreateButton("标记正常", delegate { ApplyReview(DatasetReviewStatus.Normal); }));
            reviewBar.Controls.Add(CreateButton("审核通过", delegate { ApplyReview(DatasetReviewStatus.Approved); }));
            reviewBar.Controls.Add(CreateButton("驳回", delegate { ApplyReview(DatasetReviewStatus.Rejected); }));
            reviewBar.Controls.Add(CreateButton("忽略", delegate { ApplyReview(DatasetReviewStatus.Ignored); }));
            reviewBar.Controls.Add(CreateButton("重置待审核", delegate { ApplyReview(DatasetReviewStatus.Pending); }));
            workflow.Controls.Add(reviewBar, 0, 2);

            FlowLayoutPanel splitBar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            splitBar.Controls.Add(CreateButton("设为 Train", delegate { ApplySplit(DatasetSplit.Train); }));
            splitBar.Controls.Add(CreateButton("设为 Validation", delegate { ApplySplit(DatasetSplit.Validation); }));
            splitBar.Controls.Add(CreateButton("设为 Test", delegate { ApplySplit(DatasetSplit.Test); }));
            splitBar.Controls.Add(new Label { Text = "自动划分 Train%", AutoSize = true, Margin = new Padding(14, 8, 2, 0) });
            splitBar.Controls.Add(this.numTrain);
            splitBar.Controls.Add(new Label { Text = "Val%", AutoSize = true, Margin = new Padding(6, 8, 2, 0) });
            splitBar.Controls.Add(this.numValidation);
            splitBar.Controls.Add(new Label { Text = "Seed", AutoSize = true, Margin = new Padding(6, 8, 2, 0) });
            splitBar.Controls.Add(this.numSeed);
            splitBar.Controls.Add(CreateButton("执行自动划分", delegate { AssignSplits(); }));
            workflow.Controls.Add(splitBar, 0, 3);
            this.tabWorkflow.Controls.Add(workflow);

            TableLayoutPanel versions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8) };
            versions.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            versions.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            versions.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            this.dgvVersions.Dock = DockStyle.Fill;
            this.dgvVersions.MultiSelect = true;
            this.dgvVersions.Columns.Add("Version", "版本");
            this.dgvVersions.Columns.Add("Definition", "产品定义");
            this.dgvVersions.Columns.Add("Images", "图片");
            this.dgvVersions.Columns.Add("Annotations", "矢量标注");
            this.dgvVersions.Columns.Add("Masks", "Mask");
            this.dgvVersions.Columns.Add("Created", "发布时间");
            this.dgvVersions.Columns.Add("Notes", "备注");
            this.dgvVersions.Columns[6].FillWeight = 160F;
            versions.Controls.Add(this.dgvVersions, 0, 0);

            FlowLayoutPanel pathBar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            pathBar.Controls.Add(new Label { Text = "导出目录", AutoSize = true, Margin = new Padding(0, 8, 4, 0) });
            pathBar.Controls.Add(this.txtDestination);
            pathBar.Controls.Add(CreateButton("浏览…", delegate { BrowseDestination(); }));
            pathBar.Controls.Add(this.chkCoco);
            pathBar.Controls.Add(this.chkYolo);
            pathBar.Controls.Add(this.chkMasks);
            versions.Controls.Add(pathBar, 0, 1);

            FlowLayoutPanel exportBar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            exportBar.Controls.Add(this.chkApprovedOnly);
            exportBar.Controls.Add(this.chkQualityGate);
            exportBar.Controls.Add(CreateButton("导入 COCO", delegate { ImportCoco(); }));
            exportBar.Controls.Add(CreateButton("导入 YOLO/Mask", delegate { ImportYolo(); }));
            exportBar.Controls.Add(CreateButton("导出当前工作集", delegate { ExportCurrent(); }));
            exportBar.Controls.Add(CreateButton("导出选中历史版本", delegate { ExportSelectedVersion(); }));
            exportBar.Controls.Add(CreateButton("比较两个版本", delegate { CompareSelectedVersions(); }));
            exportBar.Controls.Add(CreateButton("恢复选中版本", delegate { RestoreSelectedVersion(); }));
            exportBar.Controls.Add(CreateButton("刷新", delegate { RefreshAll(); }));
            versions.Controls.Add(exportBar, 0, 2);
            this.tabVersions.Controls.Add(versions);

            this.Controls.Add(this.tabs);
            this.ResumeLayout(false);
        }

        private static DataGridView CreateGrid()
        {
            return new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };
        }

        private static NumericUpDown CreatePercent(decimal value)
        {
            return new NumericUpDown { Minimum = 0, Maximum = 100, Value = value, Width = 58 };
        }

        private static Button CreateButton(string text, System.EventHandler click)
        {
            Button button = new Button { Text = text, AutoSize = true, Height = 30 };
            button.Click += click;
            return button;
        }
    }
}
