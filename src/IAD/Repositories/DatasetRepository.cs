using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IAD.Infrastructure.Database;
using IAD.Models;

namespace IAD.Repositories
{
    internal sealed class DatasetRepository : IDatasetRepository
    {
        private readonly SqliteConnectionFactory connectionFactory;

        public DatasetRepository(SqliteConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException("connectionFactory");
        }

        public IList<DatasetImage> GetImagesByProduct(long productId)
        {
            List<DatasetImage> items = new List<DatasetImage>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Id, ProductId, FileName, RelativePath, Width, Height, Status, ProductDefinitionVersion, CreatedAtUtc, UpdatedAtUtc
                                        FROM DatasetImages WHERE ProductId = @ProductId ORDER BY Id;";
                command.Parameters.AddWithValue("@ProductId", productId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) items.Add(MapImage(reader));
                }
            }
            return items;
        }

        public DatasetImage GetImageById(long imageId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Id, ProductId, FileName, RelativePath, Width, Height, Status, ProductDefinitionVersion, CreatedAtUtc, UpdatedAtUtc
                                        FROM DatasetImages WHERE Id = @Id LIMIT 1;";
                command.Parameters.AddWithValue("@Id", imageId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                    return reader.Read() ? MapImage(reader) : null;
            }
        }

        public long InsertImage(DatasetImage image)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO DatasetImages
                        (ProductId, FileName, RelativePath, Width, Height, Status, ProductDefinitionVersion, CreatedAtUtc, UpdatedAtUtc)
                        VALUES (@ProductId, @FileName, @RelativePath, @Width, @Height, @Status, @ProductDefinitionVersion, @CreatedAtUtc, @UpdatedAtUtc);";
                    AddImageParameters(command, image);
                    command.ExecuteNonQuery();
                }
                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection))
                    return Convert.ToInt64(idCommand.ExecuteScalar());
            }
        }

        public void UpdateImageStatus(long imageId, string status, DateTime updatedAtUtc)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE DatasetImages SET Status = @Status, UpdatedAtUtc = @UpdatedAtUtc WHERE Id = @Id;";
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(updatedAtUtc));
                command.Parameters.AddWithValue("@Id", imageId);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要更新的数据集图片。Id=" + imageId);
            }
        }

        public bool IsImageReferencedByVersion(long imageId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT EXISTS(SELECT 1 FROM DatasetVersionImages WHERE SourceImageId = @ImageId LIMIT 1);";
                command.Parameters.AddWithValue("@ImageId", imageId);
                return Convert.ToInt32(command.ExecuteScalar()) != 0;
            }
        }

        public void DeleteImage(long imageId, long productId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                using (SQLiteCommand annotations = new SQLiteCommand(
                    "DELETE FROM DatasetAnnotations WHERE DatasetImageId = @ImageId;", connection, transaction))
                {
                    annotations.Parameters.AddWithValue("@ImageId", imageId);
                    annotations.ExecuteNonQuery();
                }

                using (SQLiteCommand image = new SQLiteCommand(
                    "DELETE FROM DatasetImages WHERE Id = @ImageId AND ProductId = @ProductId;", connection, transaction))
                {
                    image.Parameters.AddWithValue("@ImageId", imageId);
                    image.Parameters.AddWithValue("@ProductId", productId);
                    if (image.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("未找到需要删除的数据集图片。Id=" + imageId);
                }

                transaction.Commit();
            }
        }

        public IList<DatasetAnnotation> GetAnnotationsByImage(long imageId)
        {
            List<DatasetAnnotation> items = new List<DatasetAnnotation>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT a.Id, a.DatasetImageId, a.CategoryId,
                                               COALESCE(c.CategoryCode, a.CategoryCode) AS CategoryCode,
                                               COALESCE(c.CategoryName, a.CategoryName) AS CategoryName,
                                               a.AnnotationType, a.GeometryData, a.BrushWidth, a.Confidence,
                                               a.IsVisible, a.CreatedAtUtc, a.UpdatedAtUtc
                                        FROM DatasetAnnotations a
                                        LEFT JOIN DefectCategories c ON c.Id = a.CategoryId
                                        WHERE a.DatasetImageId = @ImageId ORDER BY a.Id;";
                command.Parameters.AddWithValue("@ImageId", imageId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) items.Add(MapAnnotation(reader));
                }
            }
            return items;
        }

        public long InsertAnnotation(DatasetAnnotation annotation)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO DatasetAnnotations
                        (DatasetImageId, CategoryId, CategoryCode, CategoryName, AnnotationType, GeometryData,
                         BrushWidth, Confidence, IsVisible, CreatedAtUtc, UpdatedAtUtc)
                        VALUES (@DatasetImageId, @CategoryId, @CategoryCode, @CategoryName, @AnnotationType, @GeometryData,
                                @BrushWidth, @Confidence, @IsVisible, @CreatedAtUtc, @UpdatedAtUtc);";
                    AddAnnotationParameters(command, annotation);
                    command.ExecuteNonQuery();
                }
                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection))
                    return Convert.ToInt64(idCommand.ExecuteScalar());
            }
        }

        public void UpdateAnnotation(DatasetAnnotation annotation)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE DatasetAnnotations SET
                    CategoryId = @CategoryId, CategoryCode = @CategoryCode, CategoryName = @CategoryName,
                    AnnotationType = @AnnotationType, GeometryData = @GeometryData, BrushWidth = @BrushWidth,
                    Confidence = @Confidence, IsVisible = @IsVisible, UpdatedAtUtc = @UpdatedAtUtc
                    WHERE Id = @Id AND DatasetImageId = @DatasetImageId;";
                AddAnnotationParameters(command, annotation);
                command.Parameters.AddWithValue("@Id", annotation.Id);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要更新的标注。Id=" + annotation.Id);
            }
        }

        public void DeleteAnnotation(long annotationId, long imageId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM DatasetAnnotations WHERE Id = @Id AND DatasetImageId = @ImageId;";
                command.Parameters.AddWithValue("@Id", annotationId);
                command.Parameters.AddWithValue("@ImageId", imageId);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要删除的标注。Id=" + annotationId);
            }
        }

        public DatasetVersion GetLatestVersion(long productId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Id, ProductId, VersionCode, ProductDefinitionVersion, ImageCount, AnnotationCount, Notes, CreatedAtUtc
                                        FROM DatasetVersions WHERE ProductId = @ProductId ORDER BY Id DESC LIMIT 1;";
                command.Parameters.AddWithValue("@ProductId", productId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                    return reader.Read() ? MapVersion(reader) : null;
            }
        }

        public long InsertVersion(DatasetVersion version)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                long versionId;
                using (SQLiteCommand command = new SQLiteCommand(@"INSERT INTO DatasetVersions
                    (ProductId, VersionCode, ProductDefinitionVersion, ImageCount, AnnotationCount, Notes, CreatedAtUtc)
                    VALUES (@ProductId, @VersionCode, @ProductDefinitionVersion, @ImageCount, @AnnotationCount, @Notes, @CreatedAtUtc);", connection, transaction))
                {
                    command.Parameters.AddWithValue("@ProductId", version.ProductId);
                    command.Parameters.AddWithValue("@VersionCode", version.VersionCode);
                    command.Parameters.AddWithValue("@ProductDefinitionVersion", DbConvert.DbNullIfEmpty(version.ProductDefinitionVersion));
                    command.Parameters.AddWithValue("@ImageCount", version.ImageCount);
                    command.Parameters.AddWithValue("@AnnotationCount", version.AnnotationCount);
                    command.Parameters.AddWithValue("@Notes", DbConvert.DbNullIfEmpty(version.Notes));
                    command.Parameters.AddWithValue("@CreatedAtUtc", DbConvert.ToUtcText(version.CreatedAtUtc));
                    command.ExecuteNonQuery();
                }
                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection, transaction))
                    versionId = Convert.ToInt64(idCommand.ExecuteScalar());

                using (SQLiteCommand imageSnapshot = new SQLiteCommand(@"INSERT INTO DatasetVersionImages
                    (VersionId, SourceImageId, FileName, RelativePath, Width, Height, Status, ProductDefinitionVersion)
                    SELECT @VersionId, Id, FileName, RelativePath, Width, Height, Status, ProductDefinitionVersion
                    FROM DatasetImages WHERE ProductId = @ProductId;", connection, transaction))
                {
                    imageSnapshot.Parameters.AddWithValue("@VersionId", versionId);
                    imageSnapshot.Parameters.AddWithValue("@ProductId", version.ProductId);
                    imageSnapshot.ExecuteNonQuery();
                }

                using (SQLiteCommand annotationSnapshot = new SQLiteCommand(@"INSERT INTO DatasetVersionAnnotations
                    (VersionId, SourceAnnotationId, SourceImageId, CategoryCode, CategoryName, AnnotationType,
                     GeometryData, BrushWidth, Confidence, IsVisible)
                    SELECT @VersionId, a.Id, a.DatasetImageId,
                           COALESCE(c.CategoryCode, a.CategoryCode), COALESCE(c.CategoryName, a.CategoryName), a.AnnotationType,
                           a.GeometryData, a.BrushWidth, a.Confidence, a.IsVisible
                    FROM DatasetAnnotations a
                    INNER JOIN DatasetImages i ON i.Id = a.DatasetImageId
                    LEFT JOIN DefectCategories c ON c.Id = a.CategoryId
                    WHERE i.ProductId = @ProductId;", connection, transaction))
                {
                    annotationSnapshot.Parameters.AddWithValue("@VersionId", versionId);
                    annotationSnapshot.Parameters.AddWithValue("@ProductId", version.ProductId);
                    annotationSnapshot.ExecuteNonQuery();
                }

                transaction.Commit();
                return versionId;
            }
        }

        public int CountImages(long productId)
        {
            return Count("SELECT COUNT(*) FROM DatasetImages WHERE ProductId = @ProductId;", productId);
        }

        public int CountAnnotations(long productId)
        {
            return Count(@"SELECT COUNT(*) FROM DatasetAnnotations a
                           INNER JOIN DatasetImages i ON i.Id = a.DatasetImageId
                           WHERE i.ProductId = @ProductId;", productId);
        }

        private int Count(string sql, long productId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@ProductId", productId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static void AddImageParameters(SQLiteCommand command, DatasetImage image)
        {
            command.Parameters.AddWithValue("@ProductId", image.ProductId);
            command.Parameters.AddWithValue("@FileName", image.FileName);
            command.Parameters.AddWithValue("@RelativePath", image.RelativePath);
            command.Parameters.AddWithValue("@Width", image.Width);
            command.Parameters.AddWithValue("@Height", image.Height);
            command.Parameters.AddWithValue("@Status", image.Status);
            command.Parameters.AddWithValue("@ProductDefinitionVersion", DbConvert.DbNullIfEmpty(image.ProductDefinitionVersion));
            command.Parameters.AddWithValue("@CreatedAtUtc", DbConvert.ToUtcText(image.CreatedAtUtc));
            command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(image.UpdatedAtUtc));
        }

        private static void AddAnnotationParameters(SQLiteCommand command, DatasetAnnotation annotation)
        {
            command.Parameters.AddWithValue("@DatasetImageId", annotation.DatasetImageId);
            command.Parameters.AddWithValue("@CategoryId", DbConvert.DbNullIfMissing(annotation.CategoryId));
            command.Parameters.AddWithValue("@CategoryCode", DbConvert.DbNullIfEmpty(annotation.CategoryCode));
            command.Parameters.AddWithValue("@CategoryName", annotation.CategoryName);
            command.Parameters.AddWithValue("@AnnotationType", annotation.AnnotationType);
            command.Parameters.AddWithValue("@GeometryData", annotation.GeometryData);
            command.Parameters.AddWithValue("@BrushWidth", annotation.BrushWidth);
            command.Parameters.AddWithValue("@Confidence", annotation.Confidence);
            command.Parameters.AddWithValue("@IsVisible", annotation.IsVisible ? 1 : 0);
            command.Parameters.AddWithValue("@CreatedAtUtc", DbConvert.ToUtcText(annotation.CreatedAtUtc));
            command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(annotation.UpdatedAtUtc));
        }

        private static DatasetImage MapImage(SQLiteDataReader reader)
        {
            return new DatasetImage
            {
                Id = DbConvert.GetInt64(reader, "Id"), ProductId = DbConvert.GetInt64(reader, "ProductId"),
                FileName = DbConvert.GetString(reader, "FileName"), RelativePath = DbConvert.GetString(reader, "RelativePath"),
                Width = DbConvert.GetInt32(reader, "Width"), Height = DbConvert.GetInt32(reader, "Height"),
                Status = DbConvert.GetString(reader, "Status"), ProductDefinitionVersion = DbConvert.GetString(reader, "ProductDefinitionVersion"),
                CreatedAtUtc = DbConvert.GetUtcDateTime(reader, "CreatedAtUtc"),
                UpdatedAtUtc = DbConvert.GetUtcDateTime(reader, "UpdatedAtUtc")
            };
        }

        private static DatasetAnnotation MapAnnotation(SQLiteDataReader reader)
        {
            return new DatasetAnnotation
            {
                Id = DbConvert.GetInt64(reader, "Id"), DatasetImageId = DbConvert.GetInt64(reader, "DatasetImageId"),
                CategoryId = DbConvert.GetNullableInt64(reader, "CategoryId"), CategoryCode = DbConvert.GetString(reader, "CategoryCode"),
                CategoryName = DbConvert.GetString(reader, "CategoryName"), AnnotationType = DbConvert.GetString(reader, "AnnotationType"),
                GeometryData = DbConvert.GetString(reader, "GeometryData"), BrushWidth = (float)DbConvert.GetDouble(reader, "BrushWidth"),
                Confidence = DbConvert.GetDouble(reader, "Confidence"), IsVisible = DbConvert.GetBoolean(reader, "IsVisible"),
                CreatedAtUtc = DbConvert.GetUtcDateTime(reader, "CreatedAtUtc"), UpdatedAtUtc = DbConvert.GetUtcDateTime(reader, "UpdatedAtUtc")
            };
        }

        private static DatasetVersion MapVersion(SQLiteDataReader reader)
        {
            return new DatasetVersion
            {
                Id = DbConvert.GetInt64(reader, "Id"), ProductId = DbConvert.GetInt64(reader, "ProductId"),
                VersionCode = DbConvert.GetString(reader, "VersionCode"), ProductDefinitionVersion = DbConvert.GetString(reader, "ProductDefinitionVersion"),
                ImageCount = DbConvert.GetInt32(reader, "ImageCount"),
                AnnotationCount = DbConvert.GetInt32(reader, "AnnotationCount"), Notes = DbConvert.GetString(reader, "Notes"),
                CreatedAtUtc = DbConvert.GetUtcDateTime(reader, "CreatedAtUtc")
            };
        }
    }
}
