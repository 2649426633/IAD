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
                    (ProductId, RecipeId, ModelId, BatchCode, SourceImagePath, OriginalImagePath, ArchivedImagePath, AnnotatedImagePath,
                     OverallResult, LocalizationScore, ModelVersion, RuleVersion, InferenceMilliseconds, OperatorName, ErrorMessage, StartedAtUtc, FinishedAtUtc)
                    VALUES (@ProductId, @RecipeId, @ModelId, @BatchCode, @SourceImagePath, @OriginalImagePath, @ArchivedImagePath, @AnnotatedImagePath,
                            @OverallResult, @LocalizationScore, @ModelVersion, @RuleVersion, @InferenceMilliseconds, @OperatorName, @ErrorMessage, @StartedAtUtc, @FinishedAtUtc);",
                    connection,
                    transaction))
                {
                    command.Parameters.AddWithValue("@ProductId", result.ProductId);
                    command.Parameters.AddWithValue("@RecipeId", DbConvert.DbNullIfMissing(result.RecipeId));
                    command.Parameters.AddWithValue("@ModelId", DbConvert.DbNullIfMissing(result.ModelId));
                    command.Parameters.AddWithValue("@BatchCode", DbConvert.DbNullIfEmpty(result.BatchCode));
                    command.Parameters.AddWithValue("@SourceImagePath", DbConvert.DbNullIfEmpty(result.SourceImagePath));
                    command.Parameters.AddWithValue("@OriginalImagePath", DbConvert.DbNullIfEmpty(result.OriginalImagePath));
                    command.Parameters.AddWithValue("@ArchivedImagePath", DbConvert.DbNullIfEmpty(result.ArchivedImagePath));
                    command.Parameters.AddWithValue("@AnnotatedImagePath", DbConvert.DbNullIfEmpty(result.AnnotatedImagePath));
                    command.Parameters.AddWithValue("@OverallResult", result.OverallResult);
                    command.Parameters.AddWithValue("@LocalizationScore", result.LocalizationScore);
                    command.Parameters.AddWithValue("@ModelVersion", DbConvert.DbNullIfEmpty(result.ModelVersion));
                    command.Parameters.AddWithValue("@RuleVersion", DbConvert.DbNullIfEmpty(result.RuleVersion));
                    command.Parameters.AddWithValue("@InferenceMilliseconds", result.InferenceMilliseconds);
                    command.Parameters.AddWithValue("@OperatorName", DbConvert.DbNullIfEmpty(result.OperatorName));
                    command.Parameters.AddWithValue("@ErrorMessage", DbConvert.DbNullIfEmpty(result.ErrorMessage));
                    command.Parameters.AddWithValue("@StartedAtUtc", DbConvert.ToUtcText(result.StartedAtUtc));
                    command.Parameters.AddWithValue("@FinishedAtUtc", DbConvert.ToUtcText(result.FinishedAtUtc));
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection, transaction))
                    resultId = Convert.ToInt64(idCommand.ExecuteScalar());

                foreach (DefectInstance defect in result.Defects)
                {
                    using (SQLiteCommand defectCommand = new SQLiteCommand(@"INSERT INTO DefectInstances
                        (InspectionResultId, RoiId, CategoryId, RoiName, CategoryCode, CategoryName, Confidence, X, Y, Area, Width, Height, Result, RuleDecision)
                        VALUES (@InspectionResultId, @RoiId, @CategoryId, @RoiName, @CategoryCode, @CategoryName, @Confidence, @X, @Y, @Area, @Width, @Height, @Result, @RuleDecision);",
                        connection,
                        transaction))
                    {
                        defectCommand.Parameters.AddWithValue("@InspectionResultId", resultId);
                        defectCommand.Parameters.AddWithValue("@RoiId", DbConvert.DbNullIfMissing(defect.RoiId));
                        defectCommand.Parameters.AddWithValue("@CategoryId", DbConvert.DbNullIfMissing(defect.CategoryId));
                        defectCommand.Parameters.AddWithValue("@RoiName", DbConvert.DbNullIfEmpty(defect.RoiName));
                        defectCommand.Parameters.AddWithValue("@CategoryCode", DbConvert.DbNullIfEmpty(defect.CategoryCode));
                        defectCommand.Parameters.AddWithValue("@CategoryName", DbConvert.DbNullIfEmpty(defect.CategoryName));
                        defectCommand.Parameters.AddWithValue("@Confidence", defect.Confidence);
                        defectCommand.Parameters.AddWithValue("@X", defect.X);
                        defectCommand.Parameters.AddWithValue("@Y", defect.Y);
                        defectCommand.Parameters.AddWithValue("@Area", defect.Area);
                        defectCommand.Parameters.AddWithValue("@Width", defect.Width);
                        defectCommand.Parameters.AddWithValue("@Height", defect.Height);
                        defectCommand.Parameters.AddWithValue("@Result", defect.Result);
                        defectCommand.Parameters.AddWithValue("@RuleDecision", DbConvert.DbNullIfEmpty(defect.RuleDecision));
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
                    command.CommandText = @"SELECT Id, ProductId, RecipeId, ModelId, BatchCode, SourceImagePath, OriginalImagePath,
                                                   ArchivedImagePath, AnnotatedImagePath, OverallResult, LocalizationScore,
                                                   ModelVersion, RuleVersion, InferenceMilliseconds, OperatorName, ErrorMessage, StartedAtUtc, FinishedAtUtc
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
                    defectCommand.CommandText = @"SELECT Id, InspectionResultId, RoiId, CategoryId, RoiName, CategoryCode, CategoryName,
                                                         Confidence, X, Y, Area, Width, Height, Result, RuleDecision
                                                  FROM DefectInstances WHERE InspectionResultId = @ResultId ORDER BY Id;";
                    defectCommand.Parameters.AddWithValue("@ResultId", id);
                    using (SQLiteDataReader reader = defectCommand.ExecuteReader())
                    {
                        while (reader.Read()) result.Defects.Add(MapDefect(reader));
                    }
                }

                result.DefectCount = result.Defects.Count;

                return result;
            }
        }

        public IList<InspectionResult> GetRecent(int limit)
        {
            return Query(new InspectionResultQuery { Limit = limit });
        }

        public IList<InspectionResult> Query(InspectionResultQuery query)
        {
            if (query == null) query = new InspectionResultQuery();
            int limit = query.Limit <= 0 ? 50 : Math.Min(query.Limit, 1000);
            List<InspectionResult> items = new List<InspectionResult>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                string sql = @"SELECT r.Id, r.ProductId, r.RecipeId, r.ModelId, r.BatchCode, r.SourceImagePath, r.OriginalImagePath,
                    r.ArchivedImagePath, r.AnnotatedImagePath, r.OverallResult, r.LocalizationScore, r.ModelVersion, r.RuleVersion,
                    r.InferenceMilliseconds, r.OperatorName, r.ErrorMessage, r.StartedAtUtc, r.FinishedAtUtc,
                    (SELECT COUNT(*) FROM DefectInstances dc WHERE dc.InspectionResultId=r.Id) AS DefectCount
                    FROM InspectionResults r WHERE 1=1";
                if (query.ProductId.HasValue) { sql += " AND r.ProductId=@ProductId"; command.Parameters.AddWithValue("@ProductId", query.ProductId.Value); }
                if (query.RecipeId.HasValue) { sql += " AND r.RecipeId=@RecipeId"; command.Parameters.AddWithValue("@RecipeId", query.RecipeId.Value); }
                if (query.FromUtc.HasValue) { sql += " AND r.FinishedAtUtc>=@FromUtc"; command.Parameters.AddWithValue("@FromUtc", DbConvert.ToUtcText(query.FromUtc.Value)); }
                if (query.ToUtc.HasValue) { sql += " AND r.FinishedAtUtc<=@ToUtc"; command.Parameters.AddWithValue("@ToUtc", DbConvert.ToUtcText(query.ToUtc.Value)); }
                if (!string.IsNullOrWhiteSpace(query.OverallResult)) { sql += " AND r.OverallResult=@OverallResult"; command.Parameters.AddWithValue("@OverallResult", query.OverallResult.Trim().ToUpperInvariant()); }
                if (!string.IsNullOrWhiteSpace(query.CategoryCode))
                {
                    sql += " AND EXISTS(SELECT 1 FROM DefectInstances d WHERE d.InspectionResultId=r.Id AND d.CategoryCode=@CategoryCode)";
                    command.Parameters.AddWithValue("@CategoryCode", query.CategoryCode.Trim());
                }
                if (!string.IsNullOrWhiteSpace(query.Keyword))
                {
                    sql += " AND (IFNULL(r.BatchCode,'') LIKE @Keyword OR IFNULL(r.SourceImagePath,'') LIKE @Keyword OR CAST(r.Id AS TEXT) LIKE @Keyword)";
                    command.Parameters.AddWithValue("@Keyword", "%" + query.Keyword.Trim() + "%");
                }
                command.CommandText = sql + " ORDER BY r.FinishedAtUtc DESC LIMIT @Limit;";
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
                ModelId = DbConvert.GetNullableInt64(reader, "ModelId"),
                BatchCode = DbConvert.GetString(reader, "BatchCode"),
                SourceImagePath = DbConvert.GetString(reader, "SourceImagePath"),
                OriginalImagePath = DbConvert.GetString(reader, "OriginalImagePath"),
                ArchivedImagePath = DbConvert.GetString(reader, "ArchivedImagePath"),
                AnnotatedImagePath = DbConvert.GetString(reader, "AnnotatedImagePath"),
                OverallResult = DbConvert.GetString(reader, "OverallResult"),
                LocalizationScore = DbConvert.GetDouble(reader, "LocalizationScore"),
                ModelVersion = DbConvert.GetString(reader, "ModelVersion"),
                RuleVersion = DbConvert.GetString(reader, "RuleVersion"),
                InferenceMilliseconds = DbConvert.GetInt64(reader, "InferenceMilliseconds"),
                OperatorName = DbConvert.GetString(reader, "OperatorName"),
                ErrorMessage = DbConvert.GetString(reader, "ErrorMessage"),
                DefectCount = HasColumn(reader, "DefectCount") ? DbConvert.GetInt32(reader, "DefectCount") : 0,
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
                CategoryName = DbConvert.GetString(reader, "CategoryName"),
                Confidence = DbConvert.GetDouble(reader, "Confidence"),
                X = DbConvert.GetDouble(reader, "X"),
                Y = DbConvert.GetDouble(reader, "Y"),
                Area = DbConvert.GetDouble(reader, "Area"),
                Width = DbConvert.GetDouble(reader, "Width"),
                Height = DbConvert.GetDouble(reader, "Height"),
                Result = DbConvert.GetString(reader, "Result"),
                RuleDecision = DbConvert.GetString(reader, "RuleDecision")
            };
        }

        private static bool HasColumn(SQLiteDataReader reader, string column)
        {
            for (int i=0; i<reader.FieldCount; i++) if (string.Equals(reader.GetName(i), column, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
