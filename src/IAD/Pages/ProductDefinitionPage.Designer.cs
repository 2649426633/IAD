using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace IAD.Pages
{
    partial class ProductDefinitionPage
    {
        private IContainer components = null;
        private TableLayoutPanel rootLayout;
        private TableLayoutPanel commandLayout;
        private FlowLayoutPanel commandButtons;
        private Button btnSave;
        private Button btnBuildTemplate;
        private Button btnTestLocalization;
        private Button btnVersion;
        private Label lblVersion;
        private GroupBox grpBasicInfo;
        private TableLayoutPanel basicLayout;
        private Label lblProductName;
        private TextBox txtProductName;
        private Label lblProductCode;
        private TextBox txtProductCode;
        private Label lblImageSize;
        private TextBox txtImageSize;
        private Label lblProductCount;
        private TextBox txtProductCount;
        private Label lblPose;
        private ComboBox cboPose;
        private Label lblAcquisition;
        private TextBox txtAcquisition;
        private TableLayoutPanel middleLayout;
        private GroupBox grpTemplate;
        private TableLayoutPanel templateLayout;
        private FlowLayoutPanel templateButtons;
        private Button btnImportReference;
        private Button btnRectangleRoi;
        private Button btnClearRoi;
        private Button btnFastMode;
        private Button btnFineMode;
        private Button btnAutoMode;
        private Panel pnlTemplateCanvas;
        private Label lblCanvasHint;
        private TableLayoutPanel templateFooter;
        private Label lblReferenceFile;
        private Label lblRoiState;
        private Label lblTemplateType;
        private Label lblLastScore;
        private TableLayoutPanel parameterStack;
        private GroupBox grpLocalization;
        private TableLayoutPanel localizationLayout;
        private Label lblLocalizationMethodKey;
        private ComboBox cboLocalizationMethod;
        private Label lblModelTypeKey;
        private ComboBox cboModelType;
        private Label lblMinScoreKey;
        private TextBox txtMinScore;
        private Label lblAngleRangeKey;
        private TextBox txtAngleRange;
        private Label lblScaleRangeKey;
        private TextBox txtScaleRange;
        private Label lblMatchCountKey;
        private TextBox txtMatchCount;
        private Label lblLastResultKey;
        private TextBox txtLastResult;
        private GroupBox grpCalibration;
        private TableLayoutPanel calibrationLayout;
        private Label lblPixelXKey;
        private TextBox txtPixelX;
        private Label lblPixelYKey;
        private TextBox txtPixelY;
        private Label lblLengthUnitKey;
        private ComboBox cboLengthUnit;
        private Label lblAreaUnitKey;
        private ComboBox cboAreaUnit;
        private Label lblCalibrationVersionKey;
        private TextBox txtCalibrationVersion;
        private Label lblCalibrationStateKey;
        private ComboBox cboCalibrationState;
        private GroupBox grpDefects;
        private TableLayoutPanel defectLayout;
        private FlowLayoutPanel defectButtons;
        private Button btnAddDefect;
        private Button btnEditDefect;
        private Button btnDeleteDefect;
        private Button btnToggleDefect;
        private Button btnImportDefects;
        private Button btnExportDefects;
        private DataGridView dgvDefects;
        private DataGridViewTextBoxColumn dgvDefectsCol1;
        private DataGridViewTextBoxColumn dgvDefectsCol2;
        private DataGridViewTextBoxColumn dgvDefectsCol3;
        private DataGridViewTextBoxColumn dgvDefectsCol4;
        private DataGridViewTextBoxColumn dgvDefectsCol5;
        private DataGridViewTextBoxColumn dgvDefectsCol6;
        private DataGridViewTextBoxColumn dgvDefectsCol7;
        private DataGridViewTextBoxColumn dgvDefectsCol8;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.commandLayout = new System.Windows.Forms.TableLayoutPanel();
            this.commandButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnBuildTemplate = new System.Windows.Forms.Button();
            this.btnTestLocalization = new System.Windows.Forms.Button();
            this.btnVersion = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.grpBasicInfo = new System.Windows.Forms.GroupBox();
            this.basicLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblProductName = new System.Windows.Forms.Label();
            this.txtProductName = new System.Windows.Forms.TextBox();
            this.lblProductCode = new System.Windows.Forms.Label();
            this.txtProductCode = new System.Windows.Forms.TextBox();
            this.lblImageSize = new System.Windows.Forms.Label();
            this.txtImageSize = new System.Windows.Forms.TextBox();
            this.lblProductCount = new System.Windows.Forms.Label();
            this.txtProductCount = new System.Windows.Forms.TextBox();
            this.lblPose = new System.Windows.Forms.Label();
            this.cboPose = new System.Windows.Forms.ComboBox();
            this.lblAcquisition = new System.Windows.Forms.Label();
            this.txtAcquisition = new System.Windows.Forms.TextBox();
            this.middleLayout = new System.Windows.Forms.TableLayoutPanel();
            this.grpTemplate = new System.Windows.Forms.GroupBox();
            this.templateLayout = new System.Windows.Forms.TableLayoutPanel();
            this.templateButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnImportReference = new System.Windows.Forms.Button();
            this.btnRectangleRoi = new System.Windows.Forms.Button();
            this.btnClearRoi = new System.Windows.Forms.Button();
            this.btnFastMode = new System.Windows.Forms.Button();
            this.btnFineMode = new System.Windows.Forms.Button();
            this.btnAutoMode = new System.Windows.Forms.Button();
            this.pnlTemplateCanvas = new System.Windows.Forms.Panel();
            this.lblCanvasHint = new System.Windows.Forms.Label();
            this.templateFooter = new System.Windows.Forms.TableLayoutPanel();
            this.lblReferenceFile = new System.Windows.Forms.Label();
            this.lblRoiState = new System.Windows.Forms.Label();
            this.lblTemplateType = new System.Windows.Forms.Label();
            this.lblLastScore = new System.Windows.Forms.Label();
            this.parameterStack = new System.Windows.Forms.TableLayoutPanel();
            this.grpLocalization = new System.Windows.Forms.GroupBox();
            this.localizationLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblLocalizationMethodKey = new System.Windows.Forms.Label();
            this.cboLocalizationMethod = new System.Windows.Forms.ComboBox();
            this.lblModelTypeKey = new System.Windows.Forms.Label();
            this.cboModelType = new System.Windows.Forms.ComboBox();
            this.lblMinScoreKey = new System.Windows.Forms.Label();
            this.txtMinScore = new System.Windows.Forms.TextBox();
            this.lblAngleRangeKey = new System.Windows.Forms.Label();
            this.txtAngleRange = new System.Windows.Forms.TextBox();
            this.lblScaleRangeKey = new System.Windows.Forms.Label();
            this.txtScaleRange = new System.Windows.Forms.TextBox();
            this.lblMatchCountKey = new System.Windows.Forms.Label();
            this.txtMatchCount = new System.Windows.Forms.TextBox();
            this.lblLastResultKey = new System.Windows.Forms.Label();
            this.txtLastResult = new System.Windows.Forms.TextBox();
            this.grpCalibration = new System.Windows.Forms.GroupBox();
            this.calibrationLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblPixelXKey = new System.Windows.Forms.Label();
            this.txtPixelX = new System.Windows.Forms.TextBox();
            this.lblPixelYKey = new System.Windows.Forms.Label();
            this.txtPixelY = new System.Windows.Forms.TextBox();
            this.lblLengthUnitKey = new System.Windows.Forms.Label();
            this.cboLengthUnit = new System.Windows.Forms.ComboBox();
            this.lblAreaUnitKey = new System.Windows.Forms.Label();
            this.cboAreaUnit = new System.Windows.Forms.ComboBox();
            this.lblCalibrationVersionKey = new System.Windows.Forms.Label();
            this.txtCalibrationVersion = new System.Windows.Forms.TextBox();
            this.lblCalibrationStateKey = new System.Windows.Forms.Label();
            this.cboCalibrationState = new System.Windows.Forms.ComboBox();
            this.grpDefects = new System.Windows.Forms.GroupBox();
            this.defectLayout = new System.Windows.Forms.TableLayoutPanel();
            this.defectButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAddDefect = new System.Windows.Forms.Button();
            this.btnEditDefect = new System.Windows.Forms.Button();
            this.btnDeleteDefect = new System.Windows.Forms.Button();
            this.btnToggleDefect = new System.Windows.Forms.Button();
            this.btnImportDefects = new System.Windows.Forms.Button();
            this.btnExportDefects = new System.Windows.Forms.Button();
            this.dgvDefects = new System.Windows.Forms.DataGridView();
            this.dgvDefectsCol1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvDefectsCol2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvDefectsCol3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvDefectsCol4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvDefectsCol5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvDefectsCol6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvDefectsCol7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvDefectsCol8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rootLayout.SuspendLayout();
            this.commandLayout.SuspendLayout();
            this.commandButtons.SuspendLayout();
            this.grpBasicInfo.SuspendLayout();
            this.basicLayout.SuspendLayout();
            this.middleLayout.SuspendLayout();
            this.grpTemplate.SuspendLayout();
            this.templateLayout.SuspendLayout();
            this.templateButtons.SuspendLayout();
            this.pnlTemplateCanvas.SuspendLayout();
            this.templateFooter.SuspendLayout();
            this.parameterStack.SuspendLayout();
            this.grpLocalization.SuspendLayout();
            this.localizationLayout.SuspendLayout();
            this.grpCalibration.SuspendLayout();
            this.calibrationLayout.SuspendLayout();
            this.grpDefects.SuspendLayout();
            this.defectLayout.SuspendLayout();
            this.defectButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDefects)).BeginInit();
            this.SuspendLayout();
            // 
            // rootLayout
            // 
            this.rootLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.commandLayout, 0, 0);
            this.rootLayout.Controls.Add(this.grpBasicInfo, 0, 1);
            this.rootLayout.Controls.Add(this.middleLayout, 0, 2);
            this.rootLayout.Controls.Add(this.grpDefects, 0, 3);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(21, 19);
            this.rootLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.RowCount = 4;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 77F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 202F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 62F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.rootLayout.Size = new System.Drawing.Size(2073, 1277);
            this.rootLayout.TabIndex = 0;
            // 
            // commandLayout
            // 
            this.commandLayout.ColumnCount = 2;
            this.commandLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.commandLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 600F));
            this.commandLayout.Controls.Add(this.commandButtons, 0, 0);
            this.commandLayout.Controls.Add(this.lblVersion, 1, 0);
            this.commandLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commandLayout.Location = new System.Drawing.Point(4, 5);
            this.commandLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.commandLayout.Name = "commandLayout";
            this.commandLayout.Size = new System.Drawing.Size(2065, 67);
            this.commandLayout.TabIndex = 0;
            // 
            // commandButtons
            // 
            this.commandButtons.Controls.Add(this.btnSave);
            this.commandButtons.Controls.Add(this.btnBuildTemplate);
            this.commandButtons.Controls.Add(this.btnTestLocalization);
            this.commandButtons.Controls.Add(this.btnVersion);
            this.commandButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commandButtons.Location = new System.Drawing.Point(4, 5);
            this.commandButtons.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.commandButtons.Name = "commandButtons";
            this.commandButtons.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.commandButtons.Size = new System.Drawing.Size(1457, 160);
            this.commandButtons.TabIndex = 0;
            this.commandButtons.WrapContents = false;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(4, 13);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(168, 37);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "保存产品定义";
            // 
            // btnBuildTemplate
            // 
            this.btnBuildTemplate.Location = new System.Drawing.Point(180, 13);
            this.btnBuildTemplate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnBuildTemplate.Name = "btnBuildTemplate";
            this.btnBuildTemplate.Size = new System.Drawing.Size(192, 37);
            this.btnBuildTemplate.TabIndex = 1;
            this.btnBuildTemplate.Text = "建立 / 更新模板";
            // 
            // btnTestLocalization
            // 
            this.btnTestLocalization.Location = new System.Drawing.Point(380, 13);
            this.btnTestLocalization.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnTestLocalization.Name = "btnTestLocalization";
            this.btnTestLocalization.Size = new System.Drawing.Size(138, 37);
            this.btnTestLocalization.TabIndex = 2;
            this.btnTestLocalization.Text = "测试定位";
            // 
            // btnVersion
            // 
            this.btnVersion.Location = new System.Drawing.Point(526, 13);
            this.btnVersion.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnVersion.Name = "btnVersion";
            this.btnVersion.Size = new System.Drawing.Size(138, 37);
            this.btnVersion.TabIndex = 3;
            this.btnVersion.Text = "版本记录";
            // 
            // lblVersion
            // 
            this.lblVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVersion.ForeColor = System.Drawing.Color.DimGray;
            this.lblVersion.Location = new System.Drawing.Point(1469, 0);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(592, 170);
            this.lblVersion.TabIndex = 1;
            this.lblVersion.Text = "产品定义版本：PD-1.0.0    模板版本：LT-1.0.0    状态：已配置";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // grpBasicInfo
            // 
            this.grpBasicInfo.BackColor = System.Drawing.Color.White;
            this.grpBasicInfo.Controls.Add(this.basicLayout);
            this.grpBasicInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpBasicInfo.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpBasicInfo.Location = new System.Drawing.Point(4, 82);
            this.grpBasicInfo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpBasicInfo.Name = "grpBasicInfo";
            this.grpBasicInfo.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpBasicInfo.Size = new System.Drawing.Size(2065, 192);
            this.grpBasicInfo.TabIndex = 1;
            this.grpBasicInfo.TabStop = false;
            this.grpBasicInfo.Text = "产品基本信息";
            // 
            // basicLayout
            // 
            this.basicLayout.ColumnCount = 6;
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.basicLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.basicLayout.Controls.Add(this.lblProductName, 0, 0);
            this.basicLayout.Controls.Add(this.txtProductName, 0, 1);
            this.basicLayout.Controls.Add(this.lblProductCode, 1, 0);
            this.basicLayout.Controls.Add(this.txtProductCode, 1, 1);
            this.basicLayout.Controls.Add(this.lblImageSize, 2, 0);
            this.basicLayout.Controls.Add(this.txtImageSize, 2, 1);
            this.basicLayout.Controls.Add(this.lblProductCount, 3, 0);
            this.basicLayout.Controls.Add(this.txtProductCount, 3, 1);
            this.basicLayout.Controls.Add(this.lblPose, 4, 0);
            this.basicLayout.Controls.Add(this.cboPose, 4, 1);
            this.basicLayout.Controls.Add(this.lblAcquisition, 5, 0);
            this.basicLayout.Controls.Add(this.txtAcquisition, 5, 1);
            this.basicLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.basicLayout.Location = new System.Drawing.Point(4, 39);
            this.basicLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.basicLayout.Name = "basicLayout";
            this.basicLayout.RowCount = 2;
            this.basicLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.basicLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.basicLayout.Size = new System.Drawing.Size(2057, 148);
            this.basicLayout.TabIndex = 0;
            // 
            // lblProductName
            // 
            this.lblProductName.Location = new System.Drawing.Point(4, 0);
            this.lblProductName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProductName.Name = "lblProductName";
            this.lblProductName.Size = new System.Drawing.Size(40, 37);
            this.lblProductName.TabIndex = 0;
            this.lblProductName.Text = "产品名称";
            // 
            // txtProductName
            // 
            this.txtProductName.Location = new System.Drawing.Point(4, 50);
            this.txtProductName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtProductName.Name = "txtProductName";
            this.txtProductName.Size = new System.Drawing.Size(38, 41);
            this.txtProductName.TabIndex = 1;
            this.txtProductName.Text = "轴承盖板";
            // 
            // lblProductCode
            // 
            this.lblProductCode.Location = new System.Drawing.Point(374, 0);
            this.lblProductCode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProductCode.Name = "lblProductCode";
            this.lblProductCode.Size = new System.Drawing.Size(144, 37);
            this.lblProductCode.TabIndex = 2;
            this.lblProductCode.Text = "产品编号";
            // 
            // txtProductCode
            // 
            this.txtProductCode.Location = new System.Drawing.Point(374, 50);
            this.txtProductCode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtProductCode.Name = "txtProductCode";
            this.txtProductCode.Size = new System.Drawing.Size(38, 41);
            this.txtProductCode.TabIndex = 3;
            this.txtProductCode.Text = "P-20250516-001";
            // 
            // lblImageSize
            // 
            this.lblImageSize.Location = new System.Drawing.Point(744, 0);
            this.lblImageSize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblImageSize.Name = "lblImageSize";
            this.lblImageSize.Size = new System.Drawing.Size(40, 37);
            this.lblImageSize.TabIndex = 4;
            this.lblImageSize.Text = "图像尺寸";
            // 
            // txtImageSize
            // 
            this.txtImageSize.Location = new System.Drawing.Point(744, 50);
            this.txtImageSize.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtImageSize.Name = "txtImageSize";
            this.txtImageSize.Size = new System.Drawing.Size(38, 41);
            this.txtImageSize.TabIndex = 5;
            this.txtImageSize.Text = "2448 × 2048 px";
            // 
            // lblProductCount
            // 
            this.lblProductCount.Location = new System.Drawing.Point(1114, 0);
            this.lblProductCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProductCount.Name = "lblProductCount";
            this.lblProductCount.Size = new System.Drawing.Size(30, 37);
            this.lblProductCount.TabIndex = 6;
            this.lblProductCount.Text = "单图产品数";
            // 
            // txtProductCount
            // 
            this.txtProductCount.Location = new System.Drawing.Point(1114, 50);
            this.txtProductCount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtProductCount.Name = "txtProductCount";
            this.txtProductCount.Size = new System.Drawing.Size(28, 41);
            this.txtProductCount.TabIndex = 7;
            this.txtProductCount.Text = "1";
            // 
            // lblPose
            // 
            this.lblPose.Location = new System.Drawing.Point(1401, 0);
            this.lblPose.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPose.Name = "lblPose";
            this.lblPose.Size = new System.Drawing.Size(30, 37);
            this.lblPose.TabIndex = 8;
            this.lblPose.Text = "姿态";
            // 
            // cboPose
            // 
            this.cboPose.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPose.Items.AddRange(new object[] {
            "固定",
            "允许旋转"});
            this.cboPose.Location = new System.Drawing.Point(1401, 50);
            this.cboPose.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cboPose.Name = "cboPose";
            this.cboPose.Size = new System.Drawing.Size(28, 44);
            this.cboPose.TabIndex = 9;
            // 
            // lblAcquisition
            // 
            this.lblAcquisition.Location = new System.Drawing.Point(1688, 0);
            this.lblAcquisition.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAcquisition.Name = "lblAcquisition";
            this.lblAcquisition.Size = new System.Drawing.Size(46, 37);
            this.lblAcquisition.TabIndex = 10;
            this.lblAcquisition.Text = "采集条件";
            // 
            // txtAcquisition
            // 
            this.txtAcquisition.Location = new System.Drawing.Point(1688, 50);
            this.txtAcquisition.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtAcquisition.Name = "txtAcquisition";
            this.txtAcquisition.Size = new System.Drawing.Size(44, 41);
            this.txtAcquisition.TabIndex = 11;
            this.txtAcquisition.Text = "相机 / 光照 / 背景稳定";
            // 
            // middleLayout
            // 
            this.middleLayout.ColumnCount = 2;
            this.middleLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 64F));
            this.middleLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.middleLayout.Controls.Add(this.grpTemplate, 0, 0);
            this.middleLayout.Controls.Add(this.parameterStack, 1, 0);
            this.middleLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.middleLayout.Location = new System.Drawing.Point(4, 284);
            this.middleLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.middleLayout.Name = "middleLayout";
            this.middleLayout.Size = new System.Drawing.Size(2065, 608);
            this.middleLayout.TabIndex = 2;
            // 
            // grpTemplate
            // 
            this.grpTemplate.BackColor = System.Drawing.Color.White;
            this.grpTemplate.Controls.Add(this.templateLayout);
            this.grpTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTemplate.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpTemplate.Location = new System.Drawing.Point(4, 5);
            this.grpTemplate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpTemplate.Name = "grpTemplate";
            this.grpTemplate.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpTemplate.Size = new System.Drawing.Size(1313, 598);
            this.grpTemplate.TabIndex = 0;
            this.grpTemplate.TabStop = false;
            this.grpTemplate.Text = "产品模板与 ROI";
            // 
            // templateLayout
            // 
            this.templateLayout.ColumnCount = 1;
            this.templateLayout.Controls.Add(this.templateButtons, 0, 0);
            this.templateLayout.Controls.Add(this.pnlTemplateCanvas, 0, 1);
            this.templateLayout.Controls.Add(this.templateFooter, 0, 2);
            this.templateLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.templateLayout.Location = new System.Drawing.Point(4, 39);
            this.templateLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.templateLayout.Name = "templateLayout";
            this.templateLayout.RowCount = 3;
            this.templateLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 67F));
            this.templateLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.templateLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.templateLayout.Size = new System.Drawing.Size(1305, 554);
            this.templateLayout.TabIndex = 0;
            // 
            // templateButtons
            // 
            this.templateButtons.Controls.Add(this.btnImportReference);
            this.templateButtons.Controls.Add(this.btnRectangleRoi);
            this.templateButtons.Controls.Add(this.btnClearRoi);
            this.templateButtons.Controls.Add(this.btnFastMode);
            this.templateButtons.Controls.Add(this.btnFineMode);
            this.templateButtons.Controls.Add(this.btnAutoMode);
            this.templateButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.templateButtons.Location = new System.Drawing.Point(4, 5);
            this.templateButtons.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.templateButtons.Name = "templateButtons";
            this.templateButtons.Size = new System.Drawing.Size(1297, 57);
            this.templateButtons.TabIndex = 0;
            this.templateButtons.WrapContents = false;
            // 
            // btnImportReference
            // 
            this.btnImportReference.Location = new System.Drawing.Point(4, 5);
            this.btnImportReference.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnImportReference.Name = "btnImportReference";
            this.btnImportReference.Size = new System.Drawing.Size(112, 37);
            this.btnImportReference.TabIndex = 0;
            this.btnImportReference.Text = "导入基准图";
            // 
            // btnRectangleRoi
            // 
            this.btnRectangleRoi.Location = new System.Drawing.Point(124, 5);
            this.btnRectangleRoi.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnRectangleRoi.Name = "btnRectangleRoi";
            this.btnRectangleRoi.Size = new System.Drawing.Size(112, 37);
            this.btnRectangleRoi.TabIndex = 1;
            this.btnRectangleRoi.Text = "Rectangle ROI";
            // 
            // btnClearRoi
            // 
            this.btnClearRoi.Location = new System.Drawing.Point(244, 5);
            this.btnClearRoi.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnClearRoi.Name = "btnClearRoi";
            this.btnClearRoi.Size = new System.Drawing.Size(112, 37);
            this.btnClearRoi.TabIndex = 2;
            this.btnClearRoi.Text = "清除 ROI";
            // 
            // btnFastMode
            // 
            this.btnFastMode.Location = new System.Drawing.Point(364, 5);
            this.btnFastMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnFastMode.Name = "btnFastMode";
            this.btnFastMode.Size = new System.Drawing.Size(112, 37);
            this.btnFastMode.TabIndex = 3;
            this.btnFastMode.Text = "快速模式";
            // 
            // btnFineMode
            // 
            this.btnFineMode.Location = new System.Drawing.Point(484, 5);
            this.btnFineMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnFineMode.Name = "btnFineMode";
            this.btnFineMode.Size = new System.Drawing.Size(112, 37);
            this.btnFineMode.TabIndex = 4;
            this.btnFineMode.Text = "精细模式";
            // 
            // btnAutoMode
            // 
            this.btnAutoMode.Location = new System.Drawing.Point(604, 5);
            this.btnAutoMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAutoMode.Name = "btnAutoMode";
            this.btnAutoMode.Size = new System.Drawing.Size(112, 37);
            this.btnAutoMode.TabIndex = 5;
            this.btnAutoMode.Text = "自动模式";
            // 
            // pnlTemplateCanvas
            // 
            this.pnlTemplateCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.pnlTemplateCanvas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTemplateCanvas.Controls.Add(this.lblCanvasHint);
            this.pnlTemplateCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTemplateCanvas.Location = new System.Drawing.Point(4, 72);
            this.pnlTemplateCanvas.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlTemplateCanvas.Name = "pnlTemplateCanvas";
            this.pnlTemplateCanvas.Size = new System.Drawing.Size(1297, 419);
            this.pnlTemplateCanvas.TabIndex = 1;
            // 
            // lblCanvasHint
            // 
            this.lblCanvasHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCanvasHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.lblCanvasHint.ForeColor = System.Drawing.Color.Gainsboro;
            this.lblCanvasHint.Location = new System.Drawing.Point(0, 0);
            this.lblCanvasHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCanvasHint.Name = "lblCanvasHint";
            this.lblCanvasHint.Size = new System.Drawing.Size(1295, 417);
            this.lblCanvasHint.TabIndex = 0;
            this.lblCanvasHint.Text = "基准图预览\r\n\r\n在此显示产品图像与 Template ROI\r\n可在功能代码中接入 HALCON HWindowControl";
            this.lblCanvasHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // templateFooter
            // 
            this.templateFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.templateFooter.ColumnCount = 4;
            this.templateFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.templateFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.templateFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.templateFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.templateFooter.Controls.Add(this.lblReferenceFile, 0, 0);
            this.templateFooter.Controls.Add(this.lblRoiState, 1, 0);
            this.templateFooter.Controls.Add(this.lblTemplateType, 2, 0);
            this.templateFooter.Controls.Add(this.lblLastScore, 3, 0);
            this.templateFooter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.templateFooter.Location = new System.Drawing.Point(4, 501);
            this.templateFooter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.templateFooter.Name = "templateFooter";
            this.templateFooter.Size = new System.Drawing.Size(1297, 48);
            this.templateFooter.TabIndex = 2;
            // 
            // lblReferenceFile
            // 
            this.lblReferenceFile.Location = new System.Drawing.Point(4, 0);
            this.lblReferenceFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblReferenceFile.Name = "lblReferenceFile";
            this.lblReferenceFile.Size = new System.Drawing.Size(66, 37);
            this.lblReferenceFile.TabIndex = 0;
            this.lblReferenceFile.Text = "基准图：reference_001.png";
            // 
            // lblRoiState
            // 
            this.lblRoiState.Location = new System.Drawing.Point(328, 0);
            this.lblRoiState.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRoiState.Name = "lblRoiState";
            this.lblRoiState.Size = new System.Drawing.Size(66, 37);
            this.lblRoiState.TabIndex = 1;
            this.lblRoiState.Text = "ROI：已定义";
            // 
            // lblTemplateType
            // 
            this.lblTemplateType.Location = new System.Drawing.Point(652, 0);
            this.lblTemplateType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTemplateType.Name = "lblTemplateType";
            this.lblTemplateType.Size = new System.Drawing.Size(66, 37);
            this.lblTemplateType.TabIndex = 2;
            this.lblTemplateType.Text = "定位模板：Shape Model";
            // 
            // lblLastScore
            // 
            this.lblLastScore.Location = new System.Drawing.Point(976, 0);
            this.lblLastScore.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLastScore.Name = "lblLastScore";
            this.lblLastScore.Size = new System.Drawing.Size(66, 37);
            this.lblLastScore.TabIndex = 3;
            this.lblLastScore.Text = "最近测试：Score 0.92";
            // 
            // parameterStack
            // 
            this.parameterStack.ColumnCount = 1;
            this.parameterStack.Controls.Add(this.grpLocalization, 0, 0);
            this.parameterStack.Controls.Add(this.grpCalibration, 0, 1);
            this.parameterStack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parameterStack.Location = new System.Drawing.Point(1325, 5);
            this.parameterStack.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.parameterStack.Name = "parameterStack";
            this.parameterStack.RowCount = 2;
            this.parameterStack.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58F));
            this.parameterStack.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42F));
            this.parameterStack.Size = new System.Drawing.Size(736, 598);
            this.parameterStack.TabIndex = 1;
            // 
            // grpLocalization
            // 
            this.grpLocalization.BackColor = System.Drawing.Color.White;
            this.grpLocalization.Controls.Add(this.localizationLayout);
            this.grpLocalization.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLocalization.Location = new System.Drawing.Point(4, 5);
            this.grpLocalization.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpLocalization.Name = "grpLocalization";
            this.grpLocalization.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpLocalization.Size = new System.Drawing.Size(728, 336);
            this.grpLocalization.TabIndex = 0;
            this.grpLocalization.TabStop = false;
            this.grpLocalization.Text = "定位参数";
            // 
            // localizationLayout
            // 
            this.localizationLayout.ColumnCount = 2;
            this.localizationLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.localizationLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.localizationLayout.Controls.Add(this.lblLocalizationMethodKey, 0, 0);
            this.localizationLayout.Controls.Add(this.cboLocalizationMethod, 1, 0);
            this.localizationLayout.Controls.Add(this.lblModelTypeKey, 0, 1);
            this.localizationLayout.Controls.Add(this.cboModelType, 1, 1);
            this.localizationLayout.Controls.Add(this.lblMinScoreKey, 0, 2);
            this.localizationLayout.Controls.Add(this.txtMinScore, 1, 2);
            this.localizationLayout.Controls.Add(this.lblAngleRangeKey, 0, 3);
            this.localizationLayout.Controls.Add(this.txtAngleRange, 1, 3);
            this.localizationLayout.Controls.Add(this.lblScaleRangeKey, 0, 4);
            this.localizationLayout.Controls.Add(this.txtScaleRange, 1, 4);
            this.localizationLayout.Controls.Add(this.lblMatchCountKey, 0, 5);
            this.localizationLayout.Controls.Add(this.txtMatchCount, 1, 5);
            this.localizationLayout.Controls.Add(this.lblLastResultKey, 0, 6);
            this.localizationLayout.Controls.Add(this.txtLastResult, 1, 6);
            this.localizationLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.localizationLayout.Location = new System.Drawing.Point(4, 33);
            this.localizationLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.localizationLayout.Name = "localizationLayout";
            this.localizationLayout.RowCount = 7;
            this.localizationLayout.Size = new System.Drawing.Size(720, 298);
            this.localizationLayout.TabIndex = 0;
            // 
            // lblLocalizationMethodKey
            // 
            this.lblLocalizationMethodKey.Location = new System.Drawing.Point(4, 0);
            this.lblLocalizationMethodKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLocalizationMethodKey.Name = "lblLocalizationMethodKey";
            this.lblLocalizationMethodKey.Size = new System.Drawing.Size(106, 37);
            this.lblLocalizationMethodKey.TabIndex = 0;
            this.lblLocalizationMethodKey.Text = "定位方式";
            // 
            // cboLocalizationMethod
            // 
            this.cboLocalizationMethod.Items.AddRange(new object[] {
            "HALCON Shape Matching",
            "特征匹配"});
            this.cboLocalizationMethod.Location = new System.Drawing.Point(292, 5);
            this.cboLocalizationMethod.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cboLocalizationMethod.Name = "cboLocalizationMethod";
            this.cboLocalizationMethod.Size = new System.Drawing.Size(164, 32);
            this.cboLocalizationMethod.TabIndex = 1;
            // 
            // lblModelTypeKey
            // 
            this.lblModelTypeKey.Location = new System.Drawing.Point(4, 42);
            this.lblModelTypeKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblModelTypeKey.Name = "lblModelTypeKey";
            this.lblModelTypeKey.Size = new System.Drawing.Size(106, 37);
            this.lblModelTypeKey.TabIndex = 2;
            this.lblModelTypeKey.Text = "模型类型";
            // 
            // cboModelType
            // 
            this.cboModelType.Items.AddRange(new object[] {
            "Shape Model",
            "NCC Model"});
            this.cboModelType.Location = new System.Drawing.Point(292, 47);
            this.cboModelType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cboModelType.Name = "cboModelType";
            this.cboModelType.Size = new System.Drawing.Size(164, 32);
            this.cboModelType.TabIndex = 3;
            // 
            // lblMinScoreKey
            // 
            this.lblMinScoreKey.Location = new System.Drawing.Point(4, 84);
            this.lblMinScoreKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMinScoreKey.Name = "lblMinScoreKey";
            this.lblMinScoreKey.Size = new System.Drawing.Size(106, 37);
            this.lblMinScoreKey.TabIndex = 4;
            this.lblMinScoreKey.Text = "最小匹配分数";
            // 
            // txtMinScore
            // 
            this.txtMinScore.Location = new System.Drawing.Point(292, 89);
            this.txtMinScore.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMinScore.Name = "txtMinScore";
            this.txtMinScore.Size = new System.Drawing.Size(148, 35);
            this.txtMinScore.TabIndex = 5;
            this.txtMinScore.Text = "0.60";
            // 
            // lblAngleRangeKey
            // 
            this.lblAngleRangeKey.Location = new System.Drawing.Point(4, 129);
            this.lblAngleRangeKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAngleRangeKey.Name = "lblAngleRangeKey";
            this.lblAngleRangeKey.Size = new System.Drawing.Size(106, 37);
            this.lblAngleRangeKey.TabIndex = 6;
            this.lblAngleRangeKey.Text = "角度范围";
            // 
            // txtAngleRange
            // 
            this.txtAngleRange.Location = new System.Drawing.Point(292, 134);
            this.txtAngleRange.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtAngleRange.Name = "txtAngleRange";
            this.txtAngleRange.Size = new System.Drawing.Size(148, 35);
            this.txtAngleRange.TabIndex = 7;
            this.txtAngleRange.Text = "-30° ~ 30°";
            // 
            // lblScaleRangeKey
            // 
            this.lblScaleRangeKey.Location = new System.Drawing.Point(4, 174);
            this.lblScaleRangeKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScaleRangeKey.Name = "lblScaleRangeKey";
            this.lblScaleRangeKey.Size = new System.Drawing.Size(106, 37);
            this.lblScaleRangeKey.TabIndex = 8;
            this.lblScaleRangeKey.Text = "缩放范围";
            // 
            // txtScaleRange
            // 
            this.txtScaleRange.Location = new System.Drawing.Point(292, 179);
            this.txtScaleRange.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtScaleRange.Name = "txtScaleRange";
            this.txtScaleRange.Size = new System.Drawing.Size(148, 35);
            this.txtScaleRange.TabIndex = 9;
            this.txtScaleRange.Text = "90% ~ 110%";
            // 
            // lblMatchCountKey
            // 
            this.lblMatchCountKey.Location = new System.Drawing.Point(4, 219);
            this.lblMatchCountKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMatchCountKey.Name = "lblMatchCountKey";
            this.lblMatchCountKey.Size = new System.Drawing.Size(106, 37);
            this.lblMatchCountKey.TabIndex = 10;
            this.lblMatchCountKey.Text = "匹配个数";
            // 
            // txtMatchCount
            // 
            this.txtMatchCount.Location = new System.Drawing.Point(292, 224);
            this.txtMatchCount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMatchCount.Name = "txtMatchCount";
            this.txtMatchCount.Size = new System.Drawing.Size(148, 35);
            this.txtMatchCount.TabIndex = 11;
            this.txtMatchCount.Text = "1";
            // 
            // lblLastResultKey
            // 
            this.lblLastResultKey.Location = new System.Drawing.Point(4, 264);
            this.lblLastResultKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLastResultKey.Name = "lblLastResultKey";
            this.lblLastResultKey.Size = new System.Drawing.Size(106, 37);
            this.lblLastResultKey.TabIndex = 12;
            this.lblLastResultKey.Text = "最近定位结果";
            // 
            // txtLastResult
            // 
            this.txtLastResult.Location = new System.Drawing.Point(292, 269);
            this.txtLastResult.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtLastResult.Name = "txtLastResult";
            this.txtLastResult.Size = new System.Drawing.Size(148, 35);
            this.txtLastResult.TabIndex = 13;
            this.txtLastResult.Text = "Row 1024.32 / Col 1226.78 / Angle -1.24° / Score 0.92";
            // 
            // grpCalibration
            // 
            this.grpCalibration.BackColor = System.Drawing.Color.White;
            this.grpCalibration.Controls.Add(this.calibrationLayout);
            this.grpCalibration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCalibration.Location = new System.Drawing.Point(4, 351);
            this.grpCalibration.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpCalibration.Name = "grpCalibration";
            this.grpCalibration.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpCalibration.Size = new System.Drawing.Size(728, 242);
            this.grpCalibration.TabIndex = 1;
            this.grpCalibration.TabStop = false;
            this.grpCalibration.Text = "尺寸标定";
            // 
            // calibrationLayout
            // 
            this.calibrationLayout.ColumnCount = 2;
            this.calibrationLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.calibrationLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.calibrationLayout.Controls.Add(this.lblPixelXKey, 0, 0);
            this.calibrationLayout.Controls.Add(this.txtPixelX, 1, 0);
            this.calibrationLayout.Controls.Add(this.lblPixelYKey, 0, 1);
            this.calibrationLayout.Controls.Add(this.txtPixelY, 1, 1);
            this.calibrationLayout.Controls.Add(this.lblLengthUnitKey, 0, 2);
            this.calibrationLayout.Controls.Add(this.cboLengthUnit, 1, 2);
            this.calibrationLayout.Controls.Add(this.lblAreaUnitKey, 0, 3);
            this.calibrationLayout.Controls.Add(this.cboAreaUnit, 1, 3);
            this.calibrationLayout.Controls.Add(this.lblCalibrationVersionKey, 0, 4);
            this.calibrationLayout.Controls.Add(this.txtCalibrationVersion, 1, 4);
            this.calibrationLayout.Controls.Add(this.lblCalibrationStateKey, 0, 5);
            this.calibrationLayout.Controls.Add(this.cboCalibrationState, 1, 5);
            this.calibrationLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.calibrationLayout.Location = new System.Drawing.Point(4, 33);
            this.calibrationLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.calibrationLayout.Name = "calibrationLayout";
            this.calibrationLayout.RowCount = 6;
            this.calibrationLayout.Size = new System.Drawing.Size(720, 204);
            this.calibrationLayout.TabIndex = 0;
            // 
            // lblPixelXKey
            // 
            this.lblPixelXKey.Location = new System.Drawing.Point(4, 0);
            this.lblPixelXKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPixelXKey.Name = "lblPixelXKey";
            this.lblPixelXKey.Size = new System.Drawing.Size(106, 37);
            this.lblPixelXKey.TabIndex = 0;
            this.lblPixelXKey.Text = "像素尺寸 X";
            // 
            // txtPixelX
            // 
            this.txtPixelX.Location = new System.Drawing.Point(292, 5);
            this.txtPixelX.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPixelX.Name = "txtPixelX";
            this.txtPixelX.Size = new System.Drawing.Size(148, 35);
            this.txtPixelX.TabIndex = 1;
            this.txtPixelX.Text = "6.5000 μm/px";
            // 
            // lblPixelYKey
            // 
            this.lblPixelYKey.Location = new System.Drawing.Point(4, 45);
            this.lblPixelYKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPixelYKey.Name = "lblPixelYKey";
            this.lblPixelYKey.Size = new System.Drawing.Size(106, 37);
            this.lblPixelYKey.TabIndex = 2;
            this.lblPixelYKey.Text = "像素尺寸 Y";
            // 
            // txtPixelY
            // 
            this.txtPixelY.Location = new System.Drawing.Point(292, 50);
            this.txtPixelY.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPixelY.Name = "txtPixelY";
            this.txtPixelY.Size = new System.Drawing.Size(148, 35);
            this.txtPixelY.TabIndex = 3;
            this.txtPixelY.Text = "6.5000 μm/px";
            // 
            // lblLengthUnitKey
            // 
            this.lblLengthUnitKey.Location = new System.Drawing.Point(4, 90);
            this.lblLengthUnitKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLengthUnitKey.Name = "lblLengthUnitKey";
            this.lblLengthUnitKey.Size = new System.Drawing.Size(106, 37);
            this.lblLengthUnitKey.TabIndex = 4;
            this.lblLengthUnitKey.Text = "长度单位";
            // 
            // cboLengthUnit
            // 
            this.cboLengthUnit.Items.AddRange(new object[] {
            "mm",
            "px"});
            this.cboLengthUnit.Location = new System.Drawing.Point(292, 95);
            this.cboLengthUnit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cboLengthUnit.Name = "cboLengthUnit";
            this.cboLengthUnit.Size = new System.Drawing.Size(164, 32);
            this.cboLengthUnit.TabIndex = 5;
            // 
            // lblAreaUnitKey
            // 
            this.lblAreaUnitKey.Location = new System.Drawing.Point(4, 132);
            this.lblAreaUnitKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAreaUnitKey.Name = "lblAreaUnitKey";
            this.lblAreaUnitKey.Size = new System.Drawing.Size(106, 37);
            this.lblAreaUnitKey.TabIndex = 6;
            this.lblAreaUnitKey.Text = "面积单位";
            // 
            // cboAreaUnit
            // 
            this.cboAreaUnit.Items.AddRange(new object[] {
            "mm²",
            "px²"});
            this.cboAreaUnit.Location = new System.Drawing.Point(292, 137);
            this.cboAreaUnit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cboAreaUnit.Name = "cboAreaUnit";
            this.cboAreaUnit.Size = new System.Drawing.Size(164, 32);
            this.cboAreaUnit.TabIndex = 7;
            // 
            // lblCalibrationVersionKey
            // 
            this.lblCalibrationVersionKey.Location = new System.Drawing.Point(4, 174);
            this.lblCalibrationVersionKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCalibrationVersionKey.Name = "lblCalibrationVersionKey";
            this.lblCalibrationVersionKey.Size = new System.Drawing.Size(106, 37);
            this.lblCalibrationVersionKey.TabIndex = 8;
            this.lblCalibrationVersionKey.Text = "标定版本";
            // 
            // txtCalibrationVersion
            // 
            this.txtCalibrationVersion.Location = new System.Drawing.Point(292, 179);
            this.txtCalibrationVersion.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCalibrationVersion.Name = "txtCalibrationVersion";
            this.txtCalibrationVersion.Size = new System.Drawing.Size(148, 35);
            this.txtCalibrationVersion.TabIndex = 9;
            this.txtCalibrationVersion.Text = "CV-1.0.2";
            // 
            // lblCalibrationStateKey
            // 
            this.lblCalibrationStateKey.Location = new System.Drawing.Point(4, 219);
            this.lblCalibrationStateKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCalibrationStateKey.Name = "lblCalibrationStateKey";
            this.lblCalibrationStateKey.Size = new System.Drawing.Size(106, 37);
            this.lblCalibrationStateKey.TabIndex = 10;
            this.lblCalibrationStateKey.Text = "状态";
            // 
            // cboCalibrationState
            // 
            this.cboCalibrationState.Items.AddRange(new object[] {
            "有效",
            "无效"});
            this.cboCalibrationState.Location = new System.Drawing.Point(292, 224);
            this.cboCalibrationState.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cboCalibrationState.Name = "cboCalibrationState";
            this.cboCalibrationState.Size = new System.Drawing.Size(164, 32);
            this.cboCalibrationState.TabIndex = 11;
            // 
            // grpDefects
            // 
            this.grpDefects.BackColor = System.Drawing.Color.White;
            this.grpDefects.Controls.Add(this.defectLayout);
            this.grpDefects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDefects.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpDefects.Location = new System.Drawing.Point(4, 902);
            this.grpDefects.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpDefects.Name = "grpDefects";
            this.grpDefects.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpDefects.Size = new System.Drawing.Size(2065, 370);
            this.grpDefects.TabIndex = 3;
            this.grpDefects.TabStop = false;
            this.grpDefects.Text = "缺陷类别管理";
            // 
            // defectLayout
            // 
            this.defectLayout.ColumnCount = 1;
            this.defectLayout.Controls.Add(this.defectButtons, 0, 0);
            this.defectLayout.Controls.Add(this.dgvDefects, 0, 1);
            this.defectLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.defectLayout.Location = new System.Drawing.Point(4, 39);
            this.defectLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.defectLayout.Name = "defectLayout";
            this.defectLayout.RowCount = 2;
            this.defectLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.defectLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.defectLayout.Size = new System.Drawing.Size(2057, 326);
            this.defectLayout.TabIndex = 0;
            // 
            // defectButtons
            // 
            this.defectButtons.Controls.Add(this.btnAddDefect);
            this.defectButtons.Controls.Add(this.btnEditDefect);
            this.defectButtons.Controls.Add(this.btnDeleteDefect);
            this.defectButtons.Controls.Add(this.btnToggleDefect);
            this.defectButtons.Controls.Add(this.btnImportDefects);
            this.defectButtons.Controls.Add(this.btnExportDefects);
            this.defectButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.defectButtons.Location = new System.Drawing.Point(4, 5);
            this.defectButtons.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.defectButtons.Name = "defectButtons";
            this.defectButtons.Size = new System.Drawing.Size(2049, 54);
            this.defectButtons.TabIndex = 0;
            this.defectButtons.WrapContents = false;
            // 
            // btnAddDefect
            // 
            this.btnAddDefect.Location = new System.Drawing.Point(4, 5);
            this.btnAddDefect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAddDefect.Name = "btnAddDefect";
            this.btnAddDefect.Size = new System.Drawing.Size(112, 37);
            this.btnAddDefect.TabIndex = 0;
            this.btnAddDefect.Text = "新增类别";
            // 
            // btnEditDefect
            // 
            this.btnEditDefect.Location = new System.Drawing.Point(124, 5);
            this.btnEditDefect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnEditDefect.Name = "btnEditDefect";
            this.btnEditDefect.Size = new System.Drawing.Size(112, 37);
            this.btnEditDefect.TabIndex = 1;
            this.btnEditDefect.Text = "编辑类别";
            // 
            // btnDeleteDefect
            // 
            this.btnDeleteDefect.Location = new System.Drawing.Point(244, 5);
            this.btnDeleteDefect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDeleteDefect.Name = "btnDeleteDefect";
            this.btnDeleteDefect.Size = new System.Drawing.Size(112, 37);
            this.btnDeleteDefect.TabIndex = 2;
            this.btnDeleteDefect.Text = "删除类别";
            // 
            // btnToggleDefect
            // 
            this.btnToggleDefect.Location = new System.Drawing.Point(364, 5);
            this.btnToggleDefect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnToggleDefect.Name = "btnToggleDefect";
            this.btnToggleDefect.Size = new System.Drawing.Size(112, 37);
            this.btnToggleDefect.TabIndex = 3;
            this.btnToggleDefect.Text = "启用 / 停用";
            // 
            // btnImportDefects
            // 
            this.btnImportDefects.Location = new System.Drawing.Point(484, 5);
            this.btnImportDefects.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnImportDefects.Name = "btnImportDefects";
            this.btnImportDefects.Size = new System.Drawing.Size(112, 37);
            this.btnImportDefects.TabIndex = 4;
            this.btnImportDefects.Text = "导入配置";
            // 
            // btnExportDefects
            // 
            this.btnExportDefects.Location = new System.Drawing.Point(604, 5);
            this.btnExportDefects.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnExportDefects.Name = "btnExportDefects";
            this.btnExportDefects.Size = new System.Drawing.Size(112, 37);
            this.btnExportDefects.TabIndex = 5;
            this.btnExportDefects.Text = "导出配置";
            // 
            // dgvDefects
            // 
            this.dgvDefects.AllowUserToAddRows = false;
            this.dgvDefects.AllowUserToDeleteRows = false;
            this.dgvDefects.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDefects.BackgroundColor = System.Drawing.Color.White;
            this.dgvDefects.ColumnHeadersHeight = 46;
            this.dgvDefects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvDefectsCol1,
            this.dgvDefectsCol2,
            this.dgvDefectsCol3,
            this.dgvDefectsCol4,
            this.dgvDefectsCol5,
            this.dgvDefectsCol6,
            this.dgvDefectsCol7,
            this.dgvDefectsCol8});
            this.dgvDefects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDefects.Location = new System.Drawing.Point(4, 69);
            this.dgvDefects.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dgvDefects.Name = "dgvDefects";
            this.dgvDefects.ReadOnly = true;
            this.dgvDefects.RowHeadersVisible = false;
            this.dgvDefects.RowHeadersWidth = 82;
            this.dgvDefects.Size = new System.Drawing.Size(2049, 252);
            this.dgvDefects.TabIndex = 1;
            // 
            // dgvDefectsCol1
            // 
            this.dgvDefectsCol1.HeaderText = "序号";
            this.dgvDefectsCol1.MinimumWidth = 10;
            this.dgvDefectsCol1.Name = "dgvDefectsCol1";
            this.dgvDefectsCol1.ReadOnly = true;
            // 
            // dgvDefectsCol2
            // 
            this.dgvDefectsCol2.HeaderText = "缺陷名称";
            this.dgvDefectsCol2.MinimumWidth = 10;
            this.dgvDefectsCol2.Name = "dgvDefectsCol2";
            this.dgvDefectsCol2.ReadOnly = true;
            // 
            // dgvDefectsCol3
            // 
            this.dgvDefectsCol3.HeaderText = "缺陷类型";
            this.dgvDefectsCol3.MinimumWidth = 10;
            this.dgvDefectsCol3.Name = "dgvDefectsCol3";
            this.dgvDefectsCol3.ReadOnly = true;
            // 
            // dgvDefectsCol4
            // 
            this.dgvDefectsCol4.HeaderText = "检测策略";
            this.dgvDefectsCol4.MinimumWidth = 10;
            this.dgvDefectsCol4.Name = "dgvDefectsCol4";
            this.dgvDefectsCol4.ReadOnly = true;
            // 
            // dgvDefectsCol5
            // 
            this.dgvDefectsCol5.HeaderText = "默认阈值";
            this.dgvDefectsCol5.MinimumWidth = 10;
            this.dgvDefectsCol5.Name = "dgvDefectsCol5";
            this.dgvDefectsCol5.ReadOnly = true;
            // 
            // dgvDefectsCol6
            // 
            this.dgvDefectsCol6.HeaderText = "最小面积(px)";
            this.dgvDefectsCol6.MinimumWidth = 10;
            this.dgvDefectsCol6.Name = "dgvDefectsCol6";
            this.dgvDefectsCol6.ReadOnly = true;
            // 
            // dgvDefectsCol7
            // 
            this.dgvDefectsCol7.HeaderText = "最小长度(px)";
            this.dgvDefectsCol7.MinimumWidth = 10;
            this.dgvDefectsCol7.Name = "dgvDefectsCol7";
            this.dgvDefectsCol7.ReadOnly = true;
            // 
            // dgvDefectsCol8
            // 
            this.dgvDefectsCol8.HeaderText = "状态";
            this.dgvDefectsCol8.MinimumWidth = 10;
            this.dgvDefectsCol8.Name = "dgvDefectsCol8";
            this.dgvDefectsCol8.ReadOnly = true;
            // 
            // ProductDefinitionPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.Controls.Add(this.rootLayout);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ProductDefinitionPage";
            this.Padding = new System.Windows.Forms.Padding(21, 19, 6, 16);
            this.Size = new System.Drawing.Size(2100, 1312);
            this.rootLayout.ResumeLayout(false);
            this.commandLayout.ResumeLayout(false);
            this.commandButtons.ResumeLayout(false);
            this.grpBasicInfo.ResumeLayout(false);
            this.basicLayout.ResumeLayout(false);
            this.basicLayout.PerformLayout();
            this.middleLayout.ResumeLayout(false);
            this.grpTemplate.ResumeLayout(false);
            this.templateLayout.ResumeLayout(false);
            this.templateButtons.ResumeLayout(false);
            this.pnlTemplateCanvas.ResumeLayout(false);
            this.templateFooter.ResumeLayout(false);
            this.parameterStack.ResumeLayout(false);
            this.grpLocalization.ResumeLayout(false);
            this.localizationLayout.ResumeLayout(false);
            this.localizationLayout.PerformLayout();
            this.grpCalibration.ResumeLayout(false);
            this.calibrationLayout.ResumeLayout(false);
            this.calibrationLayout.PerformLayout();
            this.grpDefects.ResumeLayout(false);
            this.defectLayout.ResumeLayout(false);
            this.defectButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDefects)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
