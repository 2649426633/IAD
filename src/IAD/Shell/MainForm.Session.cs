using IAD.Security;

namespace IAD.Shell
{
    public partial class MainForm
    {
        internal bool ApplyAuthenticatedSession()
        {
            if (!AppSession.IsAuthenticated)
            {
                return false;
            }

            statusLabel.Text = "CPU/GPU状态   |   HALCON Runtime   |   ONNX Runtime   |   SQLite   |   离线模式   |   当前身份：" + AppSession.CurrentRole;
            return true;
        }
    }
}
