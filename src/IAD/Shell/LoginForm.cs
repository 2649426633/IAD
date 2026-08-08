using System;
using System.Windows.Forms;
using IAD.Security;

namespace IAD.Shell
{
    public partial class LoginForm : Form
    {
        private const string DefaultPassword = "123456";

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;

            if (cboRole.SelectedIndex < 0)
            {
                lblError.Text = "请选择登录身份。";
                cboRole.Focus();
                return;
            }

            if (txtPassword.Text != DefaultPassword)
            {
                lblError.Text = "密码错误，请重新输入。";
                txtPassword.SelectAll();
                txtPassword.Focus();
                return;
            }

            AppSession.SignIn(cboRole.SelectedItem.ToString());
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            AppSession.SignOut();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void LoginForm_Shown(object sender, EventArgs e)
        {
            txtPassword.Focus();
        }
    }
}
