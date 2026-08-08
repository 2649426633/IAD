using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IAD.Infrastructure.Database;
using IAD.Models;

namespace IAD.Repositories
{
    internal sealed class DefectCategoryRepository : IDefectCategoryRepository
    {
        private readonly SqliteConnectionFactory connectionFactory;
        private const string SelectColumns = "Id, ProductId, CategoryCode, CategoryName, DefectType, DetectionStrategy, DefaultThreshold, MinArea, MinLength, DisplayOrder, IsEnabled, CreatedAtUtc, UpdatedAtUtc";

        public DefectCategoryRepository(SqliteConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException("connectionFactory");
        }

        public IList<DefectCategory> GetByProduct(long productId)
        {
            List<DefectCategory> items = new List<DefectCategory>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + SelectColumns + " FROM DefectCategories WHERE ProductId = @ProductId ORDER BY DisplayOrder, CategoryCode;";
                command.Parameters.AddWithValue("@ProductId", productId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) items.Add(Map(reader));
                }
            }
            return items;
        }

        public DefectCategory GetById(long id)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + SelectColumns + " FROM DefectCategories WHERE Id = @Id LIMIT 1;";
                command.Parameters.AddWithValue("@Id", id);
                using (SQLiteDataReader reader = command.ExecuteReader())
                    return reader.Read() ? Map(reader) : null;
            }
        }

        public long Insert(DefectCategory category)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO DefectCategories
                        (ProductId, CategoryCode, CategoryName, DefectType, DetectionStrategy, DefaultThreshold, MinArea, MinLength, DisplayOrder, IsEnabled, CreatedAtUtc, UpdatedAtUtc)
                        VALUES (@ProductId, @CategoryCode, @CategoryName, @DefectType, @DetectionStrategy, @DefaultThreshold, @MinArea, @MinLength, @DisplayOrder, @IsEnabled, @CreatedAtUtc, @UpdatedAtUtc);";
                    AddParameters(command, category);
                    command.ExecuteNonQuery();
                }
                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection))
                    return Convert.ToInt64(idCommand.ExecuteScalar());
            }
        }

        public void Update(DefectCategory category)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE DefectCategories SET
                    CategoryCode = @CategoryCode,
                    CategoryName = @CategoryName,
                    DefectType = @DefectType,
                    DetectionStrategy = @DetectionStrategy,
                    DefaultThreshold = @DefaultThreshold,
                    MinArea = @MinArea,
                    MinLength = @MinLength,
                    DisplayOrder = @DisplayOrder,
                    IsEnabled = @IsEnabled,
                    UpdatedAtUtc = @UpdatedAtUtc
                    WHERE Id = @Id AND ProductId = @ProductId;";
                AddParameters(command, category);
                command.Parameters.AddWithValue("@Id", category.Id);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要更新的缺陷类别。Id=" + category.Id);
            }
        }

        public void Delete(long id, long productId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM DefectCategories WHERE Id = @Id AND ProductId = @ProductId;";
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@ProductId", productId);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要删除的缺陷类别。Id=" + id);
            }
        }

        private static void AddParameters(SQLiteCommand command, DefectCategory category)
        {
            command.Parameters.AddWithValue("@ProductId", category.ProductId);
            command.Parameters.AddWithValue("@CategoryCode", category.CategoryCode);
            command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
            command.Parameters.AddWithValue("@DefectType", DbConvert.DbNullIfEmpty(category.DefectType));
            command.Parameters.AddWithValue("@DetectionStrategy", DbConvert.DbNullIfEmpty(category.DetectionStrategy));
            command.Parameters.AddWithValue("@DefaultThreshold", category.DefaultThreshold);
            command.Parameters.AddWithValue("@MinArea", category.MinArea);
            command.Parameters.AddWithValue("@MinLength", category.MinLength);
            command.Parameters.AddWithValue("@DisplayOrder", category.DisplayOrder);
            command.Parameters.AddWithValue("@IsEnabled", category.IsEnabled ? 1 : 0);
            command.Parameters.AddWithValue("@CreatedAtUtc", DbConvert.ToUtcText(category.CreatedAtUtc));
            command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(category.UpdatedAtUtc));
        }

        private static DefectCategory Map(SQLiteDataReader reader)
        {
            return new DefectCategory
            {
                Id = DbConvert.GetInt64(reader, "Id"),
                ProductId = DbConvert.GetInt64(reader, "ProductId"),
                CategoryCode = DbConvert.GetString(reader, "CategoryCode"),
                CategoryName = DbConvert.GetString(reader, "CategoryName"),
                DefectType = DbConvert.GetString(reader, "DefectType"),
                DetectionStrategy = DbConvert.GetString(reader, "DetectionStrategy"),
                DefaultThreshold = DbConvert.GetDouble(reader, "DefaultThreshold"),
                MinArea = DbConvert.GetDouble(reader, "MinArea"),
                MinLength = DbConvert.GetDouble(reader, "MinLength"),
                DisplayOrder = DbConvert.GetInt32(reader, "DisplayOrder"),
                IsEnabled = DbConvert.GetBoolean(reader, "IsEnabled"),
                CreatedAtUtc = DbConvert.GetUtcDateTime(reader, "CreatedAtUtc"),
                UpdatedAtUtc = DbConvert.GetUtcDateTime(reader, "UpdatedAtUtc")
            };
        }
    }
}
