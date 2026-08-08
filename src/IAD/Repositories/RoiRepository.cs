using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IAD.Infrastructure.Database;
using IAD.Models;

namespace IAD.Repositories
{
    internal sealed class RoiRepository : IRoiRepository
    {
        private readonly SqliteConnectionFactory connectionFactory;

        public RoiRepository(SqliteConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException("connectionFactory");
        }

        public IList<RoiDefinition> GetByProduct(long productId)
        {
            List<RoiDefinition> items = new List<RoiDefinition>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Id, ProductId, RoiName, RoiType, CenterX, CenterY, Width, Height, AngleDeg,
                                               GeometryJson, SortIndex, IsEnabled, CreatedAtUtc, UpdatedAtUtc
                                        FROM ProductRois WHERE ProductId = @ProductId
                                        ORDER BY SortIndex, RoiName;";
                command.Parameters.AddWithValue("@ProductId", productId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) items.Add(Map(reader));
                }
            }
            return items;
        }

        public RoiDefinition GetById(long id)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Id, ProductId, RoiName, RoiType, CenterX, CenterY, Width, Height, AngleDeg,
                                               GeometryJson, SortIndex, IsEnabled, CreatedAtUtc, UpdatedAtUtc
                                        FROM ProductRois WHERE Id = @Id LIMIT 1;";
                command.Parameters.AddWithValue("@Id", id);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public long Insert(RoiDefinition roi)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO ProductRois
                        (ProductId, RoiName, RoiType, CenterX, CenterY, Width, Height, AngleDeg, GeometryJson, SortIndex, IsEnabled, CreatedAtUtc, UpdatedAtUtc)
                        VALUES (@ProductId, @RoiName, @RoiType, @CenterX, @CenterY, @Width, @Height, @AngleDeg, @GeometryJson, @SortIndex, @IsEnabled, @CreatedAtUtc, @UpdatedAtUtc);";
                    AddParameters(command, roi);
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection))
                    return Convert.ToInt64(idCommand.ExecuteScalar());
            }
        }

        public void Update(RoiDefinition roi)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE ProductRois SET
                    RoiName = @RoiName,
                    RoiType = @RoiType,
                    CenterX = @CenterX,
                    CenterY = @CenterY,
                    Width = @Width,
                    Height = @Height,
                    AngleDeg = @AngleDeg,
                    GeometryJson = @GeometryJson,
                    SortIndex = @SortIndex,
                    IsEnabled = @IsEnabled,
                    UpdatedAtUtc = @UpdatedAtUtc
                    WHERE Id = @Id AND ProductId = @ProductId;";
                AddParameters(command, roi);
                command.Parameters.AddWithValue("@Id", roi.Id);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要更新的ROI。Id=" + roi.Id);
            }
        }

        public void Delete(long id, long productId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM ProductRois WHERE Id = @Id AND ProductId = @ProductId;";
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@ProductId", productId);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要删除的ROI。Id=" + id);
            }
        }

        public void DeleteByProduct(long productId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM ProductRois WHERE ProductId = @ProductId;";
                command.Parameters.AddWithValue("@ProductId", productId);
                command.ExecuteNonQuery();
            }
        }

        private static void AddParameters(SQLiteCommand command, RoiDefinition roi)
        {
            command.Parameters.AddWithValue("@ProductId", roi.ProductId);
            command.Parameters.AddWithValue("@RoiName", roi.RoiName);
            command.Parameters.AddWithValue("@RoiType", roi.RoiType);
            command.Parameters.AddWithValue("@CenterX", roi.CenterX);
            command.Parameters.AddWithValue("@CenterY", roi.CenterY);
            command.Parameters.AddWithValue("@Width", roi.Width);
            command.Parameters.AddWithValue("@Height", roi.Height);
            command.Parameters.AddWithValue("@AngleDeg", roi.AngleDeg);
            command.Parameters.AddWithValue("@GeometryJson", DbConvert.DbNullIfEmpty(roi.GeometryJson));
            command.Parameters.AddWithValue("@SortIndex", roi.SortIndex);
            command.Parameters.AddWithValue("@IsEnabled", roi.IsEnabled ? 1 : 0);
            command.Parameters.AddWithValue("@CreatedAtUtc", DbConvert.ToUtcText(roi.CreatedAtUtc));
            command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(roi.UpdatedAtUtc));
        }

        private static RoiDefinition Map(SQLiteDataReader reader)
        {
            return new RoiDefinition
            {
                Id = DbConvert.GetInt64(reader, "Id"),
                ProductId = DbConvert.GetInt64(reader, "ProductId"),
                RoiName = DbConvert.GetString(reader, "RoiName"),
                RoiType = DbConvert.GetString(reader, "RoiType"),
                CenterX = DbConvert.GetDouble(reader, "CenterX"),
                CenterY = DbConvert.GetDouble(reader, "CenterY"),
                Width = DbConvert.GetDouble(reader, "Width"),
                Height = DbConvert.GetDouble(reader, "Height"),
                AngleDeg = DbConvert.GetDouble(reader, "AngleDeg"),
                GeometryJson = DbConvert.GetString(reader, "GeometryJson"),
                SortIndex = DbConvert.GetInt32(reader, "SortIndex"),
                IsEnabled = DbConvert.GetBoolean(reader, "IsEnabled"),
                CreatedAtUtc = DbConvert.GetUtcDateTime(reader, "CreatedAtUtc"),
                UpdatedAtUtc = DbConvert.GetUtcDateTime(reader, "UpdatedAtUtc")
            };
        }
    }
}
