using System;
using System.Windows.Forms;
using IAD.Security;
using IAD.Services;
using IAD.Shell;

namespace IAD
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                AppServices.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "基础数据服务初始化失败，程序无法继续运行。\r\n\r\n" + ex.Message,
                    "初始化失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // ==================== 登录流程（开发阶段暂时停用） ====================
            // 后续正式启用登录时，恢复下面这段代码，并删除“开发模式直接进入主界面”代码即可。
            //
            // using (LoginForm loginForm = new LoginForm())
            // {
            //     if (loginForm.ShowDialog() != DialogResult.OK)
            //     {
            //         return;
            //     }
            // }
            //
            // using (MainForm mainForm = new MainForm())
            // {
            //     if (!mainForm.ApplyAuthenticatedSession())
            //     {
            //         return;
            //     }
            //
            //     Application.Run(mainForm);
            // }
            //
            // AppSession.SignOut();
            // ====================================================================

            // 开发模式：不经过登录页，直接进入主界面。
            Application.Run(new MainForm());
        }
    }
}
