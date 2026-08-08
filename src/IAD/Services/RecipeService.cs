using System;
using System.Collections.Generic;
using IAD.Models;
using IAD.Repositories;

namespace IAD.Services
{
    public sealed class RecipeService
    {
        private readonly IProductRepository products;
        private readonly IInspectionRecipeRepository recipes;

        internal RecipeService(IProductRepository products, IInspectionRecipeRepository recipes)
        {
            this.products = products ?? throw new ArgumentNullException("products");
            this.recipes = recipes ?? throw new ArgumentNullException("recipes");
        }

        public IList<InspectionRecipe> GetRecipes(long productId)
        {
            EnsureProductExists(productId);
            return recipes.GetByProduct(productId);
        }

        public InspectionRecipe GetActiveRecipe(long productId)
        {
            EnsureProductExists(productId);
            return recipes.GetActiveByProduct(productId);
        }

        public InspectionRecipe SaveRecipe(InspectionRecipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException("recipe");
            EnsureProductExists(recipe.ProductId);

            recipe.RecipeCode = Require(recipe.RecipeCode, "Recipe编码");
            recipe.RecipeName = Require(recipe.RecipeName, "Recipe名称");
            DateTime now = DateTime.UtcNow;

            if (recipe.Id <= 0)
            {
                recipe.CreatedAtUtc = now;
                recipe.UpdatedAtUtc = now;
                recipe.Id = recipes.Insert(recipe);
            }
            else
            {
                recipe.UpdatedAtUtc = now;
                recipes.Update(recipe);
            }

            if (recipe.IsActive)
                recipes.Activate(recipe.ProductId, recipe.Id);

            return recipe;
        }

        public void ActivateRecipe(long productId, long recipeId)
        {
            EnsureProductExists(productId);
            InspectionRecipe recipe = recipes.GetById(recipeId);
            if (recipe == null || recipe.ProductId != productId)
                throw new InvalidOperationException("Recipe不存在或不属于当前产品。Id=" + recipeId);

            recipes.Activate(productId, recipeId);
        }

        private void EnsureProductExists(long productId)
        {
            if (productId <= 0 || products.GetById(productId) == null)
                throw new InvalidOperationException("产品不存在。Id=" + productId);
        }

        private static string Require(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(fieldName + "不能为空。", fieldName);
            return value.Trim();
        }
    }
}
