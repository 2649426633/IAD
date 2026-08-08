using System;
using System.Collections.Generic;
using IAD.Models;
using IAD.Repositories;

namespace IAD.Services
{
    public sealed class ProductService
    {
        private readonly IProductRepository products;
        private readonly IDefectCategoryRepository categories;
        private readonly IRoiRepository rois;

        internal ProductService(IProductRepository products, IDefectCategoryRepository categories, IRoiRepository rois)
        {
            this.products = products ?? throw new ArgumentNullException("products");
            this.categories = categories ?? throw new ArgumentNullException("categories");
            this.rois = rois ?? throw new ArgumentNullException("rois");
        }

        public IList<Product> GetAllProducts()
        {
            return products.GetAll();
        }

        public Product GetProduct(long productId)
        {
            return products.GetById(productId);
        }

        public IList<DefectCategory> GetDefectCategories(long productId)
        {
            return categories.GetByProduct(productId);
        }

        public IList<RoiDefinition> GetRois(long productId)
        {
            return rois.GetByProduct(productId);
        }

        public Product CreateProduct(string productCode, string productName, string description)
        {
            productCode = NormalizeRequired(productCode, "产品编号");
            productName = NormalizeRequired(productName, "产品名称");

            if (products.GetByCode(productCode) != null)
                throw new InvalidOperationException("产品编号已存在：" + productCode);

            DateTime now = DateTime.UtcNow;
            Product product = new Product
            {
                ProductCode = productCode,
                ProductName = productName,
                Description = NormalizeOptional(description),
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            product.Id = products.Insert(product);
            return product;
        }

        public void UpdateProduct(Product product)
        {
            if (product == null) throw new ArgumentNullException("product");
            if (product.Id <= 0) throw new ArgumentException("产品Id无效。", "product");

            product.ProductCode = NormalizeRequired(product.ProductCode, "产品编号");
            product.ProductName = NormalizeRequired(product.ProductName, "产品名称");
            product.Description = NormalizeOptional(product.Description);
            product.UpdatedAtUtc = DateTime.UtcNow;
            products.Update(product);
        }

        public DefectCategory SaveDefectCategory(DefectCategory category)
        {
            if (category == null) throw new ArgumentNullException("category");
            EnsureProductExists(category.ProductId);

            category.CategoryCode = NormalizeRequired(category.CategoryCode, "缺陷类别编码");
            category.CategoryName = NormalizeRequired(category.CategoryName, "缺陷类别名称");
            DateTime now = DateTime.UtcNow;

            if (category.Id <= 0)
            {
                category.CreatedAtUtc = now;
                category.UpdatedAtUtc = now;
                category.Id = categories.Insert(category);
            }
            else
            {
                category.UpdatedAtUtc = now;
                categories.Update(category);
            }

            return category;
        }

        public RoiDefinition SaveRoi(RoiDefinition roi)
        {
            if (roi == null) throw new ArgumentNullException("roi");
            EnsureProductExists(roi.ProductId);

            roi.RoiName = NormalizeRequired(roi.RoiName, "ROI名称");
            roi.RoiType = NormalizeRequired(roi.RoiType, "ROI类型");
            if (roi.Width < 0 || roi.Height < 0)
                throw new ArgumentException("ROI宽高不能为负数。", "roi");

            DateTime now = DateTime.UtcNow;
            if (roi.Id <= 0)
            {
                roi.CreatedAtUtc = now;
                roi.UpdatedAtUtc = now;
                roi.Id = rois.Insert(roi);
            }
            else
            {
                roi.UpdatedAtUtc = now;
                rois.Update(roi);
            }

            return roi;
        }

        private void EnsureProductExists(long productId)
        {
            if (productId <= 0 || products.GetById(productId) == null)
                throw new InvalidOperationException("产品不存在。Id=" + productId);
        }

        private static string NormalizeRequired(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(fieldName + "不能为空。", fieldName);
            return value.Trim();
        }

        private static string NormalizeOptional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
