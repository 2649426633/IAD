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

            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            using (MainForm mainForm = new MainForm())
            {
                if (!mainForm.ApplyAuthenticatedSession())
                {
                    return;
                }

                Application.Run(mainForm);
            }

            AppSession.SignOut();
        }
    }
}
