using System;
using System.Data.SQLite;

namespace IAD.Infrastructure.Database
{
    internal static class InferenceDatabaseMigration
    {
        public const int SchemaVersion = 10;

        public static void Apply(SqliteConnectionFactory connectionFactory)
        {
            if (connectionFactory == null) throw new ArgumentNullException("connectionFactory");

            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                Execute(connection, transaction, @"CREATE TABLE IF NOT EXISTS InferenceModels (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ProductId INTEGER NOT NULL,
                    ModelCode TEXT NOT NULL,
                    ModelName TEXT NOT NULL,
                    Version TEXT NOT NULL,
                    ModelType TEXT NOT NULL,
                    RelativePath TEXT NOT NULL,
                    Sha256 TEXT NOT NULL,
                    InputName TEXT NOT NULL,
                    OutputName TEXT NOT NULL,
                    InputWidth INTEGER NOT NULL,
                    InputHeight INTEGER NOT NULL,
                    Labels TEXT NULL,
                    ConfidenceThreshold REAL NOT NULL DEFAULT 0.5,
                    NmsThreshold REAL NOT NULL DEFAULT 0.45,
                    IsActive INTEGER NOT NULL DEFAULT 0,
                    CreatedAtUtc TEXT NOT NULL,
                    UpdatedAtUtc TEXT NOT NULL,
                    FOREIGN KEY(ProductId) REFERENCES Products(Id) ON DELETE CASCADE
                );");
                Execute(connection, transaction, "CREATE UNIQUE INDEX IF NOT EXISTS UX_InferenceModels_Product_Code_Version ON InferenceModels(ProductId, ModelCode, Version);");
                Execute(connection, transaction, "CREATE INDEX IF NOT EXISTS IX_InferenceModels_Product_Active ON InferenceModels(ProductId, IsActive);");

                Execute(connection, transaction, @"CREATE TABLE IF NOT EXISTS RecipeRules (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    RecipeId INTEGER NOT NULL,
                    CategoryId INTEGER NULL,
                    CategoryCode TEXT NOT NULL,
                    CategoryName TEXT NULL,
                    RoiName TEXT NULL,
                    MinConfidence REAL NOT NULL DEFAULT 0.5,
                    MinArea REAL NOT NULL DEFAULT 0,
                    MinWidth REAL NOT NULL DEFAULT 0,
                    MinHeight REAL NOT NULL DEFAULT 0,
                    MaxAllowedCount INTEGER NOT NULL DEFAULT 0,
                    Decision TEXT NOT NULL DEFAULT 'NG',
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    FOREIGN KEY(RecipeId) REFERENCES InspectionRecipes(Id) ON DELETE CASCADE,
                    FOREIGN KEY(CategoryId) REFERENCES DefectCategories(Id) ON DELETE SET NULL
                );");
                Execute(connection, transaction, "CREATE UNIQUE INDEX IF NOT EXISTS UX_RecipeRules_Recipe_Category_Roi ON RecipeRules(RecipeId, CategoryCode, IFNULL(RoiName, ''));");

                EnsureColumn(connection, transaction, "InspectionRecipes", "ModelId", "INTEGER NULL");
                EnsureColumn(connection, transaction, "InspectionResults", "ModelId", "INTEGER NULL");
                EnsureColumn(connection, transaction, "InspectionResults", "OriginalImagePath", "TEXT NULL");
                EnsureColumn(connection, transaction, "InspectionResults", "ArchivedImagePath", "TEXT NULL");
                EnsureColumn(connection, transaction, "InspectionResults", "AnnotatedImagePath", "TEXT NULL");
                EnsureColumn(connection, transaction, "InspectionResults", "InferenceMilliseconds", "INTEGER NOT NULL DEFAULT 0");
                EnsureColumn(connection, transaction, "InspectionResults", "OperatorName", "TEXT NULL");
                EnsureColumn(connection, transaction, "InspectionResults", "ErrorMessage", "TEXT NULL");
                EnsureColumn(connection, transaction, "DefectInstances", "CategoryName", "TEXT NULL");
                EnsureColumn(connection, transaction, "DefectInstances", "RuleDecision", "TEXT NULL");

                using (SQLiteCommand version = new SQLiteCommand("INSERT OR REPLACE INTO SchemaInfo (Key, Value) VALUES ('SchemaVersion', @Value);", connection, transaction))
                {
                    version.Parameters.AddWithValue("@Value", SchemaVersion.ToString());
                    version.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }

        private static void Execute(SQLiteConnection connection, SQLiteTransaction transaction, string sql)
        {
            using (SQLiteCommand command = new SQLiteCommand(sql, connection, transaction)) command.ExecuteNonQuery();
        }

        private static void EnsureColumn(SQLiteConnection connection, SQLiteTransaction transaction, string tableName, string columnName, string definition)
        {
            bool exists = false;
            using (SQLiteCommand command = new SQLiteCommand("PRAGMA table_info(" + tableName + ");", connection, transaction))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (string.Equals(Convert.ToString(reader["name"]), columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }
            }
            if (!exists) Execute(connection, transaction, "ALTER TABLE " + tableName + " ADD COLUMN " + columnName + " " + definition + ";");
        }
    }
}
