namespace IAD.Shell
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private System.Windows.Forms.TableLayoutPanel headerLayout;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label projectLabel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.TableLayoutPanel bodyLayout;
        private System.Windows.Forms.FlowLayoutPanel navigationPanel;
        private System.Windows.Forms.Panel contentHost;
        private System.Windows.Forms.Label statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.headerLayout = new System.Windows.Forms.TableLayoutPanel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.projectLabel = new System.Windows.Forms.Label();
            this.closeButton = new System.Windows.Forms.Button();
            this.bodyLayout = new System.Windows.Forms.TableLayoutPanel();
            this.navigationPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.contentHost = new System.Windows.Forms.Panel();
            this.statusLabel = new System.Windows.Forms.Label();
            this.rootLayout.SuspendLayout();
            this.headerLayout.SuspendLayout();
            this.bodyLayout.SuspendLayout();
            this.SuspendLayout();
            // rootLayout
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.headerLayout, 0, 0);
            this.rootLayout.Controls.Add(this.bodyLayout, 0, 1);
            this.rootLayout.Controls.Add(this.statusLabel, 0, 2);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
            this.rootLayout.RowCount = 3;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            // headerLayout
            this.headerLayout.ColumnCount = 3;
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 320F));
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.headerLayout.Controls.Add(this.titleLabel, 0, 0);
            this.headerLayout.Controls.Add(this.projectLabel, 1, 0);
            this.headerLayout.Controls.Add(this.closeButton, 2, 0);
            this.headerLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerLayout.Margin = new System.Windows.Forms.Padding(0);
            // titleLabel
            this.titleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.titleLabel.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.titleLabel.Text = "通用工业瑕疵质检系统";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // projectLabel
            this.projectLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.projectLabel.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.projectLabel.Text = "项目： 单项目产线A";
            this.projectLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // closeButton
            this.closeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Text = "×";
            this.closeButton.TabStop = false;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // bodyLayout
            this.bodyLayout.ColumnCount = 2;
            this.bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bodyLayout.Controls.Add(this.navigationPanel, 0, 0);
            this.bodyLayout.Controls.Add(this.contentHost, 1, 0);
            this.bodyLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyLayout.Margin = new System.Windows.Forms.Padding(0);
            this.bodyLayout.RowCount = 1;
            this.bodyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            // navigationPanel
            this.navigationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.navigationPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.navigationPanel.WrapContents = false;
            this.navigationPanel.Padding = new System.Windows.Forms.Padding(8, 18, 8, 8);
            this.navigationPanel.Margin = new System.Windows.Forms.Padding(0);
            // contentHost
            this.contentHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentHost.Margin = new System.Windows.Forms.Padding(0);
            // statusLabel
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusLabel.Padding = new System.Windows.Forms.Padding(18, 0, 0, 0);
            this.statusLabel.Text = "CPU/GPU状态   |   HALCON Runtime   |   ONNX Runtime   |   SQLite   |   离线模式";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // MainForm
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1536, 864);
            this.Controls.Add(this.rootLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "通用工业瑕疵质检系统";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.rootLayout.ResumeLayout(false);
            this.headerLayout.ResumeLayout(false);
            this.bodyLayout.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
