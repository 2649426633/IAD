using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using IAD.UI;

namespace IAD.Pages
{
    public partial class SystemSettingsPage
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(8F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(247, 247, 247);
            this.Name = "SystemSettingsPage";
            this.Size = new Size(1400, 820);
            this.ResumeLayout(false);
        }

        private void BuildView()
        {
            this.SuspendLayout();
            this.Controls.Clear();
            this.Dock = DockStyle.Fill;
            this.BackColor = UiTheme.Page;
            this.Padding = new Padding(14, 14, 4, 10);

            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));

            root.Controls.Add(BuildFirstRow(), 0, 0);
            root.Controls.Add(BuildSecondRow(), 0, 1);
            root.Controls.Add(BuildThirdRow(), 0, 2);

            this.Controls.Add(root);
            this.ResumeLayout(true);
        }

        private Control BuildFirstRow()
        {
            TableLayoutPanel row = ThreeColumns();
            row.Controls.Add(UiFactory.Card("运行时与硬件配置", UiFactory.KeyValues(new[,] {
                { "CPU：", "Intel Core i7-12700K" }, { "GPU：", "NVIDIA RTX 3070 8GB" }, { "HALCON Runtime：", "24.11" },
                { "ONNX Runtime：", "1.16.3 / CUDA" }, { "Python Runtime：", "3.10.14" }, { "TensorRT：", "8.6.1.6" }
            }, 42)), 0, 0);

            row.Controls.Add(UiFactory.Card("存储路径配置", UiFactory.KeyValues(new[,] {
                { "Images：", "D:\\InspectSys\\Data\\Images" }, { "Masks：", "D:\\InspectSys\\Data\\Masks" },
                { "Models：", "D:\\InspectSys\\Models" }, { "Results：", "D:\\InspectSys\\Results" },
                { "Logs：", "D:\\InspectSys\\Logs" }, { "Cache：", "D:\\InspectSys\\Cache" }
            }, 31)), 1, 0);

            row.Controls.Add(UiFactory.Card("离线部署与包状态", UiFactory.KeyValues(new[,] {
                { "部署包版本：", "v1.3.0" }, { "部署模式：", "单机离线部署" }, { "目标平台：", "Windows 10/11 x64" },
                { "依赖：", "Python / ONNX / HALCON" }, { "包大小：", "1.26 GB" }, { "状态：", "已就绪" }
            }, 42)), 2, 0);
            return row;
        }

        private Control BuildSecondRow()
        {
            TableLayoutPanel row = ThreeColumns();
            row.Controls.Add(UiFactory.Card("日志与异常设置", UiFactory.KeyValues(new[,] {
                { "日志级别：", "INFO" }, { "保留天数：", "30 天" }, { "单文件大小：", "100 MB" },
                { "异常策略：", "记录并继续运行" }, { "报警：", "仅记录日志" }
            }, 42)), 0, 0);

            row.Controls.Add(UiFactory.Card("备份与恢复设置", UiFactory.KeyValues(new[,] {
                { "自动备份：", "启用" }, { "周期：", "每天 02:00" }, { "保留数量：", "7 份" },
                { "备份路径：", "D:\\InspectSys\\Backup" }, { "恢复：", "人工选择备份文件" }
            }, 42)), 1, 0);

            row.Controls.Add(UiFactory.Card("权限与用户角色", UiFactory.Grid(
                new[] { "角色", "用户数", "主要权限" },
                new[] {
                    new[] { "管理员", "2", "系统配置/全部模块" },
                    new[] { "工程师", "4", "模型/规则/部分设置" },
                    new[] { "操作员", "12", "检测/结果/追溯" },
                    new[] { "访客", "3", "结果查看" }
                })), 2, 0);
            return row;
        }

        private Control BuildThirdRow()
        {
            TableLayoutPanel row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 78F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));

            row.Controls.Add(UiFactory.Card("适配器配置（预留 / 集成）", UiFactory.Grid(
                new[] { "适配器", "状态", "类型", "地址/路径", "备注" },
                new[] {
                    new[] { "Camera", "预留", "GigE Vision", "192.168.1.100", "后续相机接入" },
                    new[] { "PLC", "预留", "Modbus TCP", "192.168.1.200:502", "OK/NG输出" },
                    new[] { "MES", "预留", "HTTP REST", "192.168.1.210/api", "生产系统" },
                    new[] { "Result Export", "已配置", "CSV + PNG", "D:\\InspectSys\\Export", "结果导出" }
                })), 0, 0);

            row.Controls.Add(UiFactory.Card("版本信息", UiFactory.KeyValues(new[,] {
                { "系统版本：", "V1.0.0" }, { "构建版本：", "20260807" }, { "部署状态：", "离线就绪" },
                { "存储模式：", "本地" }, { "当前用户：", "admin" }
            }, 45)), 1, 0);
            return row;
        }

        private TableLayoutPanel ThreeColumns()
        {
            TableLayoutPanel row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = UiTheme.Page,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            return row;
        }
    }
}
