using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class DatasetAnnotationPage
    {
        private bool correctingEditButtonColor;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            dgvImages.Enter += NonCanvasControl_Enter;
            dgvQueue.Enter += NonCanvasControl_Enter;
            dgvClasses.Enter += NonCanvasControl_Enter;
            dgvLayers.Enter += NonCanvasControl_Enter;
            cboCurrentClass.Enter += NonCanvasControl_Enter;
            btnMaskEdit.BackColorChanged += btnMaskEdit_BackColorChanged;
            EnsureEditButtonHighlight();
        }

        private void NonCanvasControl_Enter(object sender, EventArgs e)
        {
            selectedAnnotationId = 0;
            CancelAnnotationEdit();
            if (pnlCanvas != null) pnlCanvas.Invalidate();
        }

        private void btnMaskEdit_BackColorChanged(object sender, EventArgs e)
        {
            EnsureEditButtonHighlight();
        }

        private void EnsureEditButtonHighlight()
        {
            if (correctingEditButtonColor || btnMaskEdit == null) return;
            Color desired = string.Equals(activeTool, "Select", StringComparison.Ordinal)
                ? Color.FromArgb(210, 225, 242)
                : UiTheme.Surface;
            if (btnMaskEdit.BackColor == desired) return;

            correctingEditButtonColor = true;
            try
            {
                btnMaskEdit.BackColor = desired;
            }
            finally
            {
                correctingEditButtonColor = false;
            }
        }
    }
}
