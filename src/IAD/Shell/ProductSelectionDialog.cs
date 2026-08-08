using System;
using System.Collections.Generic;
using System.Windows.Forms;
using IAD.Models;
using IAD.Services;

namespace IAD.Shell
{
    internal partial class ProductSelectionDialog : Form
    {
        public long SelectedProductId { get; private set; }
        public bool CreateNewRequested { get; private set; }

        public ProductSelectionDialog(long currentProductId)
        {
            InitializeComponent();
            LoadProducts(currentProductId);
        }

        private void LoadProducts(long currentProductId)
        {
            dgvProducts.Rows.Clear();
            IList<Product> products = AppServices.Products.GetAllProducts();
            foreach (Product product in products)
            {
                int rowIndex = dgvProducts.Rows.Add(product.ProductCode, product.ProductName, product.IsActive ? "启用" : "停用", product.UpdatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
                dgvProducts.Rows[rowIndex].Tag = product.Id;
                if (product.Id == currentProductId)
                    dgvProducts.Rows[rowIndex].Selected = true;
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            CreateNewRequested = true;
            SelectedProductId = 0;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null || dgvProducts.CurrentRow.Tag == null)
            {
                MessageBox.Show(this, "请选择一个产品。", "产品选择", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SelectedProductId = Convert.ToInt64(dgvProducts.CurrentRow.Tag);
            CreateNewRequested = false;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void dgvProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) btnOpen_Click(sender, EventArgs.Empty);
        }
    }
}
