using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using IAD.Models;
using IAD.Services;

namespace IAD.Shell
{
    internal partial class RoiManagerDialog : Form
    {
        private readonly long productId;
        private long editingRoiId;

        public RoiManagerDialog(long productId)
        {
            this.productId = productId;
            InitializeComponent();
            LoadRois();
            ClearEditor();
        }

        private void LoadRois()
        {
            dgvRois.Rows.Clear();
            IList<RoiDefinition> rois = AppServices.Products.GetRois(productId);
            foreach (RoiDefinition roi in rois)
            {
                int index = dgvRois.Rows.Add(
                    roi.RoiName,
                    roi.RoiType,
                    roi.CenterX.ToString("0.###", CultureInfo.InvariantCulture),
                    roi.CenterY.ToString("0.###", CultureInfo.InvariantCulture),
                    roi.Width.ToString("0.###", CultureInfo.InvariantCulture),
                    roi.Height.ToString("0.###", CultureInfo.InvariantCulture),
                    roi.AngleDeg.ToString("0.###", CultureInfo.InvariantCulture),
                    roi.IsEnabled ? "启用" : "停用");
                dgvRois.Rows[index].Tag = roi.Id;
            }
        }

        private void ClearEditor()
        {
            editingRoiId = 0;
            txtName.Text = "Hole" + (dgvRois.Rows.Count + 1).ToString("00");
            cboType.SelectedIndex = 0;
            numCenterX.Value = 0;
            numCenterY.Value = 0;
            numWidth.Value = 60;
            numHeight.Value = 60;
            numAngle.Value = 0;
            numSort.Value = dgvRois.Rows.Count + 1;
            chkEnabled.Checked = true;
            btnSave.Text = "新增 ROI";
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearEditor();
            txtName.Focus();
            txtName.SelectAll();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvRois.CurrentRow == null || dgvRois.CurrentRow.Tag == null) return;
            long roiId = Convert.ToInt64(dgvRois.CurrentRow.Tag);
            RoiDefinition roi = null;
            IList<RoiDefinition> rois = AppServices.Products.GetRois(productId);
            foreach (RoiDefinition item in rois)
            {
                if (item.Id == roiId) { roi = item; break; }
            }
            if (roi == null) return;

            editingRoiId = roi.Id;
            txtName.Text = roi.RoiName;
            cboType.Text = roi.RoiType;
            numCenterX.Value = ClampDecimal(roi.CenterX, numCenterX.Minimum, numCenterX.Maximum);
            numCenterY.Value = ClampDecimal(roi.CenterY, numCenterY.Minimum, numCenterY.Maximum);
            numWidth.Value = ClampDecimal(roi.Width, numWidth.Minimum, numWidth.Maximum);
            numHeight.Value = ClampDecimal(roi.Height, numHeight.Minimum, numHeight.Maximum);
            numAngle.Value = ClampDecimal(roi.AngleDeg, numAngle.Minimum, numAngle.Maximum);
            numSort.Value = Math.Max(numSort.Minimum, Math.Min(numSort.Maximum, roi.SortIndex));
            chkEnabled.Checked = roi.IsEnabled;
            btnSave.Text = "保存修改";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                RoiDefinition roi = new RoiDefinition
                {
                    Id = editingRoiId,
                    ProductId = productId,
                    RoiName = txtName.Text,
                    RoiType = cboType.Text,
                    CenterX = Convert.ToDouble(numCenterX.Value),
                    CenterY = Convert.ToDouble(numCenterY.Value),
                    Width = Convert.ToDouble(numWidth.Value),
                    Height = Convert.ToDouble(numHeight.Value),
                    AngleDeg = Convert.ToDouble(numAngle.Value),
                    SortIndex = Convert.ToInt32(numSort.Value),
                    IsEnabled = chkEnabled.Checked
                };
                AppServices.Products.SaveRoi(roi);
                LoadRois();
                ClearEditor();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "ROI保存失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvRois.CurrentRow == null || dgvRois.CurrentRow.Tag == null) return;
            if (MessageBox.Show(this, "确定删除选中的 ROI？", "删除 ROI", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                AppServices.Products.DeleteRoi(productId, Convert.ToInt64(dgvRois.CurrentRow.Tag));
                LoadRois();
                ClearEditor();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "ROI删除失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgvRois_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) btnEdit_Click(sender, EventArgs.Empty);
        }

        private static decimal ClampDecimal(double value, decimal min, decimal max)
        {
            decimal d;
            try { d = Convert.ToDecimal(value); }
            catch { d = 0; }
            if (d < min) return min;
            if (d > max) return max;
            return d;
        }
    }
}
