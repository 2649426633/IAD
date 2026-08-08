using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace IAD.Pages
{
    partial class DashboardPage
    {
        private IContainer components = null;
        private TableLayoutPanel rootLayout;
        private GroupBox grpProcess;
        private TableLayoutPanel processLayout;
        private Label step1;
        private Label arrow1;
        private Label step2;
        private Label arrow2;
        private Label step3;
        private Label arrow3;
        private Label step4;
        private Label arrow4;
        private Label step5;
        private Label arrow5;
        private Label step6;
        private Label arrow6;
        private Label step7;
        private Label arrow7;
        private Label step8;
        private TableLayoutPanel statsLayout;
        private GroupBox grpImages;
        private Label lblImagesValue;
        private Label lblImagesTime;
        private GroupBox grpAnnotated;
        private Label lblAnnotatedValue;
        private Label lblAnnotatedTime;
        private GroupBox grpDefectClasses;
        private Label lblDefectClassesValue;
        private Label lblDefectClassesTime;
        private GroupBox grpDefectInstances;
        private Label lblDefectInstancesValue;
        private Label lblDefectInstancesTime;
        private GroupBox grpCandidates;
        private Label lblCandidatesValue;
        private Label lblCandidatesTime;
        private TableLayoutPanel bottomLayout;
        private GroupBox grpRecentTraining;
        private DataGridView dgvTraining;
        private DataGridViewTextBoxColumn dgvTrainingCol1;
        private DataGridViewTextBoxColumn dgvTrainingCol2;
        private DataGridViewTextBoxColumn dgvTrainingCol3;
        private DataGridViewTextBoxColumn dgvTrainingCol4;
        private DataGridViewTextBoxColumn dgvTrainingCol5;
        private DataGridViewTextBoxColumn dgvTrainingCol6;
        private DataGridViewTextBoxColumn dgvTrainingCol7;
        private GroupBox grpRecentInspection;
        private DataGridView dgvInspection;
        private DataGridViewTextBoxColumn dgvInspectionCol1;
        private DataGridViewTextBoxColumn dgvInspectionCol2;
        private DataGridViewTextBoxColumn dgvInspectionCol3;
        private DataGridViewTextBoxColumn dgvInspectionCol4;
        private DataGridViewTextBoxColumn dgvInspectionCol5;
        private DataGridViewTextBoxColumn dgvInspectionCol6;
        private DataGridViewTextBoxColumn dgvInspectionCol7;
        private GroupBox grpPending;
        private TableLayoutPanel pendingLayout;
        private Label lblPending1;
        private Label lblPending1Value;
        private Label lblPending2;
        private Label lblPending2Value;
        private Label lblPending3;
        private Label lblPending3Value;
        private Label lblPending4;
        private Label lblPending4Value;
        private Label lblPending5;
        private Label lblPending5Value;
        private Label lblPending6;
        private Label lblPending6Value;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.grpProcess = new System.Windows.Forms.GroupBox();
            this.processLayout = new System.Windows.Forms.TableLayoutPanel();
            this.step1 = new System.Windows.Forms.Label();
            this.arrow1 = new System.Windows.Forms.Label();
            this.step2 = new System.Windows.Forms.Label();
            this.arrow2 = new System.Windows.Forms.Label();
            this.step3 = new System.Windows.Forms.Label();
            this.arrow3 = new System.Windows.Forms.Label();
            this.step4 = new System.Windows.Forms.Label();
            this.arrow4 = new System.Windows.Forms.Label();
            this.step5 = new System.Windows.Forms.Label();
            this.arrow5 = new System.Windows.Forms.Label();
            this.step6 = new System.Windows.Forms.Label();
            this.arrow6 = new System.Windows.Forms.Label();
            this.step7 = new System.Windows.Forms.Label();
            this.arrow7 = new System.Windows.Forms.Label();
            this.step8 = new System.Windows.Forms.Label();
            this.statsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.grpImages = new System.Windows.Forms.GroupBox();
            this.lblImagesValue = new System.Windows.Forms.Label();
            this.lblImagesTime = new System.Windows.Forms.Label();
            this.grpAnnotated = new System.Windows.Forms.GroupBox();
            this.lblAnnotatedValue = new System.Windows.Forms.Label();
            this.lblAnnotatedTime = new System.Windows.Forms.Label();
            this.grpDefectClasses = new System.Windows.Forms.GroupBox();
            this.lblDefectClassesValue = new System.Windows.Forms.Label();
            this.lblDefectClassesTime = new System.Windows.Forms.Label();
            this.grpDefectInstances = new System.Windows.Forms.GroupBox();
            this.lblDefectInstancesValue = new System.Windows.Forms.Label();
            this.lblDefectInstancesTime = new System.Windows.Forms.Label();
            this.grpCandidates = new System.Windows.Forms.GroupBox();
            this.lblCandidatesValue = new System.Windows.Forms.Label();
            this.lblCandidatesTime = new System.Windows.Forms.Label();
            this.bottomLayout = new System.Windows.Forms.TableLayoutPanel();
            this.grpRecentTraining = new System.Windows.Forms.GroupBox();
            this.dgvTraining = new System.Windows.Forms.DataGridView();
            this.dgvTrainingCol1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTrainingCol2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTrainingCol3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTrainingCol4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTrainingCol5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTrainingCol6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTrainingCol7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpRecentInspection = new System.Windows.Forms.GroupBox();
            this.dgvInspection = new System.Windows.Forms.DataGridView();
            this.dgvInspectionCol1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvInspectionCol2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvInspectionCol3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvInspectionCol4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvInspectionCol5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvInspectionCol6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvInspectionCol7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpPending = new System.Windows.Forms.GroupBox();
            this.pendingLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblPending1 = new System.Windows.Forms.Label();
            this.lblPending1Value = new System.Windows.Forms.Label();
            this.lblPending2 = new System.Windows.Forms.Label();
            this.lblPending2Value = new System.Windows.Forms.Label();
            this.lblPending3 = new System.Windows.Forms.Label();
            this.lblPending3Value = new System.Windows.Forms.Label();
            this.lblPending4 = new System.Windows.Forms.Label();
            this.lblPending4Value = new System.Windows.Forms.Label();
            this.lblPending5 = new System.Windows.Forms.Label();
            this.lblPending5Value = new System.Windows.Forms.Label();
            this.lblPending6 = new System.Windows.Forms.Label();
            this.lblPending6Value = new System.Windows.Forms.Label();
            this.rootLayout.SuspendLayout();
            this.grpProcess.SuspendLayout();
            this.processLayout.SuspendLayout();
            this.statsLayout.SuspendLayout();
            this.grpImages.SuspendLayout();
            this.grpAnnotated.SuspendLayout();
            this.grpDefectClasses.SuspendLayout();
            this.grpDefectInstances.SuspendLayout();
            this.grpCandidates.SuspendLayout();
            this.bottomLayout.SuspendLayout();
            this.grpRecentTraining.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTraining)).BeginInit();
            this.grpRecentInspection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInspection)).BeginInit();
            this.grpPending.SuspendLayout();
            this.pendingLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // rootLayout
            // 
            this.rootLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.grpProcess, 0, 0);
            this.rootLayout.Controls.Add(this.statsLayout, 0, 1);
            this.rootLayout.Controls.Add(this.bottomLayout, 0, 2);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(21, 22);
            this.rootLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.RowCount = 3;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 272F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 211F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Size = new System.Drawing.Size(2073, 1274);
            this.rootLayout.TabIndex = 0;
            // 
            // grpProcess
            // 
            this.grpProcess.BackColor = System.Drawing.Color.White;
            this.grpProcess.Controls.Add(this.processLayout);
            this.grpProcess.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpProcess.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.grpProcess.Location = new System.Drawing.Point(4, 5);
            this.grpProcess.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpProcess.Name = "grpProcess";
            this.grpProcess.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpProcess.Size = new System.Drawing.Size(2065, 262);
            this.grpProcess.TabIndex = 0;
            this.grpProcess.TabStop = false;
            this.grpProcess.Text = "产线流程进度";
            // 
            // processLayout
            // 
            this.processLayout.ColumnCount = 15;
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11F));
            this.processLayout.Controls.Add(this.step1, 0, 0);
            this.processLayout.Controls.Add(this.arrow1, 1, 0);
            this.processLayout.Controls.Add(this.step2, 2, 0);
            this.processLayout.Controls.Add(this.arrow2, 3, 0);
            this.processLayout.Controls.Add(this.step3, 4, 0);
            this.processLayout.Controls.Add(this.arrow3, 5, 0);
            this.processLayout.Controls.Add(this.step4, 6, 0);
            this.processLayout.Controls.Add(this.arrow4, 7, 0);
            this.processLayout.Controls.Add(this.step5, 8, 0);
            this.processLayout.Controls.Add(this.arrow5, 9, 0);
            this.processLayout.Controls.Add(this.step6, 10, 0);
            this.processLayout.Controls.Add(this.arrow6, 11, 0);
            this.processLayout.Controls.Add(this.step7, 12, 0);
            this.processLayout.Controls.Add(this.arrow7, 13, 0);
            this.processLayout.Controls.Add(this.step8, 14, 0);
            this.processLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.processLayout.Location = new System.Drawing.Point(4, 41);
            this.processLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.processLayout.Name = "processLayout";
            this.processLayout.RowCount = 1;
            this.processLayout.Size = new System.Drawing.Size(2057, 216);
            this.processLayout.TabIndex = 0;
            // 
            // step1
            // 
            this.step1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.step1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.step1.Location = new System.Drawing.Point(4, 0);
            this.step1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.step1.Name = "step1";
            this.step1.Size = new System.Drawing.Size(213, 216);
            this.step1.TabIndex = 0;
            this.step1.Text = "1\r\n产品模板建立\r\n已完成\r\n2025-03-01";
            this.step1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // arrow1
            // 
            this.arrow1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.arrow1.Location = new System.Drawing.Point(225, 0);
            this.arrow1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.arrow1.Name = "arrow1";
            this.arrow1.Size = new System.Drawing.Size(32, 216);
            this.arrow1.TabIndex = 1;
            this.arrow1.Text = "→";
            this.arrow1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // step2
            // 
            this.step2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.step2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.step2.Location = new System.Drawing.Point(265, 0);
            this.step2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.step2.Name = "step2";
            this.step2.Size = new System.Drawing.Size(213, 216);
            this.step2.TabIndex = 2;
            this.step2.Text = "2\r\n标注\r\n已完成\r\n2025-03-02";
            this.step2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // arrow2
            // 
            this.arrow2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.arrow2.Location = new System.Drawing.Point(486, 0);
            this.arrow2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.arrow2.Name = "arrow2";
            this.arrow2.Size = new System.Drawing.Size(32, 216);
            this.arrow2.TabIndex = 3;
            this.arrow2.Text = "→";
            this.arrow2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // step3
            // 
            this.step3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.step3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.step3.Location = new System.Drawing.Point(526, 0);
            this.step3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.step3.Name = "step3";
            this.step3.Size = new System.Drawing.Size(213, 216);
            this.step3.TabIndex = 4;
            this.step3.Text = "3\r\n少样本扩标\r\n已完成\r\n2025-03-03";
            this.step3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // arrow3
            // 
            this.arrow3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.arrow3.Location = new System.Drawing.Point(747, 0);
            this.arrow3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.arrow3.Name = "arrow3";
            this.arrow3.Size = new System.Drawing.Size(32, 216);
            this.arrow3.TabIndex = 5;
            this.arrow3.Text = "→";
            this.arrow3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // step4
            // 
            this.step4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.step4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.step4.Location = new System.Drawing.Point(787, 0);
            this.step4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.step4.Name = "step4";
            this.step4.Size = new System.Drawing.Size(213, 216);
            this.step4.TabIndex = 6;
            this.step4.Text = "4\r\n数据质检\r\n已完成\r\n2025-03-04";
            this.step4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // arrow4
            // 
            this.arrow4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.arrow4.Location = new System.Drawing.Point(1008, 0);
            this.arrow4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.arrow4.Name = "arrow4";
            this.arrow4.Size = new System.Drawing.Size(32, 216);
            this.arrow4.TabIndex = 7;
            this.arrow4.Text = "→";
            this.arrow4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // step5
            // 
            this.step5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.step5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.step5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.step5.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.step5.Location = new System.Drawing.Point(1048, 0);
            this.step5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.step5.Name = "step5";
            this.step5.Size = new System.Drawing.Size(213, 216);
            this.step5.TabIndex = 8;
            this.step5.Text = "5\r\n训练\r\n进行中\r\n当前阶段";
            this.step5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // arrow5
            // 
            this.arrow5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.arrow5.Location = new System.Drawing.Point(1269, 0);
            this.arrow5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.arrow5.Name = "arrow5";
            this.arrow5.Size = new System.Drawing.Size(32, 216);
            this.arrow5.TabIndex = 9;
            this.arrow5.Text = "→";
            this.arrow5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // step6
            // 
            this.step6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.step6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.step6.Location = new System.Drawing.Point(1309, 0);
            this.step6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.step6.Name = "step6";
            this.step6.Size = new System.Drawing.Size(213, 216);
            this.step6.TabIndex = 10;
            this.step6.Text = "6\r\n验收\r\n待开始";
            this.step6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // arrow6
            // 
            this.arrow6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.arrow6.Location = new System.Drawing.Point(1530, 0);
            this.arrow6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.arrow6.Name = "arrow6";
            this.arrow6.Size = new System.Drawing.Size(32, 216);
            this.arrow6.TabIndex = 11;
            this.arrow6.Text = "→";
            this.arrow6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // step7
            // 
            this.step7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.step7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.step7.Location = new System.Drawing.Point(1570, 0);
            this.step7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.step7.Name = "step7";
            this.step7.Size = new System.Drawing.Size(213, 216);
            this.step7.TabIndex = 12;
            this.step7.Text = "7\r\n发布\r\n待开始";
            this.step7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // arrow7
            // 
            this.arrow7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.arrow7.Location = new System.Drawing.Point(1791, 0);
            this.arrow7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.arrow7.Name = "arrow7";
            this.arrow7.Size = new System.Drawing.Size(32, 216);
            this.arrow7.TabIndex = 13;
            this.arrow7.Text = "→";
            this.arrow7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // step8
            // 
            this.step8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.step8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.step8.Location = new System.Drawing.Point(1831, 0);
            this.step8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.step8.Name = "step8";
            this.step8.Size = new System.Drawing.Size(222, 216);
            this.step8.TabIndex = 14;
            this.step8.Text = "8\r\n检测\r\n待开始";
            this.step8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // statsLayout
            // 
            this.statsLayout.ColumnCount = 5;
            this.statsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.statsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.statsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.statsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.statsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.statsLayout.Controls.Add(this.grpImages, 0, 0);
            this.statsLayout.Controls.Add(this.grpAnnotated, 1, 0);
            this.statsLayout.Controls.Add(this.grpDefectClasses, 2, 0);
            this.statsLayout.Controls.Add(this.grpDefectInstances, 3, 0);
            this.statsLayout.Controls.Add(this.grpCandidates, 4, 0);
            this.statsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statsLayout.Location = new System.Drawing.Point(4, 277);
            this.statsLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.statsLayout.Name = "statsLayout";
            this.statsLayout.RowCount = 1;
            this.statsLayout.Size = new System.Drawing.Size(2065, 201);
            this.statsLayout.TabIndex = 1;
            // 
            // grpImages
            // 
            this.grpImages.Controls.Add(this.lblImagesValue);
            this.grpImages.Controls.Add(this.lblImagesTime);
            this.grpImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpImages.Location = new System.Drawing.Point(4, 5);
            this.grpImages.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpImages.Name = "grpImages";
            this.grpImages.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpImages.Size = new System.Drawing.Size(405, 192);
            this.grpImages.TabIndex = 0;
            this.grpImages.TabStop = false;
            this.grpImages.Text = "图片总数";
            // 
            // lblImagesValue
            // 
            this.lblImagesValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblImagesValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F);
            this.lblImagesValue.Location = new System.Drawing.Point(4, 33);
            this.lblImagesValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblImagesValue.Name = "lblImagesValue";
            this.lblImagesValue.Size = new System.Drawing.Size(397, 116);
            this.lblImagesValue.TabIndex = 0;
            this.lblImagesValue.Text = "128,560";
            this.lblImagesValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblImagesTime
            // 
            this.lblImagesTime.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblImagesTime.Location = new System.Drawing.Point(4, 149);
            this.lblImagesTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblImagesTime.Name = "lblImagesTime";
            this.lblImagesTime.Size = new System.Drawing.Size(397, 38);
            this.lblImagesTime.TabIndex = 1;
            this.lblImagesTime.Text = "数据截止：2025-05-16 24:00";
            this.lblImagesTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grpAnnotated
            // 
            this.grpAnnotated.Controls.Add(this.lblAnnotatedValue);
            this.grpAnnotated.Controls.Add(this.lblAnnotatedTime);
            this.grpAnnotated.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpAnnotated.Location = new System.Drawing.Point(417, 5);
            this.grpAnnotated.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpAnnotated.Name = "grpAnnotated";
            this.grpAnnotated.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpAnnotated.Size = new System.Drawing.Size(405, 192);
            this.grpAnnotated.TabIndex = 1;
            this.grpAnnotated.TabStop = false;
            this.grpAnnotated.Text = "已标注图片";
            // 
            // lblAnnotatedValue
            // 
            this.lblAnnotatedValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAnnotatedValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F);
            this.lblAnnotatedValue.Location = new System.Drawing.Point(4, 33);
            this.lblAnnotatedValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAnnotatedValue.Name = "lblAnnotatedValue";
            this.lblAnnotatedValue.Size = new System.Drawing.Size(397, 116);
            this.lblAnnotatedValue.TabIndex = 0;
            this.lblAnnotatedValue.Text = "98,732";
            this.lblAnnotatedValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAnnotatedTime
            // 
            this.lblAnnotatedTime.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblAnnotatedTime.Location = new System.Drawing.Point(4, 149);
            this.lblAnnotatedTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAnnotatedTime.Name = "lblAnnotatedTime";
            this.lblAnnotatedTime.Size = new System.Drawing.Size(397, 38);
            this.lblAnnotatedTime.TabIndex = 1;
            this.lblAnnotatedTime.Text = "数据截止：2025-05-16 24:00";
            this.lblAnnotatedTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grpDefectClasses
            // 
            this.grpDefectClasses.Controls.Add(this.lblDefectClassesValue);
            this.grpDefectClasses.Controls.Add(this.lblDefectClassesTime);
            this.grpDefectClasses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDefectClasses.Location = new System.Drawing.Point(830, 5);
            this.grpDefectClasses.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpDefectClasses.Name = "grpDefectClasses";
            this.grpDefectClasses.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpDefectClasses.Size = new System.Drawing.Size(405, 192);
            this.grpDefectClasses.TabIndex = 2;
            this.grpDefectClasses.TabStop = false;
            this.grpDefectClasses.Text = "缺陷类别数";
            // 
            // lblDefectClassesValue
            // 
            this.lblDefectClassesValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDefectClassesValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F);
            this.lblDefectClassesValue.Location = new System.Drawing.Point(4, 33);
            this.lblDefectClassesValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDefectClassesValue.Name = "lblDefectClassesValue";
            this.lblDefectClassesValue.Size = new System.Drawing.Size(397, 116);
            this.lblDefectClassesValue.TabIndex = 0;
            this.lblDefectClassesValue.Text = "42";
            this.lblDefectClassesValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDefectClassesTime
            // 
            this.lblDefectClassesTime.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDefectClassesTime.Location = new System.Drawing.Point(4, 149);
            this.lblDefectClassesTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDefectClassesTime.Name = "lblDefectClassesTime";
            this.lblDefectClassesTime.Size = new System.Drawing.Size(397, 38);
            this.lblDefectClassesTime.TabIndex = 1;
            this.lblDefectClassesTime.Text = "数据截止：2025-05-16 24:00";
            this.lblDefectClassesTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grpDefectInstances
            // 
            this.grpDefectInstances.Controls.Add(this.lblDefectInstancesValue);
            this.grpDefectInstances.Controls.Add(this.lblDefectInstancesTime);
            this.grpDefectInstances.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDefectInstances.Location = new System.Drawing.Point(1243, 5);
            this.grpDefectInstances.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpDefectInstances.Name = "grpDefectInstances";
            this.grpDefectInstances.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpDefectInstances.Size = new System.Drawing.Size(405, 192);
            this.grpDefectInstances.TabIndex = 3;
            this.grpDefectInstances.TabStop = false;
            this.grpDefectInstances.Text = "缺陷实例数";
            // 
            // lblDefectInstancesValue
            // 
            this.lblDefectInstancesValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDefectInstancesValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F);
            this.lblDefectInstancesValue.Location = new System.Drawing.Point(4, 33);
            this.lblDefectInstancesValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDefectInstancesValue.Name = "lblDefectInstancesValue";
            this.lblDefectInstancesValue.Size = new System.Drawing.Size(397, 116);
            this.lblDefectInstancesValue.TabIndex = 0;
            this.lblDefectInstancesValue.Text = "256,731";
            this.lblDefectInstancesValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDefectInstancesTime
            // 
            this.lblDefectInstancesTime.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDefectInstancesTime.Location = new System.Drawing.Point(4, 149);
            this.lblDefectInstancesTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDefectInstancesTime.Name = "lblDefectInstancesTime";
            this.lblDefectInstancesTime.Size = new System.Drawing.Size(397, 38);
            this.lblDefectInstancesTime.TabIndex = 1;
            this.lblDefectInstancesTime.Text = "数据截止：2025-05-16 24:00";
            this.lblDefectInstancesTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grpCandidates
            // 
            this.grpCandidates.Controls.Add(this.lblCandidatesValue);
            this.grpCandidates.Controls.Add(this.lblCandidatesTime);
            this.grpCandidates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCandidates.Location = new System.Drawing.Point(1656, 5);
            this.grpCandidates.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpCandidates.Name = "grpCandidates";
            this.grpCandidates.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpCandidates.Size = new System.Drawing.Size(405, 192);
            this.grpCandidates.TabIndex = 4;
            this.grpCandidates.TabStop = false;
            this.grpCandidates.Text = "待确认候选数";
            // 
            // lblCandidatesValue
            // 
            this.lblCandidatesValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCandidatesValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F);
            this.lblCandidatesValue.Location = new System.Drawing.Point(4, 33);
            this.lblCandidatesValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCandidatesValue.Name = "lblCandidatesValue";
            this.lblCandidatesValue.Size = new System.Drawing.Size(397, 116);
            this.lblCandidatesValue.TabIndex = 0;
            this.lblCandidatesValue.Text = "5,362";
            this.lblCandidatesValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCandidatesTime
            // 
            this.lblCandidatesTime.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblCandidatesTime.Location = new System.Drawing.Point(4, 149);
            this.lblCandidatesTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCandidatesTime.Name = "lblCandidatesTime";
            this.lblCandidatesTime.Size = new System.Drawing.Size(397, 38);
            this.lblCandidatesTime.TabIndex = 1;
            this.lblCandidatesTime.Text = "数据截止：2025-05-16 24:00";
            this.lblCandidatesTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bottomLayout
            // 
            this.bottomLayout.ColumnCount = 3;
            this.bottomLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.bottomLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.bottomLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24F));
            this.bottomLayout.Controls.Add(this.grpRecentTraining, 0, 0);
            this.bottomLayout.Controls.Add(this.grpRecentInspection, 1, 0);
            this.bottomLayout.Controls.Add(this.grpPending, 2, 0);
            this.bottomLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomLayout.Location = new System.Drawing.Point(4, 488);
            this.bottomLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bottomLayout.Name = "bottomLayout";
            this.bottomLayout.RowCount = 1;
            this.bottomLayout.Size = new System.Drawing.Size(2065, 781);
            this.bottomLayout.TabIndex = 2;
            // 
            // grpRecentTraining
            // 
            this.grpRecentTraining.Controls.Add(this.dgvTraining);
            this.grpRecentTraining.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpRecentTraining.Location = new System.Drawing.Point(4, 5);
            this.grpRecentTraining.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpRecentTraining.Name = "grpRecentTraining";
            this.grpRecentTraining.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpRecentTraining.Size = new System.Drawing.Size(776, 771);
            this.grpRecentTraining.TabIndex = 0;
            this.grpRecentTraining.TabStop = false;
            this.grpRecentTraining.Text = "最近训练";
            // 
            // dgvTraining
            // 
            this.dgvTraining.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTraining.ColumnHeadersHeight = 46;
            this.dgvTraining.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvTrainingCol1,
            this.dgvTrainingCol2,
            this.dgvTrainingCol3,
            this.dgvTrainingCol4,
            this.dgvTrainingCol5,
            this.dgvTrainingCol6,
            this.dgvTrainingCol7});
            this.dgvTraining.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTraining.Location = new System.Drawing.Point(4, 33);
            this.dgvTraining.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dgvTraining.Name = "dgvTraining";
            this.dgvTraining.ReadOnly = true;
            this.dgvTraining.RowHeadersVisible = false;
            this.dgvTraining.RowHeadersWidth = 82;
            this.dgvTraining.Size = new System.Drawing.Size(768, 733);
            this.dgvTraining.TabIndex = 0;
            // 
            // dgvTrainingCol1
            // 
            this.dgvTrainingCol1.HeaderText = "训练批次";
            this.dgvTrainingCol1.MinimumWidth = 10;
            this.dgvTrainingCol1.Name = "dgvTrainingCol1";
            this.dgvTrainingCol1.ReadOnly = true;
            // 
            // dgvTrainingCol2
            // 
            this.dgvTrainingCol2.HeaderText = "模型";
            this.dgvTrainingCol2.MinimumWidth = 10;
            this.dgvTrainingCol2.Name = "dgvTrainingCol2";
            this.dgvTrainingCol2.ReadOnly = true;
            // 
            // dgvTrainingCol3
            // 
            this.dgvTrainingCol3.HeaderText = "数据集";
            this.dgvTrainingCol3.MinimumWidth = 10;
            this.dgvTrainingCol3.Name = "dgvTrainingCol3";
            this.dgvTrainingCol3.ReadOnly = true;
            // 
            // dgvTrainingCol4
            // 
            this.dgvTrainingCol4.HeaderText = "状态";
            this.dgvTrainingCol4.MinimumWidth = 10;
            this.dgvTrainingCol4.Name = "dgvTrainingCol4";
            this.dgvTrainingCol4.ReadOnly = true;
            // 
            // dgvTrainingCol5
            // 
            this.dgvTrainingCol5.HeaderText = "F1";
            this.dgvTrainingCol5.MinimumWidth = 10;
            this.dgvTrainingCol5.Name = "dgvTrainingCol5";
            this.dgvTrainingCol5.ReadOnly = true;
            // 
            // dgvTrainingCol6
            // 
            this.dgvTrainingCol6.HeaderText = "召回率";
            this.dgvTrainingCol6.MinimumWidth = 10;
            this.dgvTrainingCol6.Name = "dgvTrainingCol6";
            this.dgvTrainingCol6.ReadOnly = true;
            // 
            // dgvTrainingCol7
            // 
            this.dgvTrainingCol7.HeaderText = "日期";
            this.dgvTrainingCol7.MinimumWidth = 10;
            this.dgvTrainingCol7.Name = "dgvTrainingCol7";
            this.dgvTrainingCol7.ReadOnly = true;
            // 
            // grpRecentInspection
            // 
            this.grpRecentInspection.Controls.Add(this.dgvInspection);
            this.grpRecentInspection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpRecentInspection.Location = new System.Drawing.Point(788, 5);
            this.grpRecentInspection.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpRecentInspection.Name = "grpRecentInspection";
            this.grpRecentInspection.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpRecentInspection.Size = new System.Drawing.Size(776, 771);
            this.grpRecentInspection.TabIndex = 1;
            this.grpRecentInspection.TabStop = false;
            this.grpRecentInspection.Text = "最近检测";
            // 
            // dgvInspection
            // 
            this.dgvInspection.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvInspection.ColumnHeadersHeight = 46;
            this.dgvInspection.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvInspectionCol1,
            this.dgvInspectionCol2,
            this.dgvInspectionCol3,
            this.dgvInspectionCol4,
            this.dgvInspectionCol5,
            this.dgvInspectionCol6,
            this.dgvInspectionCol7});
            this.dgvInspection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvInspection.Location = new System.Drawing.Point(4, 33);
            this.dgvInspection.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dgvInspection.Name = "dgvInspection";
            this.dgvInspection.ReadOnly = true;
            this.dgvInspection.RowHeadersVisible = false;
            this.dgvInspection.RowHeadersWidth = 82;
            this.dgvInspection.Size = new System.Drawing.Size(768, 733);
            this.dgvInspection.TabIndex = 0;
            // 
            // dgvInspectionCol1
            // 
            this.dgvInspectionCol1.HeaderText = "检测批次";
            this.dgvInspectionCol1.MinimumWidth = 10;
            this.dgvInspectionCol1.Name = "dgvInspectionCol1";
            this.dgvInspectionCol1.ReadOnly = true;
            // 
            // dgvInspectionCol2
            // 
            this.dgvInspectionCol2.HeaderText = "产品批次";
            this.dgvInspectionCol2.MinimumWidth = 10;
            this.dgvInspectionCol2.Name = "dgvInspectionCol2";
            this.dgvInspectionCol2.ReadOnly = true;
            // 
            // dgvInspectionCol3
            // 
            this.dgvInspectionCol3.HeaderText = "工位";
            this.dgvInspectionCol3.MinimumWidth = 10;
            this.dgvInspectionCol3.Name = "dgvInspectionCol3";
            this.dgvInspectionCol3.ReadOnly = true;
            // 
            // dgvInspectionCol4
            // 
            this.dgvInspectionCol4.HeaderText = "检测数";
            this.dgvInspectionCol4.MinimumWidth = 10;
            this.dgvInspectionCol4.Name = "dgvInspectionCol4";
            this.dgvInspectionCol4.ReadOnly = true;
            // 
            // dgvInspectionCol5
            // 
            this.dgvInspectionCol5.HeaderText = "NG数";
            this.dgvInspectionCol5.MinimumWidth = 10;
            this.dgvInspectionCol5.Name = "dgvInspectionCol5";
            this.dgvInspectionCol5.ReadOnly = true;
            // 
            // dgvInspectionCol6
            // 
            this.dgvInspectionCol6.HeaderText = "NG率";
            this.dgvInspectionCol6.MinimumWidth = 10;
            this.dgvInspectionCol6.Name = "dgvInspectionCol6";
            this.dgvInspectionCol6.ReadOnly = true;
            // 
            // dgvInspectionCol7
            // 
            this.dgvInspectionCol7.HeaderText = "时间";
            this.dgvInspectionCol7.MinimumWidth = 10;
            this.dgvInspectionCol7.Name = "dgvInspectionCol7";
            this.dgvInspectionCol7.ReadOnly = true;
            // 
            // grpPending
            // 
            this.grpPending.Controls.Add(this.pendingLayout);
            this.grpPending.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpPending.Location = new System.Drawing.Point(1572, 5);
            this.grpPending.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpPending.Name = "grpPending";
            this.grpPending.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpPending.Size = new System.Drawing.Size(489, 771);
            this.grpPending.TabIndex = 2;
            this.grpPending.TabStop = false;
            this.grpPending.Text = "待处理事项";
            // 
            // pendingLayout
            // 
            this.pendingLayout.ColumnCount = 2;
            this.pendingLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 72F));
            this.pendingLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.pendingLayout.Controls.Add(this.lblPending1, 0, 0);
            this.pendingLayout.Controls.Add(this.lblPending1Value, 1, 0);
            this.pendingLayout.Controls.Add(this.lblPending2, 0, 1);
            this.pendingLayout.Controls.Add(this.lblPending2Value, 1, 1);
            this.pendingLayout.Controls.Add(this.lblPending3Value, 1, 2);
            this.pendingLayout.Controls.Add(this.lblPending4Value, 1, 3);
            this.pendingLayout.Controls.Add(this.lblPending5, 0, 4);
            this.pendingLayout.Controls.Add(this.lblPending5Value, 1, 4);
            this.pendingLayout.Controls.Add(this.lblPending6, 0, 5);
            this.pendingLayout.Controls.Add(this.lblPending6Value, 1, 5);
            this.pendingLayout.Controls.Add(this.lblPending3, 0, 2);
            this.pendingLayout.Controls.Add(this.lblPending4, 0, 3);
            this.pendingLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pendingLayout.Location = new System.Drawing.Point(4, 33);
            this.pendingLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pendingLayout.Name = "pendingLayout";
            this.pendingLayout.RowCount = 6;
            this.pendingLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pendingLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pendingLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pendingLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pendingLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pendingLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pendingLayout.Size = new System.Drawing.Size(481, 733);
            this.pendingLayout.TabIndex = 0;
            // 
            // lblPending1
            // 
            this.lblPending1.Location = new System.Drawing.Point(4, 0);
            this.lblPending1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPending1.Name = "lblPending1";
            this.lblPending1.Size = new System.Drawing.Size(178, 20);
            this.lblPending1.TabIndex = 0;
            this.lblPending1.Text = "待确认候选";
            // 
            // lblPending1Value
            // 
            this.lblPending1Value.Location = new System.Drawing.Point(350, 0);
            this.lblPending1Value.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPending1Value.Name = "lblPending1Value";
            this.lblPending1Value.Size = new System.Drawing.Size(8, 20);
            this.lblPending1Value.TabIndex = 1;
            this.lblPending1Value.Text = "5,362";
            // 
            // lblPending2
            // 
            this.lblPending2.Location = new System.Drawing.Point(4, 20);
            this.lblPending2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPending2.Name = "lblPending2";
            this.lblPending2.Size = new System.Drawing.Size(160, 20);
            this.lblPending2.TabIndex = 2;
            this.lblPending2.Text = "待人工确认标注";
            // 
            // lblPending2Value
            // 
            this.lblPending2Value.Location = new System.Drawing.Point(350, 20);
            this.lblPending2Value.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPending2Value.Name = "lblPending2Value";
            this.lblPending2Value.Size = new System.Drawing.Size(8, 20);
            this.lblPending2Value.TabIndex = 3;
            this.lblPending2Value.Text = "3,128";
            // 
            // lblPending3
            // 
            this.lblPending3.Location = new System.Drawing.Point(4, 40);
            this.lblPending3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPending3.Name = "lblPending3";
            this.lblPending3.Size = new System.Drawing.Size(178, 20);
            this.lblPending3.TabIndex = 4;
            this.lblPending3.Text = "待验收模型";
            // 
            // lblPending3Value
            // 
            this.lblPending3Value.Location = new System.Drawing.Point(350, 40);
            this.lblPending3Value.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPending3Value.Name = "lblPending3Value";
            this.lblPending3Value.Size = new System.Drawing.Size(8, 20);
            this.lblPending3Value.TabIndex = 5;
            this.lblPending3Value.Text = "2";
            // 
            // lblPending4
            // 
            this.lblPending4.Location = new System.Drawing.Point(4, 60);
            this.lblPending4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPending4.Name = "lblPending4";
            this.lblPending4.Size = new System.Drawing.Size(247, 20);
            this.lblPending4.TabIndex = 6;
            this.lblPending4.Text = "待复核检测结果";
            // 
            // lblPending4Value
            // 
            this.lblPending4Value.Location = new System.Drawing.Point(350, 60);
            this.lblPending4Value.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPending4Value.Name = "lblPending4Value";
            this.lblPending4Value.Size = new System.Drawing.Size(8, 20);
            this.lblPending4Value.TabIndex = 7;
            this.lblPending4Value.Text = "7,812";
            // 
            // lblPending5
            // 
            this.lblPending5.Location = new System.Drawing.Point(4, 80);
            this.lblPending5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPending5.Name = "lblPending5";
            this.lblPending5.Size = new System.Drawing.Size(178, 20);
            this.lblPending5.TabIndex = 8;
            this.lblPending5.Text = "待发布模型";
            // 
            // lblPending5Value
            // 
            this.lblPending5Value.Location = new System.Drawing.Point(350, 80);
            this.lblPending5Value.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPending5Value.Name = "lblPending5Value";
            this.lblPending5Value.Size = new System.Drawing.Size(8, 20);
            this.lblPending5Value.TabIndex = 9;
            this.lblPending5Value.Text = "1";
            // 
            // lblPending6
            // 
            this.lblPending6.Location = new System.Drawing.Point(4, 100);
            this.lblPending6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPending6.Name = "lblPending6";
            this.lblPending6.Size = new System.Drawing.Size(178, 37);
            this.lblPending6.TabIndex = 10;
            this.lblPending6.Text = "异常检测告警";
            // 
            // lblPending6Value
            // 
            this.lblPending6Value.Location = new System.Drawing.Point(350, 100);
            this.lblPending6Value.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPending6Value.Name = "lblPending6Value";
            this.lblPending6Value.Size = new System.Drawing.Size(8, 37);
            this.lblPending6Value.TabIndex = 11;
            this.lblPending6Value.Text = "3";
            // 
            // DashboardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.Controls.Add(this.rootLayout);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "DashboardPage";
            this.Padding = new System.Windows.Forms.Padding(21, 22, 6, 16);
            this.Size = new System.Drawing.Size(2100, 1312);
            this.rootLayout.ResumeLayout(false);
            this.grpProcess.ResumeLayout(false);
            this.processLayout.ResumeLayout(false);
            this.statsLayout.ResumeLayout(false);
            this.grpImages.ResumeLayout(false);
            this.grpAnnotated.ResumeLayout(false);
            this.grpDefectClasses.ResumeLayout(false);
            this.grpDefectInstances.ResumeLayout(false);
            this.grpCandidates.ResumeLayout(false);
            this.bottomLayout.ResumeLayout(false);
            this.grpRecentTraining.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTraining)).EndInit();
            this.grpRecentInspection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvInspection)).EndInit();
            this.grpPending.ResumeLayout(false);
            this.pendingLayout.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
