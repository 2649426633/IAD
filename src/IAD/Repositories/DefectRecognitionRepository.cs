using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IAD.Infrastructure.Database;
using IAD.Models;

namespace IAD.Repositories
{
    internal sealed class DefectRecognitionRepository : IDefectRecognitionRepository
    {
        private readonly SqliteConnectionFactory connectionFactory;

        public DefectRecognitionRepository(SqliteConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException("connectionFactory");
        }

        public DefectRecognitionSettings GetSettings(long productId, long categoryId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT ProductId, CategoryId, SimilarityThreshold, TopK, UpdatedAtUtc
                                        FROM DefectRecognitionSettings
                                        WHERE ProductId=@ProductId AND CategoryId=@CategoryId LIMIT 1;";
                command.Parameters.AddWithValue("@ProductId", productId);
                command.Parameters.AddWithValue("@CategoryId", categoryId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new DefectRecognitionSettings
                    {
                        ProductId = DbConvert.GetInt64(reader, "ProductId"),
                        CategoryId = DbConvert.GetInt64(reader, "CategoryId"),
                        SimilarityThreshold = DbConvert.GetDouble(reader, "SimilarityThreshold"),
                        TopK = DbConvert.GetInt32(reader, "TopK"),
                        UpdatedAtUtc = DbConvert.GetUtcDateTime(reader, "UpdatedAtUtc")
                    };
                }
            }
        }

        public void UpsertSettings(DefectRecognitionSettings settings)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT INTO DefectRecognitionSettings
                    (ProductId, CategoryId, SimilarityThreshold, TopK, UpdatedAtUtc)
                    VALUES (@ProductId, @CategoryId, @SimilarityThreshold, @TopK, @UpdatedAtUtc)
                    ON CONFLICT(ProductId, CategoryId) DO UPDATE SET
                    SimilarityThreshold=excluded.SimilarityThreshold,
                    TopK=excluded.TopK,
                    UpdatedAtUtc=excluded.UpdatedAtUtc;";
                command.Parameters.AddWithValue("@ProductId", settings.ProductId);
                command.Parameters.AddWithValue("@CategoryId", settings.CategoryId);
                command.Parameters.AddWithValue("@SimilarityThreshold", settings.SimilarityThreshold);
                command.Parameters.AddWithValue("@TopK", settings.TopK);
                command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(settings.UpdatedAtUtc));
                command.ExecuteNonQuery();
            }
        }

        public void ReplacePendingCandidates(long productId, long categoryId, string runCode, IList<DefectRecognitionCandidate> candidates)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                using (SQLiteCommand delete = new SQLiteCommand(
                    "DELETE FROM DefectRecognitionCandidates WHERE ProductId=@ProductId AND CategoryId=@CategoryId;",
                    connection, transaction))
                {
                    delete.Parameters.AddWithValue("@ProductId", productId);
                    delete.Parameters.AddWithValue("@CategoryId", categoryId);
                    delete.ExecuteNonQuery();
                }

                foreach (DefectRecognitionCandidate candidate in candidates)
                {
                    using (SQLiteCommand insert = new SQLiteCommand(@"INSERT INTO DefectRecognitionCandidates
                        (RunCode, ProductId, CategoryId, DatasetImageId, Similarity, GeometryData, Status,
                         ConfirmedAnnotationId, CreatedAtUtc, UpdatedAtUtc)
                        VALUES (@RunCode, @ProductId, @CategoryId, @DatasetImageId, @Similarity, @GeometryData, @Status,
                                @ConfirmedAnnotationId, @CreatedAtUtc, @UpdatedAtUtc);", connection, transaction))
                    {
                        AddCandidateParameters(insert, candidate);
                        insert.Parameters.AddWithValue("@RunCode", runCode);
                        insert.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        public IList<DefectRecognitionCandidate> GetLatestCandidates(long productId, long categoryId)
        {
            List<DefectRecognitionCandidate> items = new List<DefectRecognitionCandidate>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT c.Id, c.RunCode, c.ProductId, c.CategoryId, c.DatasetImageId,
                                               i.FileName AS SourceFileName, c.Similarity, c.GeometryData, c.Status,
                                               c.ConfirmedAnnotationId, c.CreatedAtUtc, c.UpdatedAtUtc
                                        FROM DefectRecognitionCandidates c
                                        INNER JOIN DatasetImages i ON i.Id=c.DatasetImageId
                                        WHERE c.ProductId=@ProductId AND c.CategoryId=@CategoryId
                                          AND c.RunCode=(SELECT RunCode FROM DefectRecognitionCandidates
                                                         WHERE ProductId=@ProductId AND CategoryId=@CategoryId
                                                         ORDER BY Id DESC LIMIT 1)
                                        ORDER BY c.Similarity DESC, c.Id;";
                command.Parameters.AddWithValue("@ProductId", productId);
                command.Parameters.AddWithValue("@CategoryId", categoryId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) items.Add(MapCandidate(reader));
                }
            }
            return items;
        }

        public DefectRecognitionCandidate GetCandidateById(long candidateId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT c.Id, c.RunCode, c.ProductId, c.CategoryId, c.DatasetImageId,
                                               i.FileName AS SourceFileName, c.Similarity, c.GeometryData, c.Status,
                                               c.ConfirmedAnnotationId, c.CreatedAtUtc, c.UpdatedAtUtc
                                        FROM DefectRecognitionCandidates c
                                        INNER JOIN DatasetImages i ON i.Id=c.DatasetImageId
                                        WHERE c.Id=@Id LIMIT 1;";
                command.Parameters.AddWithValue("@Id", candidateId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                    return reader.Read() ? MapCandidate(reader) : null;
            }
        }

        public void UpdateCandidate(DefectRecognitionCandidate candidate)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE DefectRecognitionCandidates SET
                    Similarity=@Similarity, GeometryData=@GeometryData, Status=@Status,
                    ConfirmedAnnotationId=@ConfirmedAnnotationId, UpdatedAtUtc=@UpdatedAtUtc
                    WHERE Id=@Id AND ProductId=@ProductId AND CategoryId=@CategoryId;";
                AddCandidateParameters(command, candidate);
                command.Parameters.AddWithValue("@Id", candidate.Id);
                if (command.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("未找到需要更新的识别候选。Id=" + candidate.Id);
            }
        }

        public IList<DefectHardNegative> GetHardNegatives(long productId, long categoryId)
        {
            List<DefectHardNegative> items = new List<DefectHardNegative>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT h.Id, h.ProductId, h.CategoryId, h.DatasetImageId,
                                               i.FileName AS SourceFileName, h.GeometryData, h.Similarity, h.CreatedAtUtc
                                        FROM DefectHardNegatives h
                                        INNER JOIN DatasetImages i ON i.Id=h.DatasetImageId
                                        WHERE h.ProductId=@ProductId AND h.CategoryId=@CategoryId
                                        ORDER BY h.Id DESC;";
                command.Parameters.AddWithValue("@ProductId", productId);
                command.Parameters.AddWithValue("@CategoryId", categoryId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new DefectHardNegative
                        {
                            Id = DbConvert.GetInt64(reader, "Id"),
                            ProductId = DbConvert.GetInt64(reader, "ProductId"),
                            CategoryId = DbConvert.GetInt64(reader, "CategoryId"),
                            DatasetImageId = DbConvert.GetInt64(reader, "DatasetImageId"),
                            SourceFileName = DbConvert.GetString(reader, "SourceFileName"),
                            GeometryData = DbConvert.GetString(reader, "GeometryData"),
                            Similarity = DbConvert.GetDouble(reader, "Similarity"),
                            CreatedAtUtc = DbConvert.GetUtcDateTime(reader, "CreatedAtUtc")
                        });
                    }
                }
            }
            return items;
        }

        public long InsertHardNegative(DefectHardNegative hardNegative)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            {
                using (SQLiteCommand insert = connection.CreateCommand())
                {
                    insert.CommandText = @"INSERT OR IGNORE INTO DefectHardNegatives
                        (ProductId, CategoryId, DatasetImageId, GeometryData, Similarity, CreatedAtUtc)
                        VALUES (@ProductId, @CategoryId, @DatasetImageId, @GeometryData, @Similarity, @CreatedAtUtc);";
                    insert.Parameters.AddWithValue("@ProductId", hardNegative.ProductId);
                    insert.Parameters.AddWithValue("@CategoryId", hardNegative.CategoryId);
                    insert.Parameters.AddWithValue("@DatasetImageId", hardNegative.DatasetImageId);
                    insert.Parameters.AddWithValue("@GeometryData", hardNegative.GeometryData);
                    insert.Parameters.AddWithValue("@Similarity", hardNegative.Similarity);
                    insert.Parameters.AddWithValue("@CreatedAtUtc", DbConvert.ToUtcText(hardNegative.CreatedAtUtc));
                    insert.ExecuteNonQuery();
                }
                using (SQLiteCommand select = connection.CreateCommand())
                {
                    select.CommandText = @"SELECT Id FROM DefectHardNegatives
                                           WHERE CategoryId=@CategoryId AND DatasetImageId=@DatasetImageId AND GeometryData=@GeometryData LIMIT 1;";
                    select.Parameters.AddWithValue("@CategoryId", hardNegative.CategoryId);
                    select.Parameters.AddWithValue("@DatasetImageId", hardNegative.DatasetImageId);
                    select.Parameters.AddWithValue("@GeometryData", hardNegative.GeometryData);
                    return Convert.ToInt64(select.ExecuteScalar());
                }
            }
        }

        public DefectRecognitionSummary GetSummary(long productId, long categoryId)
        {
            DefectRecognitionSummary summary = new DefectRecognitionSummary();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            {
                using (SQLiteCommand candidates = connection.CreateCommand())
                {
                    candidates.CommandText = @"SELECT
                        COALESCE(SUM(CASE WHEN Status='已确认' THEN 1 ELSE 0 END),0),
                        COALESCE(SUM(CASE WHEN Status='已拒绝' THEN 1 ELSE 0 END),0),
                        COALESCE(SUM(CASE WHEN Status='待确认' THEN 1 ELSE 0 END),0)
                        FROM DefectRecognitionCandidates
                        WHERE ProductId=@ProductId AND CategoryId=@CategoryId
                          AND RunCode=(SELECT RunCode FROM DefectRecognitionCandidates
                                       WHERE ProductId=@ProductId AND CategoryId=@CategoryId
                                       ORDER BY Id DESC LIMIT 1);";
                    candidates.Parameters.AddWithValue("@ProductId", productId);
                    candidates.Parameters.AddWithValue("@CategoryId", categoryId);
                    using (SQLiteDataReader reader = candidates.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            summary.ConfirmedCount = Convert.ToInt32(reader[0]);
                            summary.RejectedCount = Convert.ToInt32(reader[1]);
                            summary.PendingCount = Convert.ToInt32(reader[2]);
                        }
                    }
                }
                using (SQLiteCommand negatives = connection.CreateCommand())
                {
                    negatives.CommandText = "SELECT COUNT(*) FROM DefectHardNegatives WHERE ProductId=@ProductId AND CategoryId=@CategoryId;";
                    negatives.Parameters.AddWithValue("@ProductId", productId);
                    negatives.Parameters.AddWithValue("@CategoryId", categoryId);
                    summary.HardNegativeCount = Convert.ToInt32(negatives.ExecuteScalar());
                }
            }
            return summary;
        }

        private static void AddCandidateParameters(SQLiteCommand command, DefectRecognitionCandidate candidate)
        {
            command.Parameters.AddWithValue("@ProductId", candidate.ProductId);
            command.Parameters.AddWithValue("@CategoryId", candidate.CategoryId);
            command.Parameters.AddWithValue("@DatasetImageId", candidate.DatasetImageId);
            command.Parameters.AddWithValue("@Similarity", candidate.Similarity);
            command.Parameters.AddWithValue("@GeometryData", candidate.GeometryData);
            command.Parameters.AddWithValue("@Status", candidate.Status);
            command.Parameters.AddWithValue("@ConfirmedAnnotationId", DbConvert.DbNullIfMissing(candidate.ConfirmedAnnotationId));
            command.Parameters.AddWithValue("@CreatedAtUtc", DbConvert.ToUtcText(candidate.CreatedAtUtc));
            command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(candidate.UpdatedAtUtc));
        }

        private static DefectRecognitionCandidate MapCandidate(SQLiteDataReader reader)
        {
            return new DefectRecognitionCandidate
            {
                Id = DbConvert.GetInt64(reader, "Id"),
                RunCode = DbConvert.GetString(reader, "RunCode"),
                ProductId = DbConvert.GetInt64(reader, "ProductId"),
                CategoryId = DbConvert.GetInt64(reader, "CategoryId"),
                DatasetImageId = DbConvert.GetInt64(reader, "DatasetImageId"),
                SourceFileName = DbConvert.GetString(reader, "SourceFileName"),
                Similarity = DbConvert.GetDouble(reader, "Similarity"),
                GeometryData = DbConvert.GetString(reader, "GeometryData"),
                Status = DbConvert.GetString(reader, "Status"),
                ConfirmedAnnotationId = DbConvert.GetNullableInt64(reader, "ConfirmedAnnotationId"),
                CreatedAtUtc = DbConvert.GetUtcDateTime(reader, "CreatedAtUtc"),
                UpdatedAtUtc = DbConvert.GetUtcDateTime(reader, "UpdatedAtUtc")
            };
        }
    }
}
