using System;
using System.Collections.Generic;
using System.Windows.Forms;
using IAD.Models;
using IAD.Services;

namespace IAD.Pages
{
    public partial class DatasetAnnotationPage
    {
        private bool maskDataIntegrationInitialized;

        private void InitializeMaskDataIntegration()
        {
            if (maskDataIntegrationInitialized) return;
            maskDataIntegrationInitialized = true;

            dgvImages.SelectionChanged += delegate { ScheduleMaskAwareUiRefresh(); };
            cboCurrentClass.SelectedIndexChanged += delegate { ScheduleMaskAwareUiRefresh(); };
            pnlCanvas.MouseUp += delegate { ScheduleMaskAwareUiRefresh(); };

            if (btnMaskRasterize != null) btnMaskRasterize.Click += delegate { ScheduleMaskAwareUiRefresh(); };
            if (btnMaskUndo != null) btnMaskUndo.Click += delegate { ScheduleMaskAwareUiRefresh(); };
            if (btnMaskRedo != null) btnMaskRedo.Click += delegate { ScheduleMaskAwareUiRefresh(); };
            if (btnMaskClear != null) btnMaskClear.Click += delegate { ScheduleMaskAwareUiRefresh(); };

            ScheduleMaskAwareUiRefresh();
        }

        private void ScheduleMaskAwareUiRefresh()
        {
            if (IsDisposed || Disposing || !IsHandleCreated) return;
            BeginInvoke(new MethodInvoker(delegate
            {
                if (IsDisposed || Disposing) return;
                RefreshMaskAwareQueueCounts();
                RefreshCurrentImageMaskState();
            }));
        }

        private void RefreshMaskAwareQueueCounts()
        {
            if (datasetImages == null || dgvQueue == null) return;

            IDictionary<long, int> classCounts;
            try
            {
                classCounts = currentProduct == null
                    ? new Dictionary<long, int>()
                    : AppServices.Datasets.GetClassCounts(currentProduct.Id);
            }
            catch
            {
                return;
            }

            foreach (DataGridViewRow row in dgvQueue.Rows)
            {
                DatasetImage image = row.Tag as DatasetImage;
                if (image == null) continue;
                int count;
                classCounts.TryGetValue(image.Id, out count);
                if (row.Cells.Count > 3) row.Cells[3].Value = count.ToString();
            }
        }

        private void RefreshCurrentImageMaskState()
        {
            if (currentImage == null) return;
            try
            {
                bool hasAnnotations = AppServices.Datasets.GetAnnotations(currentImage.Id).Count > 0;
                bool hasMasks = AppServices.Masks.GetMasks(currentImage.Id).Count > 0;
                string status = hasAnnotations || hasMasks ? "已标注" : "未标注";
                currentImage.Status = status;

                foreach (DataGridViewRow row in dgvImages.Rows)
                {
                    DatasetImage image = row.Tag as DatasetImage;
                    if (image == null || image.Id != currentImage.Id) continue;
                    if (row.Cells.Count > 1) row.Cells[1].Value = status;
                    break;
                }

                foreach (DataGridViewRow row in dgvQueue.Rows)
                {
                    DatasetImage image = row.Tag as DatasetImage;
                    if (image == null || image.Id != currentImage.Id) continue;
                    if (row.Cells.Count > 2) row.Cells[2].Value = status;
                    break;
                }
            }
            catch
            {
                // UI 汇总刷新失败不覆盖已成功写入的 Mask 数据。
            }
        }
    }
}
