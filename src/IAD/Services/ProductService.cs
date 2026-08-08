using System;
using System.Collections.Generic;
using IAD.Models;
using IAD.Repositories;

namespace IAD.Services
{
    public sealed class ProductService
    {
        private readonly IProductRepository products;
        private readonly IProductDefinitionSettingsRepository settings;
        private readonly IDefectCategoryRepository categories;
        private readonly IRoiRepository rois;

        internal ProductService(
            IProductRepository products,
            IProductDefinitionSettingsRepository settings,
            IDefectCategoryRepository categories,
            IRoiRepository rois)
        {
            this.products = products ?? throw new ArgumentNullException("products");
            this.settings = settings ?? throw new ArgumentNullException("settings");
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

        public ProductDefinitionSettings GetDefinitionSettings(long productId)
        {
            EnsureProductExists(productId);
            ProductDefinitionSettings value = settings.GetByProduct(productId);
            return value ?? CreateDefaultSettings(productId);
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
            SaveDefinitionSettings(CreateDefaultSettings(product.Id));
            return product;
        }

        public void UpdateProduct(Product product)
        {
            if (product == null) throw new ArgumentNullException("product");
            if (product.Id <= 0) throw new ArgumentException("产品Id无效。", "product");

            product.ProductCode = NormalizeRequired(product.ProductCode, "产品编号");
            product.ProductName = NormalizeRequired(product.ProductName, "产品名称");

            Product sameCode = products.GetByCode(product.ProductCode);
            if (sameCode != null && sameCode.Id != product.Id)
                throw new InvalidOperationException("产品编号已存在：" + product.ProductCode);

            product.Description = NormalizeOptional(product.Description);
            product.UpdatedAtUtc = DateTime.UtcNow;
            products.Update(product);
        }

        public ProductDefinitionSettings SaveDefinitionSettings(ProductDefinitionSettings value)
        {
            if (value == null) throw new ArgumentNullException("value");
            EnsureProductExists(value.ProductId);
            if (value.ProductCount <= 0) throw new ArgumentException("单图产品数必须大于0。", "value");
            if (value.MatchCount <= 0) throw new ArgumentException("匹配数量必须大于0。", "value");
            if (value.MinScore < 0 || value.MinScore > 1) throw new ArgumentException("最小Score必须在0到1之间。", "value");
            if (value.PixelX <= 0 || value.PixelY <= 0) throw new ArgumentException("像素标定值必须大于0。", "value");

            value.ImageSize = NormalizeOptional(value.ImageSize);
            value.Pose = NormalizeOptional(value.Pose);
            value.AcquisitionCondition = NormalizeOptional(value.AcquisitionCondition);
            value.ReferenceImagePath = NormalizeOptional(value.ReferenceImagePath);
            value.TemplateType = NormalizeOptional(value.TemplateType);
            value.LocalizationMethod = NormalizeOptional(value.LocalizationMethod);
            value.ModelType = NormalizeOptional(value.ModelType);
            value.AngleRange = NormalizeOptional(value.AngleRange);
            value.ScaleRange = NormalizeOptional(value.ScaleRange);
            value.LengthUnit = NormalizeOptional(value.LengthUnit);
            value.AreaUnit = NormalizeOptional(value.AreaUnit);
            value.CalibrationVersion = NormalizeOptional(value.CalibrationVersion);
            value.CalibrationState = NormalizeOptional(value.CalibrationState);
            value.ProductDefinitionVersion = NormalizeOptional(value.ProductDefinitionVersion);
            value.TemplateVersion = NormalizeOptional(value.TemplateVersion);
            value.UpdatedAtUtc = DateTime.UtcNow;
            settings.Upsert(value);
            return value;
        }

        public DefectCategory SaveDefectCategory(DefectCategory category)
        {
            if (category == null) throw new ArgumentNullException("category");
            EnsureProductExists(category.ProductId);

            category.CategoryCode = NormalizeRequired(category.CategoryCode, "缺陷类别编码");
            category.CategoryName = NormalizeRequired(category.CategoryName, "缺陷类别名称");
            category.DefectType = NormalizeOptional(category.DefectType);
            category.DetectionStrategy = NormalizeOptional(category.DetectionStrategy);
            if (double.IsNaN(category.DefaultThreshold) || double.IsInfinity(category.DefaultThreshold) ||
                category.DefaultThreshold < 0 || category.DefaultThreshold > 1)
                throw new ArgumentException("默认阈值必须在0到1之间。", "category");
            if (double.IsNaN(category.MinArea) || double.IsInfinity(category.MinArea) || category.MinArea < 0)
                throw new ArgumentException("最小面积不能小于0。", "category");
            if (double.IsNaN(category.MinLength) || double.IsInfinity(category.MinLength) || category.MinLength < 0)
                throw new ArgumentException("最小长度不能小于0。", "category");
            if (category.DisplayOrder <= 0)
                throw new ArgumentException("显示顺序必须大于0。", "category");
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

        public void DeleteDefectCategory(long productId, long categoryId)
        {
            EnsureProductExists(productId);
            categories.Delete(categoryId, productId);
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

        public void DeleteRoi(long productId, long roiId)
        {
            EnsureProductExists(productId);
            rois.Delete(roiId, productId);
        }

        public void DeleteAllRois(long productId)
        {
            EnsureProductExists(productId);
            rois.DeleteByProduct(productId);
        }

        public static ProductDefinitionSettings CreateDefaultSettings(long productId)
        {
            return new ProductDefinitionSettings
            {
                ProductId = productId,
                ImageSize = string.Empty,
                ProductCount = 1,
                Pose = "允许旋转",
                AcquisitionCondition = string.Empty,
                ReferenceImagePath = string.Empty,
                TemplateType = "Shape Model",
                LocalizationMethod = "Shape Matching",
                ModelType = "Shape Model",
                MinScore = 0.8,
                AngleRange = "-180 ~ 180 deg",
                ScaleRange = "0.90 ~ 1.10",
                MatchCount = 1,
                PixelX = 1,
                PixelY = 1,
                LengthUnit = "px",
                AreaUnit = "px²",
                CalibrationVersion = "CAL-1.0.0",
                CalibrationState = "未标定",
                ProductDefinitionVersion = "PD-1.0.0",
                TemplateVersion = "LT-1.0.0",
                UpdatedAtUtc = DateTime.UtcNow
            };
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
