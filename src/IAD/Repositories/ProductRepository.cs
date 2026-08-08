using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IAD.Infrastructure.Database;
using IAD.Models;

namespace IAD.Repositories
{
    internal sealed class ProductRepository : IProductRepository
    {
        private readonly SqliteConnectionFactory connectionFactory;

        public ProductRepository(SqliteConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException("connectionFactory");
        }

        public IList<Product> GetAll()
        {
            List<Product> items = new List<Product>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Id, ProductCode, ProductName, Description, IsActive, CreatedAtUtc, UpdatedAtUtc
                                        FROM Products ORDER BY ProductCode;";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) items.Add(Map(reader));
                }
            }
            return items;
        }

        public Product GetById(long id)
        {
            return GetSingle("SELECT Id, ProductCode, ProductName, Description, IsActive, CreatedAtUtc, UpdatedAtUtc FROM Products WHERE Id = @Value;", id);
        }

        public Product GetByCode(string productCode)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, ProductCode, ProductName, Description, IsActive, CreatedAtUtc, UpdatedAtUtc FROM Products WHERE ProductCode = @Code LIMIT 1;";
                command.Parameters.AddWithValue("@Code", productCode);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public long Insert(Product product)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO Products
                        (ProductCode, ProductName, Description, IsActive, CreatedAtUtc, UpdatedAtUtc)
                        VALUES (@ProductCode, @ProductName, @Description, @IsActive, @CreatedAtUtc, @UpdatedAtUtc);";
                    AddParameters(command, product);
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection))
                {
                    return Convert.ToInt64(idCommand.ExecuteScalar());
                }
            }
        }

        public void Update(Product product)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE Products SET
                    ProductCode = @ProductCode,
                    ProductName = @ProductName,
                    Description = @Description,
                    IsActive = @IsActive,
                    UpdatedAtUtc = @UpdatedAtUtc
                    WHERE Id = @Id;";
                AddParameters(command, product);
                command.Parameters.AddWithValue("@Id", product.Id);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要更新的产品。Id=" + product.Id);
            }
        }

        private Product GetSingle(string sql, long value)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@Value", value);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        private static void AddParameters(SQLiteCommand command, Product product)
        {
            command.Parameters.AddWithValue("@ProductCode", product.ProductCode);
            command.Parameters.AddWithValue("@ProductName", product.ProductName);
            command.Parameters.AddWithValue("@Description", DbConvert.DbNullIfEmpty(product.Description));
            command.Parameters.AddWithValue("@IsActive", product.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("@CreatedAtUtc", DbConvert.ToUtcText(product.CreatedAtUtc));
            command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(product.UpdatedAtUtc));
        }

        private static Product Map(SQLiteDataReader reader)
        {
            return new Product
            {
                Id = DbConvert.GetInt64(reader, "Id"),
                ProductCode = DbConvert.GetString(reader, "ProductCode"),
                ProductName = DbConvert.GetString(reader, "ProductName"),
                Description = DbConvert.GetString(reader, "Description"),
                IsActive = DbConvert.GetBoolean(reader, "IsActive"),
                CreatedAtUtc = DbConvert.GetUtcDateTime(reader, "CreatedAtUtc"),
                UpdatedAtUtc = DbConvert.GetUtcDateTime(reader, "UpdatedAtUtc")
            };
        }
    }
}
