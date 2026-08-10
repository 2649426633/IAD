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
                                               ModelId, IsActive, CreatedAtUtc, UpdatedAtUtc
                                        FROM InspectionRecipes WHERE ProductId = @ProductId
                                        ORDER BY IsActive DESC, UpdatedAtUtc DESC;";
                command.Parameters.AddWithValue("@ProductId", productId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) items.Add(Map(reader));
                }
                foreach (InspectionRecipe item in items) LoadRules(connection, item);
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
                         CalibrationVersion, ThresholdVersion, ModelId, IsActive, CreatedAtUtc, UpdatedAtUtc)
                        VALUES (@ProductId, @RecipeCode, @RecipeName, @DatasetVersion, @LocalizationTemplateVersion, @ModelVersion, @RuleVersion,
                                @CalibrationVersion, @ThresholdVersion, @ModelId, @IsActive, @CreatedAtUtc, @UpdatedAtUtc);";
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
                    ModelId = @ModelId,
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

        public void ReplaceRules(long recipeId, IList<RecipeRule> rules)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                using (SQLiteCommand delete = new SQLiteCommand("DELETE FROM RecipeRules WHERE RecipeId=@RecipeId;", connection, transaction))
                {
                    delete.Parameters.AddWithValue("@RecipeId", recipeId);
                    delete.ExecuteNonQuery();
                }
                foreach (RecipeRule rule in rules ?? new List<RecipeRule>())
                {
                    using (SQLiteCommand command = new SQLiteCommand(@"INSERT INTO RecipeRules
                        (RecipeId, CategoryId, CategoryCode, CategoryName, RoiName, MinConfidence, MinArea, MinWidth, MinHeight, MaxAllowedCount, Decision, IsEnabled)
                        VALUES (@RecipeId,@CategoryId,@CategoryCode,@CategoryName,@RoiName,@MinConfidence,@MinArea,@MinWidth,@MinHeight,@MaxAllowedCount,@Decision,@IsEnabled);", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@RecipeId", recipeId);
                        command.Parameters.AddWithValue("@CategoryId", DbConvert.DbNullIfMissing(rule.CategoryId));
                        command.Parameters.AddWithValue("@CategoryCode", rule.CategoryCode);
                        command.Parameters.AddWithValue("@CategoryName", DbConvert.DbNullIfEmpty(rule.CategoryName));
                        command.Parameters.AddWithValue("@RoiName", DbConvert.DbNullIfEmpty(rule.RoiName));
                        command.Parameters.AddWithValue("@MinConfidence", rule.MinConfidence);
                        command.Parameters.AddWithValue("@MinArea", rule.MinArea);
                        command.Parameters.AddWithValue("@MinWidth", rule.MinWidth);
                        command.Parameters.AddWithValue("@MinHeight", rule.MinHeight);
                        command.Parameters.AddWithValue("@MaxAllowedCount", rule.MaxAllowedCount);
                        command.Parameters.AddWithValue("@Decision", rule.Decision);
                        command.Parameters.AddWithValue("@IsEnabled", rule.IsEnabled ? 1 : 0);
                        command.ExecuteNonQuery();
                    }
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
                                               ModelId, IsActive, CreatedAtUtc, UpdatedAtUtc
                                        FROM InspectionRecipes WHERE " + whereSql + " LIMIT 1;";
                command.Parameters.AddWithValue("@Value", value);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    InspectionRecipe recipe = reader.Read() ? Map(reader) : null;
                    reader.Close();
                    if (recipe != null) LoadRules(connection, recipe);
                    return recipe;
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
            command.Parameters.AddWithValue("@ModelId", DbConvert.DbNullIfMissing(recipe.ModelId));
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
                ModelId = DbConvert.GetNullableInt64(reader, "ModelId"),
                IsActive = DbConvert.GetBoolean(reader, "IsActive"),
                CreatedAtUtc = DbConvert.GetUtcDateTime(reader, "CreatedAtUtc"),
                UpdatedAtUtc = DbConvert.GetUtcDateTime(reader, "UpdatedAtUtc")
            };
        }

        private static void LoadRules(SQLiteConnection connection, InspectionRecipe recipe)
        {
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Id, RecipeId, CategoryId, CategoryCode, CategoryName, RoiName,
                    MinConfidence, MinArea, MinWidth, MinHeight, MaxAllowedCount, Decision, IsEnabled
                    FROM RecipeRules WHERE RecipeId=@RecipeId ORDER BY Id;";
                command.Parameters.AddWithValue("@RecipeId", recipe.Id);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        recipe.Rules.Add(new RecipeRule
                        {
                            Id=DbConvert.GetInt64(reader,"Id"), RecipeId=DbConvert.GetInt64(reader,"RecipeId"),
                            CategoryId=DbConvert.GetNullableInt64(reader,"CategoryId"), CategoryCode=DbConvert.GetString(reader,"CategoryCode"),
                            CategoryName=DbConvert.GetString(reader,"CategoryName"), RoiName=DbConvert.GetString(reader,"RoiName"),
                            MinConfidence=DbConvert.GetDouble(reader,"MinConfidence"), MinArea=DbConvert.GetDouble(reader,"MinArea"),
                            MinWidth=DbConvert.GetDouble(reader,"MinWidth"), MinHeight=DbConvert.GetDouble(reader,"MinHeight"),
                            MaxAllowedCount=DbConvert.GetInt32(reader,"MaxAllowedCount"), Decision=DbConvert.GetString(reader,"Decision"),
                            IsEnabled=DbConvert.GetBoolean(reader,"IsEnabled")
                        });
                    }
                }
            }
        }
    }
}
