using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace IAD.Shell
{
    partial class RoiManagerDialog
    {
        private IContainer components = null;
        private TableLayoutPanel rootLayout;
        private DataGridView dgvRois;
        private GroupBox grpEditor;
        private TableLayoutPanel editorLayout;
        private TextBox txtName;
        private ComboBox cboType;
        private NumericUpDown numCenterX;
        private NumericUpDown numCenterY;
        private NumericUpDown numWidth;
        private NumericUpDown numHeight;
        private NumericUpDown numAngle;
        private NumericUpDown numSort;
        private CheckBox chkEnabled;
        private FlowLayoutPanel buttonPanel;
        private Button btnNew;
        private Button btnEdit;
        private Button btnSave;
        private Button btnDelete;
        private Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.rootLayout = new TableLayoutPanel();
            this.dgvRois = new DataGridView();
            this.grpEditor = new GroupBox();
            this.editorLayout = new TableLayoutPanel();
            this.txtName = new TextBox();
            this.cboType = new ComboBox();
            this.numCenterX = CreateNumber(-100000, 100000, 3);
            this.numCenterY = CreateNumber(-100000, 100000, 3);
            this.numWidth = CreateNumber(0, 100000, 3);
            this.numHeight = CreateNumber(0, 100000, 3);
            this.numAngle = CreateNumber(-360, 360, 3);
            this.numSort = CreateNumber(0, 10000, 0);
            this.chkEnabled = new CheckBox();
            this.buttonPanel = new FlowLayoutPanel();
            this.btnNew = new Button();
            this.btnEdit = new Button();
            this.btnSave = new Button();
            this.btnDelete = new Button();
            this.btnClose = new Button();
            ((ISupportInitialize)(this.dgvRois)).BeginInit();
            this.SuspendLayout();

            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.rootLayout.RowCount = 3;
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            this.rootLayout.Dock = DockStyle.Fill;
            this.rootLayout.Padding = new Padding(14);

            this.dgvRois.AllowUserToAddRows = false;
            this.dgvRois.AllowUserToDeleteRows = false;
            this.dgvRois.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvRois.BackgroundColor = Color.White;
            this.dgvRois.Dock = DockStyle.Fill;
            this.dgvRois.MultiSelect = false;
            this.dgvRois.ReadOnly = true;
            this.dgvRois.RowHeadersVisible = false;
            this.dgvRois.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvRois.Columns.Add("colName", "ROI名称");
            this.dgvRois.Columns.Add("colType", "类型");
            this.dgvRois.Columns.Add("colX", "CenterX");
            this.dgvRois.Columns.Add("colY", "CenterY");
            this.dgvRois.Columns.Add("colW", "Width");
            this.dgvRois.Columns.Add("colH", "Height");
            this.dgvRois.Columns.Add("colAngle", "Angle");
            this.dgvRois.Columns.Add("colState", "状态");
            this.dgvRois.CellDoubleClick += new DataGridViewCellEventHandler(this.dgvRois_CellDoubleClick);

            this.grpEditor.Text = "ROI 参数";
            this.grpEditor.Dock = DockStyle.Fill;
            this.grpEditor.Controls.Add(this.editorLayout);

            this.editorLayout.ColumnCount = 6;
            for (int i = 0; i < 6; i++) this.editorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666F));
            this.editorLayout.RowCount = 4;
            this.editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            this.editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            this.editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            this.editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            this.editorLayout.Dock = DockStyle.Fill;
            this.editorLayout.Padding = new Padding(10, 8, 10, 8);

            AddField(this.editorLayout, "ROI名称", this.txtName, 0, 0);
            this.cboType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboType.Items.AddRange(new object[] { "Rectangle", "Circle", "Polygon" });
            AddField(this.editorLayout, "ROI类型", this.cboType, 1, 0);
            AddField(this.editorLayout, "CenterX", this.numCenterX, 2, 0);
            AddField(this.editorLayout, "CenterY", this.numCenterY, 3, 0);
            AddField(this.editorLayout, "Width", this.numWidth, 4, 0);
            AddField(this.editorLayout, "Height", this.numHeight, 5, 0);
            AddField(this.editorLayout, "Angle(deg)", this.numAngle, 0, 2);
            AddField(this.editorLayout, "排序", this.numSort, 1, 2);
            this.chkEnabled.Text = "启用";
            this.chkEnabled.Dock = DockStyle.Fill;
            this.chkEnabled.TextAlign = ContentAlignment.MiddleLeft;
            this.editorLayout.Controls.Add(this.chkEnabled, 2, 3);

            this.buttonPanel.Dock = DockStyle.Fill;
            this.buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            this.buttonPanel.Padding = new Padding(0, 10, 0, 0);
            this.buttonPanel.WrapContents = false;
            SetButton(this.btnClose, "关闭", 88);
            this.btnClose.DialogResult = DialogResult.OK;
            SetButton(this.btnDelete, "删除", 88);
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            SetButton(this.btnSave, "新增 ROI", 100);
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            SetButton(this.btnEdit, "编辑", 88);
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            SetButton(this.btnNew, "新建", 88);
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            this.buttonPanel.Controls.Add(this.btnClose);
            this.buttonPanel.Controls.Add(this.btnDelete);
            this.buttonPanel.Controls.Add(this.btnSave);
            this.buttonPanel.Controls.Add(this.btnEdit);
            this.buttonPanel.Controls.Add(this.btnNew);

            this.rootLayout.Controls.Add(this.dgvRois, 0, 0);
            this.rootLayout.Controls.Add(this.grpEditor, 0, 1);
            this.rootLayout.Controls.Add(this.buttonPanel, 0, 2);

            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.ClientSize = new Size(900, 580);
            this.Controls.Add(this.rootLayout);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RoiManagerDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "ROI 管理";

            ((ISupportInitialize)(this.dgvRois)).EndInit();
            this.ResumeLayout(false);
        }

        private static NumericUpDown CreateNumber(decimal min, decimal max, int decimals)
        {
            NumericUpDown control = new NumericUpDown();
            control.Minimum = min;
            control.Maximum = max;
            control.DecimalPlaces = decimals;
            control.Dock = DockStyle.Fill;
            return control;
        }

        private static void AddField(TableLayoutPanel layout, string text, Control control, int column, int labelRow)
        {
            Label label = new Label();
            label.Text = text;
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.BottomLeft;
            layout.Controls.Add(label, column, labelRow);
            control.Dock = DockStyle.Fill;
            layout.Controls.Add(control, column, labelRow + 1);
        }

        private static void SetButton(Button button, string text, int width)
        {
            button.Text = text;
            button.Size = new Size(width, 34);
            button.Margin = new Padding(8, 0, 0, 0);
        }
    }
}
