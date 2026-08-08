using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IAD.Infrastructure.Database;
using IAD.Models;

namespace IAD.Repositories
{
    internal sealed class InspectionRecipeRepository : IInspectionRecipeRepository
    {
        private readonly SqliteConnectionFactory connectionFactory;

        public InspectionRecipeRepository(SqliteConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException("connectionFactory");
        }

        public IList<InspectionRecipe> GetByProduct(long productId)
        {
            List<InspectionRecipe> items = new List<InspectionRecipe>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Id, ProductId, RecipeCode, RecipeName, DatasetVersion, LocalizationTemplateVersion,
                                               ModelVersion, RuleVersion, CalibrationVersion, ThresholdVersion,
                                               IsActive, CreatedAtUtc, UpdatedAtUtc
                                        FROM InspectionRecipes WHERE ProductId = @ProductId
                                        ORDER BY IsActive DESC, UpdatedAtUtc DESC;";
                command.Parameters.AddWithValue("@ProductId", productId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) items.Add(Map(reader));
                }
            }
            return items;
        }

        public InspectionRecipe GetById(long id)
        {
            return GetSingle("Id = @Value", id);
        }

        public InspectionRecipe GetActiveByProduct(long productId)
        {
            return GetSingle("ProductId = @Value AND IsActive = 1 ORDER BY UpdatedAtUtc DESC", productId);
        }

        public long Insert(InspectionRecipe recipe)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO InspectionRecipes
                        (ProductId, RecipeCode, RecipeName, DatasetVersion, LocalizationTemplateVersion, ModelVersion, RuleVersion,
                         CalibrationVersion, ThresholdVersion, IsActive, CreatedAtUtc, UpdatedAtUtc)
                        VALUES (@ProductId, @RecipeCode, @RecipeName, @DatasetVersion, @LocalizationTemplateVersion, @ModelVersion, @RuleVersion,
                                @CalibrationVersion, @ThresholdVersion, @IsActive, @CreatedAtUtc, @UpdatedAtUtc);";
                    AddParameters(command, recipe);
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection))
                    return Convert.ToInt64(idCommand.ExecuteScalar());
            }
        }

        public void Update(InspectionRecipe recipe)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE InspectionRecipes SET
                    RecipeCode = @RecipeCode,
                    RecipeName = @RecipeName,
                    DatasetVersion = @DatasetVersion,
                    LocalizationTemplateVersion = @LocalizationTemplateVersion,
                    ModelVersion = @ModelVersion,
                    RuleVersion = @RuleVersion,
                    CalibrationVersion = @CalibrationVersion,
                    ThresholdVersion = @ThresholdVersion,
                    IsActive = @IsActive,
                    UpdatedAtUtc = @UpdatedAtUtc
                    WHERE Id = @Id AND ProductId = @ProductId;";
                AddParameters(command, recipe);
                command.Parameters.AddWithValue("@Id", recipe.Id);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要更新的Recipe。Id=" + recipe.Id);
            }
        }

        public void Activate(long productId, long recipeId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                using (SQLiteCommand clear = new SQLiteCommand("UPDATE InspectionRecipes SET IsActive = 0 WHERE ProductId = @ProductId;", connection, transaction))
                {
                    clear.Parameters.AddWithValue("@ProductId", productId);
                    clear.ExecuteNonQuery();
                }

                using (SQLiteCommand activate = new SQLiteCommand("UPDATE InspectionRecipes SET IsActive = 1, UpdatedAtUtc = @UpdatedAtUtc WHERE Id = @RecipeId AND ProductId = @ProductId;", connection, transaction))
                {
                    activate.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(DateTime.UtcNow));
                    activate.Parameters.AddWithValue("@RecipeId", recipeId);
                    activate.Parameters.AddWithValue("@ProductId", productId);
                    if (activate.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("未找到需要激活的Recipe。Id=" + recipeId);
                }

                transaction.Commit();
            }
        }

        private InspectionRecipe GetSingle(string whereSql, long value)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Id, ProductId, RecipeCode, RecipeName, DatasetVersion, LocalizationTemplateVersion,
                                               ModelVersion, RuleVersion, CalibrationVersion, ThresholdVersion,
                                               IsActive, CreatedAtUtc, UpdatedAtUtc
                                        FROM InspectionRecipes WHERE " + whereSql + " LIMIT 1;";
                command.Parameters.AddWithValue("@Value", value);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        private static void AddParameters(SQLiteCommand command, InspectionRecipe recipe)
        {
            command.Parameters.AddWithValue("@ProductId", recipe.ProductId);
            command.Parameters.AddWithValue("@RecipeCode", recipe.RecipeCode);
            command.Parameters.AddWithValue("@RecipeName", recipe.RecipeName);
            command.Parameters.AddWithValue("@DatasetVersion", DbConvert.DbNullIfEmpty(recipe.DatasetVersion));
            command.Parameters.AddWithValue("@LocalizationTemplateVersion", DbConvert.DbNullIfEmpty(recipe.LocalizationTemplateVersion));
            command.Parameters.AddWithValue("@ModelVersion", DbConvert.DbNullIfEmpty(recipe.ModelVersion));
            command.Parameters.AddWithValue("@RuleVersion", DbConvert.DbNullIfEmpty(recipe.RuleVersion));
            command.Parameters.AddWithValue("@CalibrationVersion", DbConvert.DbNullIfEmpty(recipe.CalibrationVersion));
            command.Parameters.AddWithValue("@ThresholdVersion", DbConvert.DbNullIfEmpty(recipe.ThresholdVersion));
            command.Parameters.AddWithValue("@IsActive", recipe.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("@CreatedAtUtc", DbConvert.ToUtcText(recipe.CreatedAtUtc));
            command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(recipe.UpdatedAtUtc));
        }

        private static InspectionRecipe Map(SQLiteDataReader reader)
        {
            return new InspectionRecipe
            {
                Id = DbConvert.GetInt64(reader, "Id"),
                ProductId = DbConvert.GetInt64(reader, "ProductId"),
                RecipeCode = DbConvert.GetString(reader, "RecipeCode"),
                RecipeName = DbConvert.GetString(reader, "RecipeName"),
                DatasetVersion = DbConvert.GetString(reader, "DatasetVersion"),
                LocalizationTemplateVersion = DbConvert.GetString(reader, "LocalizationTemplateVersion"),
                ModelVersion = DbConvert.GetString(reader, "ModelVersion"),
                RuleVersion = DbConvert.GetString(reader, "RuleVersion"),
                CalibrationVersion = DbConvert.GetString(reader, "CalibrationVersion"),
                ThresholdVersion = DbConvert.GetString(reader, "ThresholdVersion"),
                IsActive = DbConvert.GetBoolean(reader, "IsActive"),
                CreatedAtUtc = DbConvert.GetUtcDateTime(reader, "CreatedAtUtc"),
                UpdatedAtUtc = DbConvert.GetUtcDateTime(reader, "UpdatedAtUtc")
            };
        }
    }
}
