using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IAD.Infrastructure.Database;
using IAD.Models;

namespace IAD.Repositories
{
    internal sealed class InspectionResultRepository : IInspectionResultRepository
    {
        private readonly SqliteConnectionFactory connectionFactory;

        public InspectionResultRepository(SqliteConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException("connectionFactory");
        }

        public long Save(InspectionResult result)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                long resultId;
                using (SQLiteCommand command = new SQLiteCommand(@"INSERT INTO InspectionResults
                    (ProductId, RecipeId, BatchCode, SourceImagePath, OverallResult, LocalizationScore, ModelVersion, RuleVersion, StartedAtUtc, FinishedAtUtc)
                    VALUES (@ProductId, @RecipeId, @BatchCode, @SourceImagePath, @OverallResult, @LocalizationScore, @ModelVersion, @RuleVersion, @StartedAtUtc, @FinishedAtUtc);",
                    connection,
                    transaction))
                {
                    command.Parameters.AddWithValue("@ProductId", result.ProductId);
                    command.Parameters.AddWithValue("@RecipeId", DbConvert.DbNullIfMissing(result.RecipeId));
                    command.Parameters.AddWithValue("@BatchCode", DbConvert.DbNullIfEmpty(result.BatchCode));
                    command.Parameters.AddWithValue("@SourceImagePath", DbConvert.DbNullIfEmpty(result.SourceImagePath));
                    command.Parameters.AddWithValue("@OverallResult", result.OverallResult);
                    command.Parameters.AddWithValue("@LocalizationScore", result.LocalizationScore);
                    command.Parameters.AddWithValue("@ModelVersion", DbConvert.DbNullIfEmpty(result.ModelVersion));
                    command.Parameters.AddWithValue("@RuleVersion", DbConvert.DbNullIfEmpty(result.RuleVersion));
                    command.Parameters.AddWithValue("@StartedAtUtc", DbConvert.ToUtcText(result.StartedAtUtc));
                    command.Parameters.AddWithValue("@FinishedAtUtc", DbConvert.ToUtcText(result.FinishedAtUtc));
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection, transaction))
                    resultId = Convert.ToInt64(idCommand.ExecuteScalar());

                foreach (DefectInstance defect in result.Defects)
                {
                    using (SQLiteCommand defectCommand = new SQLiteCommand(@"INSERT INTO DefectInstances
                        (InspectionResultId, RoiId, CategoryId, RoiName, CategoryCode, Confidence, X, Y, Area, Width, Height, Result)
                        VALUES (@InspectionResultId, @RoiId, @CategoryId, @RoiName, @CategoryCode, @Confidence, @X, @Y, @Area, @Width, @Height, @Result);",
                        connection,
                        transaction))
                    {
                        defectCommand.Parameters.AddWithValue("@InspectionResultId", resultId);
                        defectCommand.Parameters.AddWithValue("@RoiId", DbConvert.DbNullIfMissing(defect.RoiId));
                        defectCommand.Parameters.AddWithValue("@CategoryId", DbConvert.DbNullIfMissing(defect.CategoryId));
                        defectCommand.Parameters.AddWithValue("@RoiName", DbConvert.DbNullIfEmpty(defect.RoiName));
                        defectCommand.Parameters.AddWithValue("@CategoryCode", DbConvert.DbNullIfEmpty(defect.CategoryCode));
                        defectCommand.Parameters.AddWithValue("@Confidence", defect.Confidence);
                        defectCommand.Parameters.AddWithValue("@X", defect.X);
                        defectCommand.Parameters.AddWithValue("@Y", defect.Y);
                        defectCommand.Parameters.AddWithValue("@Area", defect.Area);
                        defectCommand.Parameters.AddWithValue("@Width", defect.Width);
                        defectCommand.Parameters.AddWithValue("@Height", defect.Height);
                        defectCommand.Parameters.AddWithValue("@Result", defect.Result);
                        defectCommand.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                return resultId;
            }
        }

        public InspectionResult GetById(long id)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            {
                InspectionResult result;
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT Id, ProductId, RecipeId, BatchCode, SourceImagePath, OverallResult, LocalizationScore,
                                                   ModelVersion, RuleVersion, StartedAtUtc, FinishedAtUtc
                                            FROM InspectionResults WHERE Id = @Id LIMIT 1;";
                    command.Parameters.AddWithValue("@Id", id);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        result = MapResult(reader);
                    }
                }

                using (SQLiteCommand defectCommand = connection.CreateCommand())
                {
                    defectCommand.CommandText = @"SELECT Id, InspectionResultId, RoiId, CategoryId, RoiName, CategoryCode,
                                                         Confidence, X, Y, Area, Width, Height, Result
                                                  FROM DefectInstances WHERE InspectionResultId = @ResultId ORDER BY Id;";
                    defectCommand.Parameters.AddWithValue("@ResultId", id);
                    using (SQLiteDataReader reader = defectCommand.ExecuteReader())
                    {
                        while (reader.Read()) result.Defects.Add(MapDefect(reader));
                    }
                }

                return result;
            }
        }

        public IList<InspectionResult> GetRecent(int limit)
        {
            List<InspectionResult> items = new List<InspectionResult>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Id, ProductId, RecipeId, BatchCode, SourceImagePath, OverallResult, LocalizationScore,
                                               ModelVersion, RuleVersion, StartedAtUtc, FinishedAtUtc
                                        FROM InspectionResults ORDER BY FinishedAtUtc DESC LIMIT @Limit;";
                command.Parameters.AddWithValue("@Limit", limit);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) items.Add(MapResult(reader));
                }
            }
            return items;
        }

        private static InspectionResult MapResult(SQLiteDataReader reader)
        {
            return new InspectionResult
            {
                Id = DbConvert.GetInt64(reader, "Id"),
                ProductId = DbConvert.GetInt64(reader, "ProductId"),
                RecipeId = DbConvert.GetNullableInt64(reader, "RecipeId"),
                BatchCode = DbConvert.GetString(reader, "BatchCode"),
                SourceImagePath = DbConvert.GetString(reader, "SourceImagePath"),
                OverallResult = DbConvert.GetString(reader, "OverallResult"),
                LocalizationScore = DbConvert.GetDouble(reader, "LocalizationScore"),
                ModelVersion = DbConvert.GetString(reader, "ModelVersion"),
                RuleVersion = DbConvert.GetString(reader, "RuleVersion"),
                StartedAtUtc = DbConvert.GetUtcDateTime(reader, "StartedAtUtc"),
                FinishedAtUtc = DbConvert.GetUtcDateTime(reader, "FinishedAtUtc")
            };
        }

        private static DefectInstance MapDefect(SQLiteDataReader reader)
        {
            return new DefectInstance
            {
                Id = DbConvert.GetInt64(reader, "Id"),
                InspectionResultId = DbConvert.GetInt64(reader, "InspectionResultId"),
                RoiId = DbConvert.GetNullableInt64(reader, "RoiId"),
                CategoryId = DbConvert.GetNullableInt64(reader, "CategoryId"),
                RoiName = DbConvert.GetString(reader, "RoiName"),
                CategoryCode = DbConvert.GetString(reader, "CategoryCode"),
                Confidence = DbConvert.GetDouble(reader, "Confidence"),
                X = DbConvert.GetDouble(reader, "X"),
                Y = DbConvert.GetDouble(reader, "Y"),
                Area = DbConvert.GetDouble(reader, "Area"),
                Width = DbConvert.GetDouble(reader, "Width"),
                Height = DbConvert.GetDouble(reader, "Height"),
                Result = DbConvert.GetString(reader, "Result")
            };
        }
    }
}
