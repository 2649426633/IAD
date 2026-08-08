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
        private TableLayoutPanel buttonLayout;
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
            this.buttonLayout = new TableLayoutPanel();
            this.btnLogin = new Button();
            this.btnCancel = new Button();
            this.lblFooter = new Label();
            this.rootLayout.SuspendLayout();
            this.loginPanel.SuspendLayout();
            this.loginLayout.SuspendLayout();
            this.buttonLayout.SuspendLayout();
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
            this.rootLayout.Margin = new Padding(0);
            this.rootLayout.Padding = new Padding(54, 34, 54, 24);
            this.rootLayout.RowCount = 4;
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            //
            // lblTitle
            //
            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.Font = new Font("Microsoft YaHei UI", 17F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.ForeColor = Color.FromArgb(28, 28, 28);
            this.lblTitle.Text = "通用工业瑕疵质检系统";
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblSubtitle
            //
            this.lblSubtitle.Dock = DockStyle.Fill;
            this.lblSubtitle.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
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
            this.loginPanel.Margin = new Padding(16, 10, 16, 10);
            this.loginPanel.MinimumSize = new Size(430, 270);
            //
            // loginLayout
            //
            this.loginLayout.ColumnCount = 2;
            this.loginLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
            this.loginLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.loginLayout.Controls.Add(this.lblRole, 0, 0);
            this.loginLayout.Controls.Add(this.cboRole, 1, 0);
            this.loginLayout.Controls.Add(this.lblPassword, 0, 1);
            this.loginLayout.Controls.Add(this.txtPassword, 1, 1);
            this.loginLayout.Controls.Add(this.lblError, 1, 2);
            this.loginLayout.Controls.Add(this.buttonLayout, 0, 3);
            this.loginLayout.SetColumnSpan(this.buttonLayout, 2);
            this.loginLayout.Dock = DockStyle.Fill;
            this.loginLayout.Margin = new Padding(0);
            this.loginLayout.Padding = new Padding(34, 28, 34, 24);
            this.loginLayout.RowCount = 4;
            this.loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            this.loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            this.loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
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
            this.cboRole.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
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
            this.txtPassword.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
            this.txtPassword.UseSystemPasswordChar = true;
            //
            // lblError
            //
            this.lblError.Dock = DockStyle.Fill;
            this.lblError.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblError.ForeColor = Color.FromArgb(120, 50, 50);
            this.lblError.TextAlign = ContentAlignment.MiddleLeft;
            //
            // buttonLayout
            //
            this.buttonLayout.ColumnCount = 4;
            this.buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126F));
            this.buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108F));
            this.buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.buttonLayout.Controls.Add(this.btnLogin, 1, 0);
            this.buttonLayout.Controls.Add(this.btnCancel, 2, 0);
            this.buttonLayout.Dock = DockStyle.Fill;
            this.buttonLayout.Margin = new Padding(0);
            this.buttonLayout.Padding = new Padding(0, 8, 0, 0);
            this.buttonLayout.RowCount = 1;
            this.buttonLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            //
            // btnLogin
            //
            this.btnLogin.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.btnLogin.BackColor = Color.FromArgb(40, 40, 40);
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.Margin = new Padding(0, 0, 10, 0);
            this.btnLogin.Height = 40;
            this.btnLogin.Text = "登录";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.btnCancel.BackColor = Color.White;
            this.btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            this.btnCancel.ForeColor = Color.FromArgb(45, 45, 45);
            this.btnCancel.Margin = new Padding(0);
            this.btnCancel.Height = 40;
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
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = Color.FromArgb(244, 244, 244);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new Size(620, 480);
            this.Controls.Add(this.rootLayout);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new Size(636, 519);
            this.Name = "LoginForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "系统登录";
            this.Shown += new System.EventHandler(this.LoginForm_Shown);
            this.rootLayout.ResumeLayout(false);
            this.loginPanel.ResumeLayout(false);
            this.loginLayout.ResumeLayout(false);
            this.loginLayout.PerformLayout();
            this.buttonLayout.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
