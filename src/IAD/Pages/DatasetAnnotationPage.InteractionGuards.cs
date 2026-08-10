using System;
using System.Windows.Forms;

namespace IAD.Pages
{
    public partial class DatasetAnnotationPage
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            dgvImages.Enter += NonCanvasControl_Enter;
            dgvQueue.Enter += NonCanvasControl_Enter;
            dgvClasses.Enter += NonCanvasControl_Enter;
            dgvLayers.Enter += NonCanvasControl_Enter;
            cboCurrentClass.Enter += NonCanvasControl_Enter;
            InitializeMaskEditorRuntime();
            InitializeMaskDataIntegration();
            InitializeDatasetWorkflowUi();
        }

        private void NonCanvasControl_Enter(object sender, EventArgs e)
        {
            selectedAnnotationId = 0;
            CancelAnnotationEdit();
            if (pnlCanvas != null) pnlCanvas.Invalidate();
        }
    }
}
