using System;
using System.Windows.Forms;
using IAD.Security;
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
