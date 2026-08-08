using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace IAD.Shell
{
    partial class ProductSelectionDialog
    {
        private IContainer components = null;
        private TableLayoutPanel rootLayout;
        private Label lblTitle;
        private DataGridView dgvProducts;
        private DataGridViewTextBoxColumn colCode;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colState;
        private DataGridViewTextBoxColumn colUpdated;
        private FlowLayoutPanel buttonPanel;
        private Button btnNew;
        private Button btnOpen;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.rootLayout = new TableLayoutPanel();
            this.lblTitle = new Label();
            this.dgvProducts = new DataGridView();
            this.colCode = new DataGridViewTextBoxColumn();
            this.colName = new DataGridViewTextBoxColumn();
            this.colState = new DataGridViewTextBoxColumn();
            this.colUpdated = new DataGridViewTextBoxColumn();
            this.buttonPanel = new FlowLayoutPanel();
            this.btnNew = new Button();
            this.btnOpen = new Button();
            this.btnCancel = new Button();
            this.rootLayout.SuspendLayout();
            ((ISupportInitialize)(this.dgvProducts)).BeginInit();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();

            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.lblTitle, 0, 0);
            this.rootLayout.Controls.Add(this.dgvProducts, 0, 1);
            this.rootLayout.Controls.Add(this.buttonPanel, 0, 2);
            this.rootLayout.Dock = DockStyle.Fill;
            this.rootLayout.Padding = new Padding(16);
            this.rootLayout.RowCount = 3;
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));

            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
            this.lblTitle.Text = "选择已有产品，或新建一个产品定义";
            this.lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            this.dgvProducts.AllowUserToAddRows = false;
            this.dgvProducts.AllowUserToDeleteRows = false;
            this.dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProducts.BackgroundColor = Color.White;
            this.dgvProducts.BorderStyle = BorderStyle.Fixed3D;
            this.dgvProducts.ColumnHeadersHeight = 32;
            this.dgvProducts.Columns.AddRange(new DataGridViewColumn[] { this.colCode, this.colName, this.colState, this.colUpdated });
            this.dgvProducts.Dock = DockStyle.Fill;
            this.dgvProducts.MultiSelect = false;
            this.dgvProducts.ReadOnly = true;
            this.dgvProducts.RowHeadersVisible = false;
            this.dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvProducts.CellDoubleClick += new DataGridViewCellEventHandler(this.dgvProducts_CellDoubleClick);

            this.colCode.HeaderText = "产品编号";
            this.colCode.Name = "colCode";
            this.colName.HeaderText = "产品名称";
            this.colName.Name = "colName";
            this.colState.HeaderText = "状态";
            this.colState.Name = "colState";
            this.colUpdated.HeaderText = "更新时间";
            this.colUpdated.Name = "colUpdated";

            this.buttonPanel.Controls.Add(this.btnCancel);
            this.buttonPanel.Controls.Add(this.btnOpen);
            this.buttonPanel.Controls.Add(this.btnNew);
            this.buttonPanel.Dock = DockStyle.Fill;
            this.buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            this.buttonPanel.Padding = new Padding(0, 10, 0, 0);
            this.buttonPanel.WrapContents = false;

            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Size = new Size(88, 34);
            this.btnCancel.Text = "取消";
            this.btnOpen.Size = new Size(88, 34);
            this.btnOpen.Text = "打开";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            this.btnNew.Size = new Size(100, 34);
            this.btnNew.Text = "新建产品";
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);

            this.AcceptButton = this.btnOpen;
            this.CancelButton = this.btnCancel;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.ClientSize = new Size(720, 460);
            this.Controls.Add(this.rootLayout);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProductSelectionDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "产品管理";

            this.rootLayout.ResumeLayout(false);
            ((ISupportInitialize)(this.dgvProducts)).EndInit();
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
