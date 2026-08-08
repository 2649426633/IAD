using System.Windows.Forms;
using IAD.UI;

namespace IAD.Pages
{
    public partial class TrainingModelsPage
    {
        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Page;
            Padding = new Padding(14, 14, 4, 10);

            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = UiTheme.Page };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 245F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 190F));

            TableLayoutPanel top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = UiTheme.Page };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27F));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27F));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
            top.Controls.Add(UiFactory.Card("训练配置", UiFactory.KeyValues(new[,] { { "模型结构：", "SegFormer-B2" }, { "Tile Size：", "1024 × 1024" }, { "Batch Size：", "16" }, { "Epoch：", "100" }, { "学习率：", "0.0001" }, { "数据增强：", "翻转 / 旋转 / 抖动 / 裁剪" }, { "GPU/CPU：", "自动选择" } }, 42)), 0, 0);
            top.Controls.Add(UiFactory.Card("数据集划分", UiFactory.Grid(new[] { "集合", "图片数", "占比", "缺陷实例", "占比" }, new[] { new[] { "Train", "18,732", "70%", "98,732", "70.1%" }, new[] { "Validation", "4,027", "15%", "21,365", "15.2%" }, new[] { "Acceptance", "4,027", "15%", "21,402", "14.7%" } })), 1, 0);
            top.Controls.Add(UiFactory.Card("训练任务队列", UiFactory.Grid(new[] { "任务ID", "模型", "数据集", "提交时间", "状态", "优先级" }, new[] { new[] { "TRN-005", "SegFormer-B2", "V2.1.0", "14:25:32", "训练中", "高" }, new[] { "TRN-004", "UNet", "V2.1.0", "13:48:21", "排队中", "中" }, new[] { "TRN-003", "Model-X", "V2.1.0", "13:30:11", "等待资源", "低" }, new[] { "TRN-002", "SegFormer-B1", "V2.1.0", "12:55:06", "已完成", "中" } })), 2, 0);
            root.Controls.Add(top, 0, 0);

            TableLayoutPanel mid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = UiTheme.Page };
            mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            TextBox log = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                Font = UiTheme.Font(8.2F, false),
                Text = "当前任务：TRN-20250516-005 (SegFormer-B2)\r\n进度：Epoch 38 / 100 (38%)\r\n预计剩余：00:42:17\r\n当前学习率：0.000087\r\n最佳验证集 mIoU：0.7421 (Epoch 32)\r\n\r\n[14:41:02] Loss 0.3867  Dice 0.7183  mIoU 0.7112\r\n[14:41:46] Loss 0.3681  Dice 0.7245  mIoU 0.7169\r\n[14:43:15] Validation mIoU 0.7421  Recall 0.7325  Precision 0.7719\r\n[14:43:15] New best model saved."
            };
            mid.Controls.Add(UiFactory.Card("训练状态 / 实时日志", log), 0, 0);
            mid.Controls.Add(UiFactory.Card("模型基准对比（验证集）", UiFactory.Grid(new[] { "模型", "Recall", "Precision", "F1", "mIoU", "小缺陷召回", "推理ms", "显存GB" }, new[] { new[] { "SegFormer-B2", "0.7325", "0.7719", "0.7518", "0.7421", "0.6123", "42.3", "6.21" }, new[] { "UNet", "0.7086", "0.7472", "0.7274", "0.7098", "0.5891", "31.7", "5.03" }, new[] { "Model-X", "0.7243", "0.7581", "0.7410", "0.7312", "0.6048", "38.9", "5.76" } })), 1, 0);
            root.Controls.Add(mid, 0, 1);

            TableLayoutPanel bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = UiTheme.Page };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 69F));
            bottom.Controls.Add(UiFactory.Card("验证与验收", UiFactory.KeyValues(new[,] { { "Validation mIoU：", "0.7421" }, { "F1：", "0.7518" }, { "Recall：", "0.7325" }, { "Precision：", "0.7719" }, { "Acceptance：", "待评估" } }, 52)), 0, 0);
            bottom.Controls.Add(UiFactory.Card("模型库", UiFactory.Grid(new[] { "版本", "结构", "数据集", "训练时间", "mIoU", "状态", "SHA256", "操作" }, new[] { new[] { "V2.1.0", "SegFormer-B2", "V2.1.0", "05-16", "0.7398", "已发布", "3e6f7a9c...", "导出/停用/回滚" }, new[] { "V2.0.0", "SegFormer-B1", "V2.0.0", "05-14", "0.7216", "已停用", "7b1c2d3e...", "导出/发布/回滚" }, new[] { "V1.3.0", "UNet", "V1.3.0", "05-12", "0.6991", "已停用", "9a8b7c6d...", "导出/发布/回滚" } })), 1, 0);
            root.Controls.Add(bottom, 0, 2);
            Controls.Add(root);
        }
    }
}
