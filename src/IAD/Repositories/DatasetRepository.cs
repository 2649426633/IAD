using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
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
                command.CommandText = @"SELECT Id, ProductId, FileName, RelativePath, Width, Height, Status,
                                               ReviewStatus, DatasetSplit, ContentHash, ReviewComment, ReviewedBy, ReviewedAtUtc,
                                               ProductDefinitionVersion, CreatedAtUtc, UpdatedAtUtc
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
                command.CommandText = @"SELECT Id, ProductId, FileName, RelativePath, Width, Height, Status,
                                               ReviewStatus, DatasetSplit, ContentHash, ReviewComment, ReviewedBy, ReviewedAtUtc,
                                               ProductDefinitionVersion, CreatedAtUtc, UpdatedAtUtc
                                        FROM DatasetImages WHERE Id = @Id LIMIT 1;";
                command.Parameters.AddWithValue("@Id", imageId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                    return reader.Read() ? MapImage(reader) : null;
            }
        }

        public DatasetImage GetImageByContentHash(long productId, string contentHash)
        {
            if (string.IsNullOrWhiteSpace(contentHash)) return null;
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Id, ProductId, FileName, RelativePath, Width, Height, Status,
                                               ReviewStatus, DatasetSplit, ContentHash, ReviewComment, ReviewedBy, ReviewedAtUtc,
                                               ProductDefinitionVersion, CreatedAtUtc, UpdatedAtUtc
                                        FROM DatasetImages
                                        WHERE ProductId = @ProductId AND ContentHash = @ContentHash LIMIT 1;";
                command.Parameters.AddWithValue("@ProductId", productId);
                command.Parameters.AddWithValue("@ContentHash", contentHash);
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
                        (ProductId, FileName, RelativePath, Width, Height, Status, ReviewStatus, DatasetSplit,
                         ContentHash, ReviewComment, ReviewedBy, ReviewedAtUtc,
                         ProductDefinitionVersion, CreatedAtUtc, UpdatedAtUtc)
                        VALUES (@ProductId, @FileName, @RelativePath, @Width, @Height, @Status, @ReviewStatus, @DatasetSplit,
                                @ContentHash, @ReviewComment, @ReviewedBy, @ReviewedAtUtc,
                                @ProductDefinitionVersion, @CreatedAtUtc, @UpdatedAtUtc);";
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
                command.CommandText = @"UPDATE DatasetImages SET
                    Status = @Status, ReviewStatus = 'Pending', ReviewComment = NULL, ReviewedBy = NULL, ReviewedAtUtc = NULL,
                    UpdatedAtUtc = @UpdatedAtUtc WHERE Id = @Id;";
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(updatedAtUtc));
                command.Parameters.AddWithValue("@Id", imageId);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要更新的数据集图片。Id=" + imageId);
            }
        }

        public void UpdateImageWorkflow(DatasetImage image)
        {
            if (image == null) throw new ArgumentNullException("image");
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE DatasetImages SET
                    Status = @Status, ReviewStatus = @ReviewStatus, DatasetSplit = @DatasetSplit,
                    ReviewComment = @ReviewComment, ReviewedBy = @ReviewedBy, ReviewedAtUtc = @ReviewedAtUtc,
                    UpdatedAtUtc = @UpdatedAtUtc WHERE Id = @Id AND ProductId = @ProductId;";
                command.Parameters.AddWithValue("@Status", image.Status);
                command.Parameters.AddWithValue("@ReviewStatus", image.ReviewStatus);
                command.Parameters.AddWithValue("@DatasetSplit", image.DatasetSplit);
                command.Parameters.AddWithValue("@ReviewComment", DbConvert.DbNullIfEmpty(image.ReviewComment));
                command.Parameters.AddWithValue("@ReviewedBy", DbConvert.DbNullIfEmpty(image.ReviewedBy));
                command.Parameters.AddWithValue("@ReviewedAtUtc", image.ReviewedAtUtc.HasValue
                    ? (object)DbConvert.ToUtcText(image.ReviewedAtUtc.Value)
                    : DBNull.Value);
                command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(image.UpdatedAtUtc));
                command.Parameters.AddWithValue("@Id", image.Id);
                command.Parameters.AddWithValue("@ProductId", image.ProductId);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要更新的数据集图片。Id=" + image.Id);
            }
        }

        public void UpdateImageContentHash(long imageId, string contentHash, DateTime updatedAtUtc)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE DatasetImages SET ContentHash = @ContentHash, UpdatedAtUtc = @UpdatedAtUtc WHERE Id = @Id;";
                command.Parameters.AddWithValue("@ContentHash", DbConvert.DbNullIfEmpty(contentHash));
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
                command.CommandText = @"SELECT Id, ProductId, VersionCode, ProductDefinitionVersion, ImageCount, AnnotationCount, MaskCount, Notes, CreatedAtUtc
                                        FROM DatasetVersions WHERE ProductId = @ProductId ORDER BY Id DESC LIMIT 1;";
                command.Parameters.AddWithValue("@ProductId", productId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                    return reader.Read() ? MapVersion(reader) : null;
            }
        }

        public IList<DatasetVersion> GetVersions(long productId)
        {
            List<DatasetVersion> items = new List<DatasetVersion>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Id, ProductId, VersionCode, ProductDefinitionVersion, ImageCount, AnnotationCount, MaskCount, Notes, CreatedAtUtc
                                        FROM DatasetVersions WHERE ProductId = @ProductId ORDER BY Id DESC;";
                command.Parameters.AddWithValue("@ProductId", productId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                    while (reader.Read()) items.Add(MapVersion(reader));
            }
            return items;
        }

        public IList<DatasetVersionImage> GetVersionImages(long versionId)
        {
            List<DatasetVersionImage> items = new List<DatasetVersionImage>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT VersionId, SourceImageId, FileName, RelativePath, Width, Height, Status,
                                               ReviewStatus, DatasetSplit, ContentHash, ReviewComment, ReviewedBy, ReviewedAtUtc,
                                               ProductDefinitionVersion
                                        FROM DatasetVersionImages WHERE VersionId = @VersionId ORDER BY SourceImageId;";
                command.Parameters.AddWithValue("@VersionId", versionId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new DatasetVersionImage
                        {
                            VersionId = DbConvert.GetInt64(reader, "VersionId"),
                            SourceImageId = DbConvert.GetInt64(reader, "SourceImageId"),
                            FileName = DbConvert.GetString(reader, "FileName"),
                            RelativePath = DbConvert.GetString(reader, "RelativePath"),
                            Width = DbConvert.GetInt32(reader, "Width"),
                            Height = DbConvert.GetInt32(reader, "Height"),
                            Status = DbConvert.GetString(reader, "Status"),
                            ReviewStatus = DbConvert.GetString(reader, "ReviewStatus"),
                            DatasetSplit = DbConvert.GetString(reader, "DatasetSplit"),
                            ContentHash = DbConvert.GetString(reader, "ContentHash"),
                            ReviewComment = DbConvert.GetString(reader, "ReviewComment"),
                            ReviewedBy = DbConvert.GetString(reader, "ReviewedBy"),
                            ReviewedAtUtc = ParseNullableUtc(DbConvert.GetString(reader, "ReviewedAtUtc")),
                            ProductDefinitionVersion = DbConvert.GetString(reader, "ProductDefinitionVersion")
                        });
                    }
                }
            }
            return items;
        }

        public IList<DatasetVersionAnnotation> GetVersionAnnotations(long versionId)
        {
            List<DatasetVersionAnnotation> items = new List<DatasetVersionAnnotation>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT VersionId, SourceAnnotationId, SourceImageId, CategoryCode, CategoryName,
                                               AnnotationType, GeometryData, BrushWidth, Confidence, IsVisible
                                        FROM DatasetVersionAnnotations WHERE VersionId = @VersionId ORDER BY SourceAnnotationId;";
                command.Parameters.AddWithValue("@VersionId", versionId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new DatasetVersionAnnotation
                        {
                            VersionId = DbConvert.GetInt64(reader, "VersionId"),
                            SourceAnnotationId = DbConvert.GetInt64(reader, "SourceAnnotationId"),
                            SourceImageId = DbConvert.GetInt64(reader, "SourceImageId"),
                            CategoryCode = DbConvert.GetString(reader, "CategoryCode"),
                            CategoryName = DbConvert.GetString(reader, "CategoryName"),
                            AnnotationType = DbConvert.GetString(reader, "AnnotationType"),
                            GeometryData = DbConvert.GetString(reader, "GeometryData"),
                            BrushWidth = (float)DbConvert.GetDouble(reader, "BrushWidth"),
                            Confidence = DbConvert.GetDouble(reader, "Confidence"),
                            IsVisible = DbConvert.GetBoolean(reader, "IsVisible")
                        });
                    }
                }
            }
            return items;
        }

        public IList<DatasetVersionMask> GetVersionMasks(long versionId)
        {
            List<DatasetVersionMask> items = new List<DatasetVersionMask>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT VersionId, SourceMaskId, SourceImageId, CategoryCode, CategoryName,
                                               RelativePath, Width, Height, Revision, PixelCount, IsVisible
                                        FROM DatasetVersionMasks WHERE VersionId = @VersionId ORDER BY SourceMaskId;";
                command.Parameters.AddWithValue("@VersionId", versionId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new DatasetVersionMask
                        {
                            VersionId = DbConvert.GetInt64(reader, "VersionId"),
                            SourceMaskId = DbConvert.GetInt64(reader, "SourceMaskId"),
                            SourceImageId = DbConvert.GetInt64(reader, "SourceImageId"),
                            CategoryCode = DbConvert.GetString(reader, "CategoryCode"),
                            CategoryName = DbConvert.GetString(reader, "CategoryName"),
                            RelativePath = DbConvert.GetString(reader, "RelativePath"),
                            Width = DbConvert.GetInt32(reader, "Width"),
                            Height = DbConvert.GetInt32(reader, "Height"),
                            Revision = DbConvert.GetInt32(reader, "Revision"),
                            PixelCount = DbConvert.GetInt64(reader, "PixelCount"),
                            IsVisible = DbConvert.GetBoolean(reader, "IsVisible")
                        });
                    }
                }
            }
            return items;
        }

        public void RestoreVersion(long productId, long versionId, DateTime restoredAtUtc)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                int versionExists;
                using (SQLiteCommand verify = new SQLiteCommand(
                    "SELECT COUNT(*) FROM DatasetVersions WHERE Id = @VersionId AND ProductId = @ProductId;", connection, transaction))
                {
                    verify.Parameters.AddWithValue("@VersionId", versionId);
                    verify.Parameters.AddWithValue("@ProductId", productId);
                    versionExists = Convert.ToInt32(verify.ExecuteScalar());
                }
                if (versionExists == 0) throw new InvalidOperationException("数据集版本不存在或不属于当前产品。Id=" + versionId);

                using (SQLiteCommand delete = new SQLiteCommand(
                    "DELETE FROM DatasetImages WHERE ProductId = @ProductId;", connection, transaction))
                {
                    delete.Parameters.AddWithValue("@ProductId", productId);
                    delete.ExecuteNonQuery();
                }

                using (SQLiteCommand restoreImages = new SQLiteCommand(@"INSERT INTO DatasetImages
                    (Id, ProductId, FileName, RelativePath, Width, Height, Status, ReviewStatus, DatasetSplit,
                     ContentHash, ReviewComment, ReviewedBy, ReviewedAtUtc, ProductDefinitionVersion, CreatedAtUtc, UpdatedAtUtc)
                    SELECT SourceImageId, @ProductId, FileName, RelativePath, Width, Height, Status,
                           ReviewStatus, DatasetSplit, ContentHash, ReviewComment, ReviewedBy, ReviewedAtUtc,
                           ProductDefinitionVersion, @RestoredAtUtc, @RestoredAtUtc
                    FROM DatasetVersionImages WHERE VersionId = @VersionId;", connection, transaction))
                {
                    restoreImages.Parameters.AddWithValue("@ProductId", productId);
                    restoreImages.Parameters.AddWithValue("@VersionId", versionId);
                    restoreImages.Parameters.AddWithValue("@RestoredAtUtc", DbConvert.ToUtcText(restoredAtUtc));
                    restoreImages.ExecuteNonQuery();
                }

                using (SQLiteCommand restoreAnnotations = new SQLiteCommand(@"INSERT INTO DatasetAnnotations
                    (Id, DatasetImageId, CategoryId, CategoryCode, CategoryName, AnnotationType, GeometryData,
                     BrushWidth, Confidence, IsVisible, CreatedAtUtc, UpdatedAtUtc)
                    SELECT va.SourceAnnotationId, va.SourceImageId,
                           (SELECT c.Id FROM DefectCategories c
                            WHERE c.ProductId = @ProductId AND
                                  ((va.CategoryCode IS NOT NULL AND c.CategoryCode = va.CategoryCode) OR
                                   (va.CategoryCode IS NULL AND c.CategoryName = va.CategoryName))
                            ORDER BY c.Id LIMIT 1),
                           va.CategoryCode, va.CategoryName, va.AnnotationType, va.GeometryData,
                           va.BrushWidth, va.Confidence, va.IsVisible, @RestoredAtUtc, @RestoredAtUtc
                    FROM DatasetVersionAnnotations va WHERE va.VersionId = @VersionId;", connection, transaction))
                {
                    restoreAnnotations.Parameters.AddWithValue("@ProductId", productId);
                    restoreAnnotations.Parameters.AddWithValue("@VersionId", versionId);
                    restoreAnnotations.Parameters.AddWithValue("@RestoredAtUtc", DbConvert.ToUtcText(restoredAtUtc));
                    restoreAnnotations.ExecuteNonQuery();
                }

                using (SQLiteCommand restoreMasks = new SQLiteCommand(@"INSERT INTO DatasetMasks
                    (Id, DatasetImageId, CategoryId, CategoryCode, CategoryName, RelativePath, Width, Height,
                     Revision, PixelCount, IsVisible, CreatedAtUtc, UpdatedAtUtc)
                    SELECT vm.SourceMaskId, vm.SourceImageId,
                           (SELECT c.Id FROM DefectCategories c
                            WHERE c.ProductId = @ProductId AND
                                  ((vm.CategoryCode IS NOT NULL AND c.CategoryCode = vm.CategoryCode) OR
                                   (vm.CategoryCode IS NULL AND c.CategoryName = vm.CategoryName))
                            ORDER BY c.Id LIMIT 1),
                           vm.CategoryCode, vm.CategoryName, vm.RelativePath, vm.Width, vm.Height,
                           vm.Revision, vm.PixelCount, vm.IsVisible, @RestoredAtUtc, @RestoredAtUtc
                    FROM DatasetVersionMasks vm WHERE vm.VersionId = @VersionId;", connection, transaction))
                {
                    restoreMasks.Parameters.AddWithValue("@ProductId", productId);
                    restoreMasks.Parameters.AddWithValue("@VersionId", versionId);
                    restoreMasks.Parameters.AddWithValue("@RestoredAtUtc", DbConvert.ToUtcText(restoredAtUtc));
                    restoreMasks.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        public long InsertVersion(DatasetVersion version)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                long versionId;
                using (SQLiteCommand command = new SQLiteCommand(@"INSERT INTO DatasetVersions
                    (ProductId, VersionCode, ProductDefinitionVersion, ImageCount, AnnotationCount, MaskCount, Notes, CreatedAtUtc)
                    VALUES (@ProductId, @VersionCode, @ProductDefinitionVersion, @ImageCount, @AnnotationCount, @MaskCount, @Notes, @CreatedAtUtc);", connection, transaction))
                {
                    command.Parameters.AddWithValue("@ProductId", version.ProductId);
                    command.Parameters.AddWithValue("@VersionCode", version.VersionCode);
                    command.Parameters.AddWithValue("@ProductDefinitionVersion", DbConvert.DbNullIfEmpty(version.ProductDefinitionVersion));
                    command.Parameters.AddWithValue("@ImageCount", version.ImageCount);
                    command.Parameters.AddWithValue("@AnnotationCount", version.AnnotationCount);
                    command.Parameters.AddWithValue("@MaskCount", version.MaskCount);
                    command.Parameters.AddWithValue("@Notes", DbConvert.DbNullIfEmpty(version.Notes));
                    command.Parameters.AddWithValue("@CreatedAtUtc", DbConvert.ToUtcText(version.CreatedAtUtc));
                    command.ExecuteNonQuery();
                }
                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection, transaction))
                    versionId = Convert.ToInt64(idCommand.ExecuteScalar());

                using (SQLiteCommand imageSnapshot = new SQLiteCommand(@"INSERT INTO DatasetVersionImages
                    (VersionId, SourceImageId, FileName, RelativePath, Width, Height, Status,
                     ReviewStatus, DatasetSplit, ContentHash, ReviewComment, ReviewedBy, ReviewedAtUtc,
                     ProductDefinitionVersion)
                    SELECT @VersionId, Id, FileName, RelativePath, Width, Height, Status,
                           ReviewStatus, DatasetSplit, ContentHash, ReviewComment, ReviewedBy, ReviewedAtUtc,
                           ProductDefinitionVersion
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

        public int CountMasks(long productId)
        {
            return Count(@"SELECT COUNT(*) FROM DatasetMasks m
                           INNER JOIN DatasetImages i ON i.Id = m.DatasetImageId
                           WHERE i.ProductId = @ProductId;", productId);
        }

        public IDictionary<long, int> GetClassCounts(long productId)
        {
            Dictionary<long, int> result = new Dictionary<long, int>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT ImageId, COUNT(*) AS ClassCount
                    FROM (
                        SELECT DISTINCT a.DatasetImageId AS ImageId,
                               CASE WHEN a.CategoryId IS NULL THEN 'N|' || COALESCE(a.CategoryName, '')
                                    ELSE 'I|' || CAST(a.CategoryId AS TEXT) END AS CategoryKey
                        FROM DatasetAnnotations a
                        INNER JOIN DatasetImages i ON i.Id = a.DatasetImageId
                        WHERE i.ProductId = @ProductId
                        UNION
                        SELECT DISTINCT m.DatasetImageId AS ImageId,
                               CASE WHEN m.CategoryId IS NULL THEN 'N|' || COALESCE(m.CategoryName, '')
                                    ELSE 'I|' || CAST(m.CategoryId AS TEXT) END AS CategoryKey
                        FROM DatasetMasks m
                        INNER JOIN DatasetImages i ON i.Id = m.DatasetImageId
                        WHERE i.ProductId = @ProductId
                    ) labels
                    GROUP BY ImageId;";
                command.Parameters.AddWithValue("@ProductId", productId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                    while (reader.Read()) result[DbConvert.GetInt64(reader, "ImageId")] = DbConvert.GetInt32(reader, "ClassCount");
            }
            return result;
        }

        public IList<string> GetAllReferencedImagePaths()
        {
            List<string> result = new List<string>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT RelativePath FROM DatasetImages
                    UNION SELECT RelativePath FROM DatasetVersionImages;";
                using (SQLiteDataReader reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        string value = DbConvert.GetString(reader, "RelativePath");
                        if (!string.IsNullOrWhiteSpace(value)) result.Add(value);
                    }
            }
            return result;
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
            command.Parameters.AddWithValue("@ReviewStatus", string.IsNullOrWhiteSpace(image.ReviewStatus) ? DatasetReviewStatus.Pending : image.ReviewStatus);
            command.Parameters.AddWithValue("@DatasetSplit", string.IsNullOrWhiteSpace(image.DatasetSplit) ? DatasetSplit.Unassigned : image.DatasetSplit);
            command.Parameters.AddWithValue("@ContentHash", DbConvert.DbNullIfEmpty(image.ContentHash));
            command.Parameters.AddWithValue("@ReviewComment", DbConvert.DbNullIfEmpty(image.ReviewComment));
            command.Parameters.AddWithValue("@ReviewedBy", DbConvert.DbNullIfEmpty(image.ReviewedBy));
            command.Parameters.AddWithValue("@ReviewedAtUtc", image.ReviewedAtUtc.HasValue
                ? (object)DbConvert.ToUtcText(image.ReviewedAtUtc.Value)
                : DBNull.Value);
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
                Status = DbConvert.GetString(reader, "Status"),
                ReviewStatus = DbConvert.GetString(reader, "ReviewStatus"),
                DatasetSplit = DbConvert.GetString(reader, "DatasetSplit"),
                ContentHash = DbConvert.GetString(reader, "ContentHash"),
                ReviewComment = DbConvert.GetString(reader, "ReviewComment"),
                ReviewedBy = DbConvert.GetString(reader, "ReviewedBy"),
                ReviewedAtUtc = ParseNullableUtc(DbConvert.GetString(reader, "ReviewedAtUtc")),
                ProductDefinitionVersion = DbConvert.GetString(reader, "ProductDefinitionVersion"),
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
                AnnotationCount = DbConvert.GetInt32(reader, "AnnotationCount"),
                MaskCount = DbConvert.GetInt32(reader, "MaskCount"), Notes = DbConvert.GetString(reader, "Notes"),
                CreatedAtUtc = DbConvert.GetUtcDateTime(reader, "CreatedAtUtc")
            };
        }

        private static DateTime? ParseNullableUtc(string value)
        {
            DateTime parsed;
            if (string.IsNullOrWhiteSpace(value) ||
                !DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed))
                return null;
            return parsed.Kind == DateTimeKind.Utc ? parsed : parsed.ToUniversalTime();
        }
    }
}
