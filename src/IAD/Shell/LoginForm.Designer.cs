using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace IAD.Shell
{
    partial class LoginForm
    {
        private IContainer components = null;
        private TableLayoutPanel rootLayout;
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel loginPanel;
        private TableLayoutPanel loginLayout;
        private Label lblRole;
        private ComboBox cboRole;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblError;
        private FlowLayoutPanel buttonPanel;
        private Button btnLogin;
        private Button btnCancel;
        private Label lblFooter;

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
            this.lblSubtitle = new Label();
            this.loginPanel = new Panel();
            this.loginLayout = new TableLayoutPanel();
            this.lblRole = new Label();
            this.cboRole = new ComboBox();
            this.lblPassword = new Label();
            this.txtPassword = new TextBox();
            this.lblError = new Label();
            this.buttonPanel = new FlowLayoutPanel();
            this.btnLogin = new Button();
            this.btnCancel = new Button();
            this.lblFooter = new Label();
            this.rootLayout.SuspendLayout();
            this.loginPanel.SuspendLayout();
            this.loginLayout.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // rootLayout
            //
            this.rootLayout.BackColor = Color.FromArgb(244, 244, 244);
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.lblTitle, 0, 0);
            this.rootLayout.Controls.Add(this.lblSubtitle, 0, 1);
            this.rootLayout.Controls.Add(this.loginPanel, 0, 2);
            this.rootLayout.Controls.Add(this.lblFooter, 0, 3);
            this.rootLayout.Dock = DockStyle.Fill;
            this.rootLayout.Padding = new Padding(42, 28, 42, 20);
            this.rootLayout.RowCount = 4;
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            //
            // lblTitle
            //
            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.ForeColor = Color.FromArgb(28, 28, 28);
            this.lblTitle.Text = "通用工业瑕疵质检系统";
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblSubtitle
            //
            this.lblSubtitle.Dock = DockStyle.Fill;
            this.lblSubtitle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblSubtitle.ForeColor = Color.FromArgb(100, 100, 100);
            this.lblSubtitle.Text = "请选择身份并输入密码登录";
            this.lblSubtitle.TextAlign = ContentAlignment.TopCenter;
            //
            // loginPanel
            //
            this.loginPanel.BackColor = Color.White;
            this.loginPanel.BorderStyle = BorderStyle.FixedSingle;
            this.loginPanel.Controls.Add(this.loginLayout);
            this.loginPanel.Dock = DockStyle.Fill;
            this.loginPanel.Margin = new Padding(0, 8, 0, 8);
            //
            // loginLayout
            //
            this.loginLayout.ColumnCount = 2;
            this.loginLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82F));
            this.loginLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.loginLayout.Controls.Add(this.lblRole, 0, 0);
            this.loginLayout.Controls.Add(this.cboRole, 1, 0);
            this.loginLayout.Controls.Add(this.lblPassword, 0, 1);
            this.loginLayout.Controls.Add(this.txtPassword, 1, 1);
            this.loginLayout.Controls.Add(this.lblError, 1, 2);
            this.loginLayout.Controls.Add(this.buttonPanel, 1, 3);
            this.loginLayout.Dock = DockStyle.Fill;
            this.loginLayout.Padding = new Padding(28, 26, 28, 20);
            this.loginLayout.RowCount = 4;
            this.loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            this.loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            this.loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this.loginLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            //
            // lblRole
            //
            this.lblRole.Dock = DockStyle.Fill;
            this.lblRole.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblRole.ForeColor = Color.FromArgb(35, 35, 35);
            this.lblRole.Text = "身份";
            this.lblRole.TextAlign = ContentAlignment.MiddleLeft;
            //
            // cboRole
            //
            this.cboRole.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboRole.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.cboRole.FormattingEnabled = true;
            this.cboRole.Items.AddRange(new object[] { "操作员", "工程师", "管理员" });
            this.cboRole.SelectedIndex = 0;
            //
            // lblPassword
            //
            this.lblPassword.Dock = DockStyle.Fill;
            this.lblPassword.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblPassword.ForeColor = Color.FromArgb(35, 35, 35);
            this.lblPassword.Text = "密码";
            this.lblPassword.TextAlign = ContentAlignment.MiddleLeft;
            //
            // txtPassword
            //
            this.txtPassword.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.txtPassword.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.txtPassword.UseSystemPasswordChar = true;
            //
            // lblError
            //
            this.lblError.Dock = DockStyle.Fill;
            this.lblError.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblError.ForeColor = Color.FromArgb(120, 50, 50);
            this.lblError.TextAlign = ContentAlignment.MiddleLeft;
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.btnLogin);
            this.buttonPanel.Controls.Add(this.btnCancel);
            this.buttonPanel.Dock = DockStyle.Fill;
            this.buttonPanel.FlowDirection = FlowDirection.LeftToRight;
            this.buttonPanel.Padding = new Padding(0, 8, 0, 0);
            this.buttonPanel.WrapContents = false;
            //
            // btnLogin
            //
            this.btnLogin.BackColor = Color.FromArgb(40, 40, 40);
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.Margin = new Padding(0, 0, 10, 0);
            this.btnLogin.Size = new Size(110, 36);
            this.btnLogin.Text = "登录";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            //
            // btnCancel
            //
            this.btnCancel.BackColor = Color.White;
            this.btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            this.btnCancel.ForeColor = Color.FromArgb(45, 45, 45);
            this.btnCancel.Size = new Size(90, 36);
            this.btnCancel.Text = "退出";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // lblFooter
            //
            this.lblFooter.Dock = DockStyle.Fill;
            this.lblFooter.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblFooter.ForeColor = Color.FromArgb(120, 120, 120);
            this.lblFooter.Text = "离线工业质检工作站";
            this.lblFooter.TextAlign = ContentAlignment.MiddleCenter;
            //
            // LoginForm
            //
            this.AcceptButton = this.btnLogin;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = Color.FromArgb(244, 244, 244);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new Size(520, 390);
            this.Controls.Add(this.rootLayout);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "系统登录";
            this.Shown += new System.EventHandler(this.LoginForm_Shown);
            this.rootLayout.ResumeLayout(false);
            this.loginPanel.ResumeLayout(false);
            this.loginLayout.ResumeLayout(false);
            this.loginLayout.PerformLayout();
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
