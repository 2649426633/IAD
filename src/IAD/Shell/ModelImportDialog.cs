using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using IAD.Models;

namespace IAD.Shell
{
    internal sealed class ModelImportDialog : Form
    {
        private readonly TextBox txtCode = new TextBox();
        private readonly TextBox txtName = new TextBox();
        private readonly TextBox txtVersion = new TextBox();
        private readonly ComboBox cboType = new ComboBox();
        private readonly NumericUpDown numWidth = new NumericUpDown();
        private readonly NumericUpDown numHeight = new NumericUpDown();
        private readonly NumericUpDown numConfidence = new NumericUpDown();
        private readonly NumericUpDown numNms = new NumericUpDown();
        private readonly TextBox txtLabels = new TextBox();
        private readonly CheckBox chkActive = new CheckBox();

        public ModelImportDialog(string sourcePath, IEnumerable<DefectCategory> categories)
        {
            Text = "导入 ONNX 模型";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(560, 500);
            Font = new Font("Microsoft YaHei UI", 9F);

            string baseName = Path.GetFileNameWithoutExtension(sourcePath);
            txtCode.Text = baseName;
            txtName.Text = baseName;
            txtVersion.Text = "V" + DateTime.Now.ToString("yyyyMMdd-HHmm");
            cboType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboType.Items.AddRange(new object[] { "Classification", "YoloV5", "YoloV8", "Yolo26" });
            cboType.SelectedIndex = 3;
            cboType.SelectedIndexChanged += delegate
            {
                if (Convert.ToString(cboType.SelectedItem) == "Classification" && !txtLabels.Text.TrimStart().StartsWith("normal", StringComparison.OrdinalIgnoreCase))
                    txtLabels.Text = "normal" + Environment.NewLine + txtLabels.Text;
            };
            ConfigureDimension(numWidth);
            ConfigureDimension(numHeight);
            ConfigureThreshold(numConfidence, 0.50M);
            ConfigureThreshold(numNms, 0.45M);
            txtLabels.Multiline = true;
            txtLabels.ScrollBars = ScrollBars.Vertical;
            txtLabels.Text = string.Join(Environment.NewLine, (categories ?? Enumerable.Empty<DefectCategory>()).Where(c => c.IsEnabled).OrderBy(c => c.DisplayOrder).Select(c => c.CategoryCode));
            chkActive.Text = "导入后立即启用";
            chkActive.Checked = true;

            TableLayoutPanel layout = new TableLayoutPanel { Dock=DockStyle.Fill, Padding=new Padding(14), ColumnCount=2, RowCount=12 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i=0; i<9; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            AddRow(layout, 0, "模型文件", new Label { Text=Path.GetFileName(sourcePath), AutoEllipsis=true, Dock=DockStyle.Fill, TextAlign=ContentAlignment.MiddleLeft });
            AddRow(layout, 1, "模型编号", txtCode);
            AddRow(layout, 2, "模型名称", txtName);
            AddRow(layout, 3, "版本", txtVersion);
            AddRow(layout, 4, "输出格式", cboType);
            AddRow(layout, 5, "输入宽度（0=自动）", numWidth);
            AddRow(layout, 6, "输入高度（0=自动）", numHeight);
            AddRow(layout, 7, "置信度阈值", numConfidence);
            AddRow(layout, 8, "NMS 阈值", numNms);
            AddRow(layout, 9, "类别顺序", txtLabels);
            layout.Controls.Add(chkActive, 1, 10);
            FlowLayoutPanel buttons = new FlowLayoutPanel { Dock=DockStyle.Fill, FlowDirection=FlowDirection.RightToLeft };
            Button ok = new Button { Text="导入", DialogResult=DialogResult.OK, Width=86, Height=30 };
            Button cancel = new Button { Text="取消", DialogResult=DialogResult.Cancel, Width=86, Height=30 };
            buttons.Controls.Add(ok); buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons, 1, 11);
            Controls.Add(layout);
            AcceptButton = ok;
            CancelButton = cancel;
            ok.Click += ValidateBeforeClose;
        }

        public InferenceModel CreateDefinition(long productId)
        {
            return new InferenceModel
            {
                ProductId=productId, ModelCode=txtCode.Text, ModelName=txtName.Text, Version=txtVersion.Text,
                ModelType=Convert.ToString(cboType.SelectedItem), InputWidth=(int)numWidth.Value, InputHeight=(int)numHeight.Value,
                ConfidenceThreshold=(double)numConfidence.Value, NmsThreshold=(double)numNms.Value,
                Labels=txtLabels.Text, IsActive=chkActive.Checked
            };
        }

        private void ValidateBeforeClose(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtVersion.Text))
            {
                MessageBox.Show(this, "模型编号、名称和版本不能为空。", "导入模型", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        }

        private static void ConfigureDimension(NumericUpDown control) { control.Minimum=0; control.Maximum=8192; control.Increment=32; control.Dock=DockStyle.Fill; }
        private static void ConfigureThreshold(NumericUpDown control, decimal value) { control.Minimum=0; control.Maximum=1; control.DecimalPlaces=2; control.Increment=0.05M; control.Value=value; control.Dock=DockStyle.Fill; }
        private static void AddRow(TableLayoutPanel layout, int row, string title, Control control)
        {
            layout.Controls.Add(new Label { Text=title, Dock=DockStyle.Fill, TextAlign=ContentAlignment.MiddleLeft }, 0, row);
            control.Dock = DockStyle.Fill;
            layout.Controls.Add(control, 1, row);
        }
    }
}
