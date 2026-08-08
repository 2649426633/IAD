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
            this.components = new Container();
            this.rootLayout = new TableLayoutPanel();
            this.grpProcess = new GroupBox();
            this.processLayout = new TableLayoutPanel();
            this.step1 = new Label(); this.arrow1 = new Label(); this.step2 = new Label(); this.arrow2 = new Label();
            this.step3 = new Label(); this.arrow3 = new Label(); this.step4 = new Label(); this.arrow4 = new Label();
            this.step5 = new Label(); this.arrow5 = new Label(); this.step6 = new Label(); this.arrow6 = new Label();
            this.step7 = new Label(); this.arrow7 = new Label(); this.step8 = new Label();
            this.statsLayout = new TableLayoutPanel();
            this.grpImages = new GroupBox(); this.lblImagesValue = new Label(); this.lblImagesTime = new Label();
            this.grpAnnotated = new GroupBox(); this.lblAnnotatedValue = new Label(); this.lblAnnotatedTime = new Label();
            this.grpDefectClasses = new GroupBox(); this.lblDefectClassesValue = new Label(); this.lblDefectClassesTime = new Label();
            this.grpDefectInstances = new GroupBox(); this.lblDefectInstancesValue = new Label(); this.lblDefectInstancesTime = new Label();
            this.grpCandidates = new GroupBox(); this.lblCandidatesValue = new Label(); this.lblCandidatesTime = new Label();
            this.bottomLayout = new TableLayoutPanel();
            this.grpRecentTraining = new GroupBox(); this.dgvTraining = new DataGridView();
            this.dgvTrainingCol1 = new DataGridViewTextBoxColumn(); this.dgvTrainingCol2 = new DataGridViewTextBoxColumn(); this.dgvTrainingCol3 = new DataGridViewTextBoxColumn(); this.dgvTrainingCol4 = new DataGridViewTextBoxColumn(); this.dgvTrainingCol5 = new DataGridViewTextBoxColumn(); this.dgvTrainingCol6 = new DataGridViewTextBoxColumn(); this.dgvTrainingCol7 = new DataGridViewTextBoxColumn();
            this.grpRecentInspection = new GroupBox(); this.dgvInspection = new DataGridView();
            this.dgvInspectionCol1 = new DataGridViewTextBoxColumn(); this.dgvInspectionCol2 = new DataGridViewTextBoxColumn(); this.dgvInspectionCol3 = new DataGridViewTextBoxColumn(); this.dgvInspectionCol4 = new DataGridViewTextBoxColumn(); this.dgvInspectionCol5 = new DataGridViewTextBoxColumn(); this.dgvInspectionCol6 = new DataGridViewTextBoxColumn(); this.dgvInspectionCol7 = new DataGridViewTextBoxColumn();
            this.grpPending = new GroupBox(); this.pendingLayout = new TableLayoutPanel();
            this.lblPending1 = new Label(); this.lblPending1Value = new Label(); this.lblPending2 = new Label(); this.lblPending2Value = new Label(); this.lblPending3 = new Label(); this.lblPending3Value = new Label(); this.lblPending4 = new Label(); this.lblPending4Value = new Label(); this.lblPending5 = new Label(); this.lblPending5Value = new Label(); this.lblPending6 = new Label(); this.lblPending6Value = new Label();
            ((ISupportInitialize)(this.dgvTraining)).BeginInit();
            ((ISupportInitialize)(this.dgvInspection)).BeginInit();
            this.SuspendLayout();

            this.rootLayout.BackColor = Color.FromArgb(247, 247, 247);
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.rootLayout.Dock = DockStyle.Fill;
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.RowCount = 3;
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 170F));
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            this.grpProcess.BackColor = Color.White;
            this.grpProcess.Dock = DockStyle.Fill;
            this.grpProcess.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold);
            this.grpProcess.Name = "grpProcess";
            this.grpProcess.Text = "产线流程进度";
            this.grpProcess.Controls.Add(this.processLayout);
            this.processLayout.ColumnCount = 15;
            this.processLayout.Dock = DockStyle.Fill;
            this.processLayout.Name = "processLayout";
            this.processLayout.RowCount = 1;
            this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11F)); this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11F)); this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11F)); this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11F)); this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11F)); this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11F)); this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11F)); this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 2F));
            this.processLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11F));
            this.step1.BorderStyle = BorderStyle.FixedSingle; this.step1.Dock = DockStyle.Fill; this.step1.Name = "step1"; this.step1.Text = "1\r\n产品模板建立\r\n已完成\r\n2025-03-01"; this.step1.TextAlign = ContentAlignment.MiddleCenter;
            this.step2.BorderStyle = BorderStyle.FixedSingle; this.step2.Dock = DockStyle.Fill; this.step2.Name = "step2"; this.step2.Text = "2\r\n标注\r\n已完成\r\n2025-03-02"; this.step2.TextAlign = ContentAlignment.MiddleCenter;
            this.step3.BorderStyle = BorderStyle.FixedSingle; this.step3.Dock = DockStyle.Fill; this.step3.Name = "step3"; this.step3.Text = "3\r\n少样本扩标\r\n已完成\r\n2025-03-03"; this.step3.TextAlign = ContentAlignment.MiddleCenter;
            this.step4.BorderStyle = BorderStyle.FixedSingle; this.step4.Dock = DockStyle.Fill; this.step4.Name = "step4"; this.step4.Text = "4\r\n数据质检\r\n已完成\r\n2025-03-04"; this.step4.TextAlign = ContentAlignment.MiddleCenter;
            this.step5.BackColor = Color.FromArgb(242, 242, 242); this.step5.BorderStyle = BorderStyle.FixedSingle; this.step5.Dock = DockStyle.Fill; this.step5.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold); this.step5.Name = "step5"; this.step5.Text = "5\r\n训练\r\n进行中\r\n当前阶段"; this.step5.TextAlign = ContentAlignment.MiddleCenter;
            this.step6.BorderStyle = BorderStyle.FixedSingle; this.step6.Dock = DockStyle.Fill; this.step6.Name = "step6"; this.step6.Text = "6\r\n验收\r\n待开始"; this.step6.TextAlign = ContentAlignment.MiddleCenter;
            this.step7.BorderStyle = BorderStyle.FixedSingle; this.step7.Dock = DockStyle.Fill; this.step7.Name = "step7"; this.step7.Text = "7\r\n发布\r\n待开始"; this.step7.TextAlign = ContentAlignment.MiddleCenter;
            this.step8.BorderStyle = BorderStyle.FixedSingle; this.step8.Dock = DockStyle.Fill; this.step8.Name = "step8"; this.step8.Text = "8\r\n检测\r\n待开始"; this.step8.TextAlign = ContentAlignment.MiddleCenter;
            this.arrow1.Dock = DockStyle.Fill; this.arrow1.Name = "arrow1"; this.arrow1.Text = "→"; this.arrow1.TextAlign = ContentAlignment.MiddleCenter;
            this.arrow2.Dock = DockStyle.Fill; this.arrow2.Name = "arrow2"; this.arrow2.Text = "→"; this.arrow2.TextAlign = ContentAlignment.MiddleCenter;
            this.arrow3.Dock = DockStyle.Fill; this.arrow3.Name = "arrow3"; this.arrow3.Text = "→"; this.arrow3.TextAlign = ContentAlignment.MiddleCenter;
            this.arrow4.Dock = DockStyle.Fill; this.arrow4.Name = "arrow4"; this.arrow4.Text = "→"; this.arrow4.TextAlign = ContentAlignment.MiddleCenter;
            this.arrow5.Dock = DockStyle.Fill; this.arrow5.Name = "arrow5"; this.arrow5.Text = "→"; this.arrow5.TextAlign = ContentAlignment.MiddleCenter;
            this.arrow6.Dock = DockStyle.Fill; this.arrow6.Name = "arrow6"; this.arrow6.Text = "→"; this.arrow6.TextAlign = ContentAlignment.MiddleCenter;
            this.arrow7.Dock = DockStyle.Fill; this.arrow7.Name = "arrow7"; this.arrow7.Text = "→"; this.arrow7.TextAlign = ContentAlignment.MiddleCenter;
            this.processLayout.Controls.Add(this.step1, 0, 0); this.processLayout.Controls.Add(this.arrow1, 1, 0); this.processLayout.Controls.Add(this.step2, 2, 0); this.processLayout.Controls.Add(this.arrow2, 3, 0); this.processLayout.Controls.Add(this.step3, 4, 0); this.processLayout.Controls.Add(this.arrow3, 5, 0); this.processLayout.Controls.Add(this.step4, 6, 0); this.processLayout.Controls.Add(this.arrow4, 7, 0); this.processLayout.Controls.Add(this.step5, 8, 0); this.processLayout.Controls.Add(this.arrow5, 9, 0); this.processLayout.Controls.Add(this.step6, 10, 0); this.processLayout.Controls.Add(this.arrow6, 11, 0); this.processLayout.Controls.Add(this.step7, 12, 0); this.processLayout.Controls.Add(this.arrow7, 13, 0); this.processLayout.Controls.Add(this.step8, 14, 0);

            this.statsLayout.ColumnCount = 5; this.statsLayout.Dock = DockStyle.Fill; this.statsLayout.Name = "statsLayout"; this.statsLayout.RowCount = 1;
            this.statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); this.statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); this.statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); this.statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); this.statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            this.grpImages.Dock = DockStyle.Fill; this.grpImages.Name = "grpImages"; this.grpImages.Text = "图片总数"; this.grpImages.Controls.Add(this.lblImagesValue); this.grpImages.Controls.Add(this.lblImagesTime);
            this.lblImagesValue.Dock = DockStyle.Fill; this.lblImagesValue.Font = new Font("Microsoft YaHei UI", 18F); this.lblImagesValue.Name = "lblImagesValue"; this.lblImagesValue.Text = "128,560"; this.lblImagesValue.TextAlign = ContentAlignment.MiddleCenter;
            this.lblImagesTime.Dock = DockStyle.Bottom; this.lblImagesTime.Height = 24; this.lblImagesTime.Name = "lblImagesTime"; this.lblImagesTime.Text = "数据截止：2025-05-16 24:00"; this.lblImagesTime.TextAlign = ContentAlignment.MiddleCenter;
            this.grpAnnotated.Dock = DockStyle.Fill; this.grpAnnotated.Name = "grpAnnotated"; this.grpAnnotated.Text = "已标注图片"; this.grpAnnotated.Controls.Add(this.lblAnnotatedValue); this.grpAnnotated.Controls.Add(this.lblAnnotatedTime);
            this.lblAnnotatedValue.Dock = DockStyle.Fill; this.lblAnnotatedValue.Font = new Font("Microsoft YaHei UI", 18F); this.lblAnnotatedValue.Name = "lblAnnotatedValue"; this.lblAnnotatedValue.Text = "98,732"; this.lblAnnotatedValue.TextAlign = ContentAlignment.MiddleCenter;
            this.lblAnnotatedTime.Dock = DockStyle.Bottom; this.lblAnnotatedTime.Height = 24; this.lblAnnotatedTime.Name = "lblAnnotatedTime"; this.lblAnnotatedTime.Text = "数据截止：2025-05-16 24:00"; this.lblAnnotatedTime.TextAlign = ContentAlignment.MiddleCenter;
            this.grpDefectClasses.Dock = DockStyle.Fill; this.grpDefectClasses.Name = "grpDefectClasses"; this.grpDefectClasses.Text = "缺陷类别数"; this.grpDefectClasses.Controls.Add(this.lblDefectClassesValue); this.grpDefectClasses.Controls.Add(this.lblDefectClassesTime);
            this.lblDefectClassesValue.Dock = DockStyle.Fill; this.lblDefectClassesValue.Font = new Font("Microsoft YaHei UI", 18F); this.lblDefectClassesValue.Name = "lblDefectClassesValue"; this.lblDefectClassesValue.Text = "42"; this.lblDefectClassesValue.TextAlign = ContentAlignment.MiddleCenter;
            this.lblDefectClassesTime.Dock = DockStyle.Bottom; this.lblDefectClassesTime.Height = 24; this.lblDefectClassesTime.Name = "lblDefectClassesTime"; this.lblDefectClassesTime.Text = "数据截止：2025-05-16 24:00"; this.lblDefectClassesTime.TextAlign = ContentAlignment.MiddleCenter;
            this.grpDefectInstances.Dock = DockStyle.Fill; this.grpDefectInstances.Name = "grpDefectInstances"; this.grpDefectInstances.Text = "缺陷实例数"; this.grpDefectInstances.Controls.Add(this.lblDefectInstancesValue); this.grpDefectInstances.Controls.Add(this.lblDefectInstancesTime);
            this.lblDefectInstancesValue.Dock = DockStyle.Fill; this.lblDefectInstancesValue.Font = new Font("Microsoft YaHei UI", 18F); this.lblDefectInstancesValue.Name = "lblDefectInstancesValue"; this.lblDefectInstancesValue.Text = "256,731"; this.lblDefectInstancesValue.TextAlign = ContentAlignment.MiddleCenter;
            this.lblDefectInstancesTime.Dock = DockStyle.Bottom; this.lblDefectInstancesTime.Height = 24; this.lblDefectInstancesTime.Name = "lblDefectInstancesTime"; this.lblDefectInstancesTime.Text = "数据截止：2025-05-16 24:00"; this.lblDefectInstancesTime.TextAlign = ContentAlignment.MiddleCenter;
            this.grpCandidates.Dock = DockStyle.Fill; this.grpCandidates.Name = "grpCandidates"; this.grpCandidates.Text = "待确认候选数"; this.grpCandidates.Controls.Add(this.lblCandidatesValue); this.grpCandidates.Controls.Add(this.lblCandidatesTime);
            this.lblCandidatesValue.Dock = DockStyle.Fill; this.lblCandidatesValue.Font = new Font("Microsoft YaHei UI", 18F); this.lblCandidatesValue.Name = "lblCandidatesValue"; this.lblCandidatesValue.Text = "5,362"; this.lblCandidatesValue.TextAlign = ContentAlignment.MiddleCenter;
            this.lblCandidatesTime.Dock = DockStyle.Bottom; this.lblCandidatesTime.Height = 24; this.lblCandidatesTime.Name = "lblCandidatesTime"; this.lblCandidatesTime.Text = "数据截止：2025-05-16 24:00"; this.lblCandidatesTime.TextAlign = ContentAlignment.MiddleCenter;
            this.statsLayout.Controls.Add(this.grpImages, 0, 0); this.statsLayout.Controls.Add(this.grpAnnotated, 1, 0); this.statsLayout.Controls.Add(this.grpDefectClasses, 2, 0); this.statsLayout.Controls.Add(this.grpDefectInstances, 3, 0); this.statsLayout.Controls.Add(this.grpCandidates, 4, 0);

            this.bottomLayout.ColumnCount = 3; this.bottomLayout.Dock = DockStyle.Fill; this.bottomLayout.Name = "bottomLayout"; this.bottomLayout.RowCount = 1;
            this.bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F)); this.bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F)); this.bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            this.grpRecentTraining.Dock = DockStyle.Fill; this.grpRecentTraining.Name = "grpRecentTraining"; this.grpRecentTraining.Text = "最近训练"; this.grpRecentTraining.Controls.Add(this.dgvTraining);
            this.dgvTraining.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; this.dgvTraining.Dock = DockStyle.Fill; this.dgvTraining.Name = "dgvTraining"; this.dgvTraining.ReadOnly = true; this.dgvTraining.RowHeadersVisible = false;
            this.dgvTraining.Columns.AddRange(new DataGridViewColumn[] { this.dgvTrainingCol1, this.dgvTrainingCol2, this.dgvTrainingCol3, this.dgvTrainingCol4, this.dgvTrainingCol5, this.dgvTrainingCol6, this.dgvTrainingCol7 });
            this.dgvTrainingCol1.HeaderText = "训练批次"; this.dgvTrainingCol1.Name = "dgvTrainingCol1"; this.dgvTrainingCol2.HeaderText = "模型"; this.dgvTrainingCol2.Name = "dgvTrainingCol2"; this.dgvTrainingCol3.HeaderText = "数据集"; this.dgvTrainingCol3.Name = "dgvTrainingCol3"; this.dgvTrainingCol4.HeaderText = "状态"; this.dgvTrainingCol4.Name = "dgvTrainingCol4"; this.dgvTrainingCol5.HeaderText = "F1"; this.dgvTrainingCol5.Name = "dgvTrainingCol5"; this.dgvTrainingCol6.HeaderText = "召回率"; this.dgvTrainingCol6.Name = "dgvTrainingCol6"; this.dgvTrainingCol7.HeaderText = "日期"; this.dgvTrainingCol7.Name = "dgvTrainingCol7";
            this.grpRecentInspection.Dock = DockStyle.Fill; this.grpRecentInspection.Name = "grpRecentInspection"; this.grpRecentInspection.Text = "最近检测"; this.grpRecentInspection.Controls.Add(this.dgvInspection);
            this.dgvInspection.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; this.dgvInspection.Dock = DockStyle.Fill; this.dgvInspection.Name = "dgvInspection"; this.dgvInspection.ReadOnly = true; this.dgvInspection.RowHeadersVisible = false;
            this.dgvInspection.Columns.AddRange(new DataGridViewColumn[] { this.dgvInspectionCol1, this.dgvInspectionCol2, this.dgvInspectionCol3, this.dgvInspectionCol4, this.dgvInspectionCol5, this.dgvInspectionCol6, this.dgvInspectionCol7 });
            this.dgvInspectionCol1.HeaderText = "检测批次"; this.dgvInspectionCol1.Name = "dgvInspectionCol1"; this.dgvInspectionCol2.HeaderText = "产品批次"; this.dgvInspectionCol2.Name = "dgvInspectionCol2"; this.dgvInspectionCol3.HeaderText = "工位"; this.dgvInspectionCol3.Name = "dgvInspectionCol3"; this.dgvInspectionCol4.HeaderText = "检测数"; this.dgvInspectionCol4.Name = "dgvInspectionCol4"; this.dgvInspectionCol5.HeaderText = "NG数"; this.dgvInspectionCol5.Name = "dgvInspectionCol5"; this.dgvInspectionCol6.HeaderText = "NG率"; this.dgvInspectionCol6.Name = "dgvInspectionCol6"; this.dgvInspectionCol7.HeaderText = "时间"; this.dgvInspectionCol7.Name = "dgvInspectionCol7";
            this.grpPending.Dock = DockStyle.Fill; this.grpPending.Name = "grpPending"; this.grpPending.Text = "待处理事项"; this.grpPending.Controls.Add(this.pendingLayout);
            this.pendingLayout.ColumnCount = 2; this.pendingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72F)); this.pendingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F)); this.pendingLayout.Dock = DockStyle.Fill; this.pendingLayout.Name = "pendingLayout"; this.pendingLayout.RowCount = 6;
            this.lblPending1.Name = "lblPending1"; this.lblPending1.Text = "待确认候选"; this.lblPending1Value.Name = "lblPending1Value"; this.lblPending1Value.Text = "5,362";
            this.lblPending2.Name = "lblPending2"; this.lblPending2.Text = "待人工确认标注"; this.lblPending2Value.Name = "lblPending2Value"; this.lblPending2Value.Text = "3,128";
            this.lblPending3.Name = "lblPending3"; this.lblPending3.Text = "待验收模型"; this.lblPending3Value.Name = "lblPending3Value"; this.lblPending3Value.Text = "2";
            this.lblPending4.Name = "lblPending4"; this.lblPending4.Text = "待复核检测结果"; this.lblPending4Value.Name = "lblPending4Value"; this.lblPending4Value.Text = "7,812";
            this.lblPending5.Name = "lblPending5"; this.lblPending5.Text = "待发布模型"; this.lblPending5Value.Name = "lblPending5Value"; this.lblPending5Value.Text = "1";
            this.lblPending6.Name = "lblPending6"; this.lblPending6.Text = "异常检测告警"; this.lblPending6Value.Name = "lblPending6Value"; this.lblPending6Value.Text = "3";
            this.pendingLayout.Controls.Add(this.lblPending1, 0, 0); this.pendingLayout.Controls.Add(this.lblPending1Value, 1, 0); this.pendingLayout.Controls.Add(this.lblPending2, 0, 1); this.pendingLayout.Controls.Add(this.lblPending2Value, 1, 1); this.pendingLayout.Controls.Add(this.lblPending3, 0, 2); this.pendingLayout.Controls.Add(this.lblPending3Value, 1, 2); this.pendingLayout.Controls.Add(this.lblPending4, 0, 3); this.pendingLayout.Controls.Add(this.lblPending4Value, 1, 3); this.pendingLayout.Controls.Add(this.lblPending5, 0, 4); this.pendingLayout.Controls.Add(this.lblPending5Value, 1, 4); this.pendingLayout.Controls.Add(this.lblPending6, 0, 5); this.pendingLayout.Controls.Add(this.lblPending6Value, 1, 5);
            this.bottomLayout.Controls.Add(this.grpRecentTraining, 0, 0); this.bottomLayout.Controls.Add(this.grpRecentInspection, 1, 0); this.bottomLayout.Controls.Add(this.grpPending, 2, 0);

            this.rootLayout.Controls.Add(this.grpProcess, 0, 0); this.rootLayout.Controls.Add(this.statsLayout, 0, 1); this.rootLayout.Controls.Add(this.bottomLayout, 0, 2);
            this.AutoScaleDimensions = new SizeF(8F, 15F); this.AutoScaleMode = AutoScaleMode.Font; this.BackColor = Color.FromArgb(247, 247, 247); this.Controls.Add(this.rootLayout); this.Name = "DashboardPage"; this.Padding = new Padding(14, 14, 4, 10); this.Size = new Size(1400, 820);
            ((ISupportInitialize)(this.dgvTraining)).EndInit(); ((ISupportInitialize)(this.dgvInspection)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
