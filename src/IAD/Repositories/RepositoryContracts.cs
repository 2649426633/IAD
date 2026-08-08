using System.Collections.Generic;
using IAD.Models;

namespace IAD.Repositories
{
    public interface IProductRepository
    {
        IList<Product> GetAll();
        Product GetById(long id);
        Product GetByCode(string productCode);
        long Insert(Product product);
        void Update(Product product);
    }

    public interface IDefectCategoryRepository
    {
        IList<DefectCategory> GetByProduct(long productId);
        DefectCategory GetById(long id);
        long Insert(DefectCategory category);
        void Update(DefectCategory category);
    }

    public interface IRoiRepository
    {
        IList<RoiDefinition> GetByProduct(long productId);
        RoiDefinition GetById(long id);
        long Insert(RoiDefinition roi);
        void Update(RoiDefinition roi);
    }

    public interface IInspectionRecipeRepository
    {
        IList<InspectionRecipe> GetByProduct(long productId);
        InspectionRecipe GetById(long id);
        InspectionRecipe GetActiveByProduct(long productId);
        long Insert(InspectionRecipe recipe);
        void Update(InspectionRecipe recipe);
        void Activate(long productId, long recipeId);
    }

    public interface IInspectionResultRepository
    {
        long Save(InspectionResult result);
        InspectionResult GetById(long id);
        IList<InspectionResult> GetRecent(int limit);
    }
}
