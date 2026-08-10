using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IAD.Infrastructure.Database;
using IAD.Models;

namespace IAD.Repositories
{
    internal sealed class DatasetMaskRepository : IDatasetMaskRepository
    {
        private readonly SqliteConnectionFactory connectionFactory;

        public DatasetMaskRepository(SqliteConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException("connectionFactory");
        }

        public IList<DatasetMask> GetByImage(long imageId)
        {
            List<DatasetMask> items = new List<DatasetMask>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = SelectSql + " WHERE m.DatasetImageId = @ImageId ORDER BY m.Id;";
                command.Parameters.AddWithValue("@ImageId", imageId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) items.Add(Map(reader));
                }
            }
            return items;
        }

        public DatasetMask GetByImageAndCategory(long imageId, long categoryId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = SelectSql + " WHERE m.DatasetImageId = @ImageId AND m.CategoryId = @CategoryId LIMIT 1;";
                command.Parameters.AddWithValue("@ImageId", imageId);
                command.Parameters.AddWithValue("@CategoryId", categoryId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                    return reader.Read() ? Map(reader) : null;
            }
        }

        public DatasetMask GetById(long maskId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = SelectSql + " WHERE m.Id = @Id LIMIT 1;";
                command.Parameters.AddWithValue("@Id", maskId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                    return reader.Read() ? Map(reader) : null;
            }
        }

        public long Insert(DatasetMask mask)
        {
            if (mask == null) throw new ArgumentNullException("mask");
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO DatasetMasks
                        (DatasetImageId, CategoryId, CategoryCode, CategoryName, RelativePath,
                         Width, Height, Revision, PixelCount, IsVisible, CreatedAtUtc, UpdatedAtUtc)
                        VALUES (@DatasetImageId, @CategoryId, @CategoryCode, @CategoryName, @RelativePath,
                                @Width, @Height, @Revision, @PixelCount, @IsVisible, @CreatedAtUtc, @UpdatedAtUtc);";
                    AddParameters(command, mask);
                    command.ExecuteNonQuery();
                }
                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection))
                    return Convert.ToInt64(idCommand.ExecuteScalar());
            }
        }

        public void Update(DatasetMask mask)
        {
            if (mask == null) throw new ArgumentNullException("mask");
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE DatasetMasks SET
                    CategoryId = @CategoryId,
                    CategoryCode = @CategoryCode,
                    CategoryName = @CategoryName,
                    RelativePath = @RelativePath,
                    Width = @Width,
                    Height = @Height,
                    Revision = @Revision,
                    PixelCount = @PixelCount,
                    IsVisible = @IsVisible,
                    UpdatedAtUtc = @UpdatedAtUtc
                    WHERE Id = @Id AND DatasetImageId = @DatasetImageId;";
                AddParameters(command, mask);
                command.Parameters.AddWithValue("@Id", mask.Id);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要更新的 Mask。Id=" + mask.Id);
            }
        }

        public void Delete(long maskId, long imageId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM DatasetMasks WHERE Id = @Id AND DatasetImageId = @ImageId;";
                command.Parameters.AddWithValue("@Id", maskId);
                command.Parameters.AddWithValue("@ImageId", imageId);
                command.ExecuteNonQuery();
            }
        }

        public bool IsRelativePathReferencedByVersion(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return false;
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT EXISTS(SELECT 1 FROM DatasetVersionMasks WHERE RelativePath = @RelativePath LIMIT 1);";
                command.Parameters.AddWithValue("@RelativePath", relativePath);
                return Convert.ToInt32(command.ExecuteScalar()) != 0;
            }
        }

        public IList<string> GetAllReferencedRelativePaths()
        {
            List<string> paths = new List<string>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT RelativePath FROM DatasetMasks
                                        UNION
                                        SELECT RelativePath FROM DatasetVersionMasks;";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string value = DbConvert.GetString(reader, "RelativePath");
                        if (!string.IsNullOrWhiteSpace(value)) paths.Add(value);
                    }
                }
            }
            return paths;
        }

        private static void AddParameters(SQLiteCommand command, DatasetMask mask)
        {
            command.Parameters.AddWithValue("@DatasetImageId", mask.DatasetImageId);
            command.Parameters.AddWithValue("@CategoryId", DbConvert.DbNullIfMissing(mask.CategoryId));
            command.Parameters.AddWithValue("@CategoryCode", DbConvert.DbNullIfEmpty(mask.CategoryCode));
            command.Parameters.AddWithValue("@CategoryName", string.IsNullOrWhiteSpace(mask.CategoryName) ? "未命名类别" : mask.CategoryName);
            command.Parameters.AddWithValue("@RelativePath", mask.RelativePath);
            command.Parameters.AddWithValue("@Width", mask.Width);
            command.Parameters.AddWithValue("@Height", mask.Height);
            command.Parameters.AddWithValue("@Revision", mask.Revision);
            command.Parameters.AddWithValue("@PixelCount", mask.PixelCount);
            command.Parameters.AddWithValue("@IsVisible", mask.IsVisible ? 1 : 0);
            command.Parameters.AddWithValue("@CreatedAtUtc", DbConvert.ToUtcText(mask.CreatedAtUtc));
            command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(mask.UpdatedAtUtc));
        }

        private static DatasetMask Map(SQLiteDataReader reader)
        {
            return new DatasetMask
            {
                Id = DbConvert.GetInt64(reader, "Id"),
                DatasetImageId = DbConvert.GetInt64(reader, "DatasetImageId"),
                CategoryId = DbConvert.GetNullableInt64(reader, "CategoryId"),
                CategoryCode = DbConvert.GetString(reader, "CategoryCode"),
                CategoryName = DbConvert.GetString(reader, "CategoryName"),
                RelativePath = DbConvert.GetString(reader, "RelativePath"),
                Width = DbConvert.GetInt32(reader, "Width"),
                Height = DbConvert.GetInt32(reader, "Height"),
                Revision = DbConvert.GetInt32(reader, "Revision"),
                PixelCount = DbConvert.GetInt64(reader, "PixelCount"),
                IsVisible = DbConvert.GetBoolean(reader, "IsVisible"),
                CreatedAtUtc = DbConvert.GetUtcDateTime(reader, "CreatedAtUtc"),
                UpdatedAtUtc = DbConvert.GetUtcDateTime(reader, "UpdatedAtUtc")
            };
        }

        private const string SelectSql = @"SELECT m.Id, m.DatasetImageId, m.CategoryId,
            COALESCE(c.CategoryCode, m.CategoryCode) AS CategoryCode,
            COALESCE(c.CategoryName, m.CategoryName) AS CategoryName,
            m.RelativePath, m.Width, m.Height, m.Revision, m.PixelCount, m.IsVisible,
            m.CreatedAtUtc, m.UpdatedAtUtc
            FROM DatasetMasks m
            LEFT JOIN DefectCategories c ON c.Id = m.CategoryId";
    }
}
