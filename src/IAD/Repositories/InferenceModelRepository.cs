using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IAD.Infrastructure.Database;
using IAD.Models;

namespace IAD.Repositories
{
    internal sealed class InferenceModelRepository : IInferenceModelRepository
    {
        private const string SelectColumns = @"Id, ProductId, ModelCode, ModelName, Version, ModelType, RelativePath, Sha256,
            InputName, OutputName, InputWidth, InputHeight, Labels, ConfidenceThreshold, NmsThreshold,
            IsActive, CreatedAtUtc, UpdatedAtUtc";
        private readonly SqliteConnectionFactory connectionFactory;

        public InferenceModelRepository(SqliteConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException("connectionFactory");
        }

        public IList<InferenceModel> GetByProduct(long productId)
        {
            List<InferenceModel> items = new List<InferenceModel>();
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + SelectColumns + " FROM InferenceModels WHERE ProductId=@ProductId ORDER BY IsActive DESC, UpdatedAtUtc DESC;";
                command.Parameters.AddWithValue("@ProductId", productId);
                using (SQLiteDataReader reader = command.ExecuteReader()) while (reader.Read()) items.Add(Map(reader));
            }
            return items;
        }

        public InferenceModel GetById(long id) { return GetSingle("Id=@Value", id); }
        public InferenceModel GetActiveByProduct(long productId) { return GetSingle("ProductId=@Value AND IsActive=1 ORDER BY UpdatedAtUtc DESC", productId); }

        public long Insert(InferenceModel model)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO InferenceModels
                        (ProductId, ModelCode, ModelName, Version, ModelType, RelativePath, Sha256, InputName, OutputName,
                         InputWidth, InputHeight, Labels, ConfidenceThreshold, NmsThreshold, IsActive, CreatedAtUtc, UpdatedAtUtc)
                        VALUES (@ProductId,@ModelCode,@ModelName,@Version,@ModelType,@RelativePath,@Sha256,@InputName,@OutputName,
                                @InputWidth,@InputHeight,@Labels,@ConfidenceThreshold,@NmsThreshold,@IsActive,@CreatedAtUtc,@UpdatedAtUtc);";
                    AddParameters(command, model);
                    command.ExecuteNonQuery();
                }
                using (SQLiteCommand command = new SQLiteCommand("SELECT last_insert_rowid();", connection)) return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        public void Update(InferenceModel model)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE InferenceModels SET ModelName=@ModelName, ModelType=@ModelType,
                    InputName=@InputName, OutputName=@OutputName, InputWidth=@InputWidth, InputHeight=@InputHeight,
                    Labels=@Labels, ConfidenceThreshold=@ConfidenceThreshold, NmsThreshold=@NmsThreshold,
                    IsActive=@IsActive, UpdatedAtUtc=@UpdatedAtUtc WHERE Id=@Id AND ProductId=@ProductId;";
                AddParameters(command, model);
                command.Parameters.AddWithValue("@Id", model.Id);
                if (command.ExecuteNonQuery() == 0) throw new InvalidOperationException("未找到需要更新的模型。Id=" + model.Id);
            }
        }

        public void Activate(long productId, long modelId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                using (SQLiteCommand clear = new SQLiteCommand("UPDATE InferenceModels SET IsActive=0 WHERE ProductId=@ProductId;", connection, transaction))
                {
                    clear.Parameters.AddWithValue("@ProductId", productId);
                    clear.ExecuteNonQuery();
                }
                using (SQLiteCommand activate = new SQLiteCommand("UPDATE InferenceModels SET IsActive=1, UpdatedAtUtc=@Now WHERE Id=@Id AND ProductId=@ProductId;", connection, transaction))
                {
                    activate.Parameters.AddWithValue("@Now", DbConvert.ToUtcText(DateTime.UtcNow));
                    activate.Parameters.AddWithValue("@Id", modelId);
                    activate.Parameters.AddWithValue("@ProductId", productId);
                    if (activate.ExecuteNonQuery() == 0) throw new InvalidOperationException("未找到需要启用的模型。Id=" + modelId);
                }
                transaction.Commit();
            }
        }

        public void Delete(long productId, long modelId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"DELETE FROM InferenceModels WHERE ProductId=@ProductId AND Id=@Id
                    AND NOT EXISTS(SELECT 1 FROM InspectionRecipes WHERE ModelId=@Id);";
                command.Parameters.AddWithValue("@ProductId", productId);
                command.Parameters.AddWithValue("@Id", modelId);
                if (command.ExecuteNonQuery() == 0) throw new InvalidOperationException("模型不存在，或已被 Recipe 引用，不能删除。");
            }
        }

        private InferenceModel GetSingle(string whereSql, long value)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + SelectColumns + " FROM InferenceModels WHERE " + whereSql + " LIMIT 1;";
                command.Parameters.AddWithValue("@Value", value);
                using (SQLiteDataReader reader = command.ExecuteReader()) return reader.Read() ? Map(reader) : null;
            }
        }

        private static void AddParameters(SQLiteCommand command, InferenceModel model)
        {
            command.Parameters.AddWithValue("@ProductId", model.ProductId);
            command.Parameters.AddWithValue("@ModelCode", model.ModelCode);
            command.Parameters.AddWithValue("@ModelName", model.ModelName);
            command.Parameters.AddWithValue("@Version", model.Version);
            command.Parameters.AddWithValue("@ModelType", model.ModelType);
            command.Parameters.AddWithValue("@RelativePath", model.RelativePath);
            command.Parameters.AddWithValue("@Sha256", model.Sha256);
            command.Parameters.AddWithValue("@InputName", model.InputName);
            command.Parameters.AddWithValue("@OutputName", model.OutputName);
            command.Parameters.AddWithValue("@InputWidth", model.InputWidth);
            command.Parameters.AddWithValue("@InputHeight", model.InputHeight);
            command.Parameters.AddWithValue("@Labels", DbConvert.DbNullIfEmpty(model.Labels));
            command.Parameters.AddWithValue("@ConfidenceThreshold", model.ConfidenceThreshold);
            command.Parameters.AddWithValue("@NmsThreshold", model.NmsThreshold);
            command.Parameters.AddWithValue("@IsActive", model.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("@CreatedAtUtc", DbConvert.ToUtcText(model.CreatedAtUtc));
            command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(model.UpdatedAtUtc));
        }

        private static InferenceModel Map(SQLiteDataReader reader)
        {
            return new InferenceModel
            {
                Id=DbConvert.GetInt64(reader,"Id"), ProductId=DbConvert.GetInt64(reader,"ProductId"),
                ModelCode=DbConvert.GetString(reader,"ModelCode"), ModelName=DbConvert.GetString(reader,"ModelName"),
                Version=DbConvert.GetString(reader,"Version"), ModelType=DbConvert.GetString(reader,"ModelType"),
                RelativePath=DbConvert.GetString(reader,"RelativePath"), Sha256=DbConvert.GetString(reader,"Sha256"),
                InputName=DbConvert.GetString(reader,"InputName"), OutputName=DbConvert.GetString(reader,"OutputName"),
                InputWidth=DbConvert.GetInt32(reader,"InputWidth"), InputHeight=DbConvert.GetInt32(reader,"InputHeight"),
                Labels=DbConvert.GetString(reader,"Labels"), ConfidenceThreshold=DbConvert.GetDouble(reader,"ConfidenceThreshold"),
                NmsThreshold=DbConvert.GetDouble(reader,"NmsThreshold"), IsActive=DbConvert.GetBoolean(reader,"IsActive"),
                CreatedAtUtc=DbConvert.GetUtcDateTime(reader,"CreatedAtUtc"), UpdatedAtUtc=DbConvert.GetUtcDateTime(reader,"UpdatedAtUtc")
            };
        }
    }
}
