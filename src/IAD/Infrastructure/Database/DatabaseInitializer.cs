using System;
using System.Data.SQLite;

namespace IAD.Infrastructure.Database
{
    internal static class DatabaseInitializer
    {
        public const int CurrentSchemaVersion = 3;

        public static void Initialize(SqliteConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException("connectionFactory");

            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            {
                using (SQLiteCommand pragma = connection.CreateCommand())
                {
                    pragma.CommandText = "PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL;";
                    pragma.ExecuteNonQuery();
                }

                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    foreach (string sql in SchemaStatements)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(sql, connection, transaction))
                            command.ExecuteNonQuery();
                    }

                    EnsureColumn(connection, transaction, "DefectCategories", "DefectType", "TEXT NULL");
                    EnsureColumn(connection, transaction, "DefectCategories", "DetectionStrategy", "TEXT NULL");
                    EnsureColumn(connection, transaction, "DefectCategories", "DefaultThreshold", "REAL NOT NULL DEFAULT 0.8");
                    EnsureColumn(connection, transaction, "DefectCategories", "MinArea", "REAL NOT NULL DEFAULT 0");
                    EnsureColumn(connection, transaction, "DefectCategories", "MinLength", "REAL NOT NULL DEFAULT 0");

                    using (SQLiteCommand versionCommand = new SQLiteCommand(
                        "INSERT OR REPLACE INTO SchemaInfo (Key, Value) VALUES ('SchemaVersion', @Value);",
                        connection,
                        transaction))
                    {
                        versionCommand.Parameters.AddWithValue("@Value", CurrentSchemaVersion.ToString());
                        versionCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
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

            if (!exists)
            {
                using (SQLiteCommand alter = new SQLiteCommand("ALTER TABLE " + tableName + " ADD COLUMN " + columnName + " " + definition + ";", connection, transaction))
                    alter.ExecuteNonQuery();
            }
        }

        private static readonly string[] SchemaStatements =
        {
            @"CREATE TABLE IF NOT EXISTS SchemaInfo (
                Key TEXT NOT NULL PRIMARY KEY,
                Value TEXT NOT NULL
            );",

            @"CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ProductCode TEXT NOT NULL,
                ProductName TEXT NOT NULL,
                Description TEXT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAtUtc TEXT NOT NULL,
                UpdatedAtUtc TEXT NOT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS UX_Products_ProductCode ON Products(ProductCode);",

            @"CREATE TABLE IF NOT EXISTS ProductDefinitionSettings (
                ProductId INTEGER NOT NULL PRIMARY KEY,
                ImageSize TEXT NULL,
                ProductCount INTEGER NOT NULL DEFAULT 1,
                Pose TEXT NULL,
                AcquisitionCondition TEXT NULL,
                ReferenceImagePath TEXT NULL,
                TemplateType TEXT NULL,
                LocalizationMethod TEXT NULL,
                ModelType TEXT NULL,
                MinScore REAL NOT NULL DEFAULT 0.8,
                AngleRange TEXT NULL,
                ScaleRange TEXT NULL,
                MatchCount INTEGER NOT NULL DEFAULT 1,
                PixelX REAL NOT NULL DEFAULT 1,
                PixelY REAL NOT NULL DEFAULT 1,
                LengthUnit TEXT NULL,
                AreaUnit TEXT NULL,
                CalibrationVersion TEXT NULL,
                CalibrationState TEXT NULL,
                ProductDefinitionVersion TEXT NULL,
                TemplateVersion TEXT NULL,
                UpdatedAtUtc TEXT NOT NULL,
                FOREIGN KEY(ProductId) REFERENCES Products(Id) ON DELETE CASCADE
            );",

            @"CREATE TABLE IF NOT EXISTS DefectCategories (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER NOT NULL,
                CategoryCode TEXT NOT NULL,
                CategoryName TEXT NOT NULL,
                DefectType TEXT NULL,
                DetectionStrategy TEXT NULL,
                DefaultThreshold REAL NOT NULL DEFAULT 0.8,
                MinArea REAL NOT NULL DEFAULT 0,
                MinLength REAL NOT NULL DEFAULT 0,
                DisplayOrder INTEGER NOT NULL DEFAULT 0,
                IsEnabled INTEGER NOT NULL DEFAULT 1,
                CreatedAtUtc TEXT NOT NULL,
                UpdatedAtUtc TEXT NOT NULL,
                FOREIGN KEY(ProductId) REFERENCES Products(Id) ON DELETE CASCADE
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS UX_DefectCategories_Product_Code ON DefectCategories(ProductId, CategoryCode);",

            @"CREATE TABLE IF NOT EXISTS ProductRois (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER NOT NULL,
                RoiName TEXT NOT NULL,
                RoiType TEXT NOT NULL,
                CenterX REAL NOT NULL DEFAULT 0,
                CenterY REAL NOT NULL DEFAULT 0,
                Width REAL NOT NULL DEFAULT 0,
                Height REAL NOT NULL DEFAULT 0,
                AngleDeg REAL NOT NULL DEFAULT 0,
                GeometryJson TEXT NULL,
                SortIndex INTEGER NOT NULL DEFAULT 0,
                IsEnabled INTEGER NOT NULL DEFAULT 1,
                CreatedAtUtc TEXT NOT NULL,
                UpdatedAtUtc TEXT NOT NULL,
                FOREIGN KEY(ProductId) REFERENCES Products(Id) ON DELETE CASCADE
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS UX_ProductRois_Product_Name ON ProductRois(ProductId, RoiName);",

            @"CREATE TABLE IF NOT EXISTS InspectionRecipes (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER NOT NULL,
                RecipeCode TEXT NOT NULL,
                RecipeName TEXT NOT NULL,
                DatasetVersion TEXT NULL,
                LocalizationTemplateVersion TEXT NULL,
                ModelVersion TEXT NULL,
                RuleVersion TEXT NULL,
                CalibrationVersion TEXT NULL,
                ThresholdVersion TEXT NULL,
                IsActive INTEGER NOT NULL DEFAULT 0,
                CreatedAtUtc TEXT NOT NULL,
                UpdatedAtUtc TEXT NOT NULL,
                FOREIGN KEY(ProductId) REFERENCES Products(Id) ON DELETE RESTRICT
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS UX_InspectionRecipes_Product_Code ON InspectionRecipes(ProductId, RecipeCode);",
            @"CREATE INDEX IF NOT EXISTS IX_InspectionRecipes_Product_Active ON InspectionRecipes(ProductId, IsActive);",

            @"CREATE TABLE IF NOT EXISTS InspectionResults (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER NOT NULL,
                RecipeId INTEGER NULL,
                BatchCode TEXT NULL,
                SourceImagePath TEXT NULL,
                OverallResult TEXT NOT NULL,
                LocalizationScore REAL NOT NULL DEFAULT 0,
                ModelVersion TEXT NULL,
                RuleVersion TEXT NULL,
                StartedAtUtc TEXT NOT NULL,
                FinishedAtUtc TEXT NOT NULL,
                FOREIGN KEY(ProductId) REFERENCES Products(Id) ON DELETE RESTRICT,
                FOREIGN KEY(RecipeId) REFERENCES InspectionRecipes(Id) ON DELETE SET NULL
            );",
            @"CREATE INDEX IF NOT EXISTS IX_InspectionResults_Product_Time ON InspectionResults(ProductId, FinishedAtUtc DESC);",
            @"CREATE INDEX IF NOT EXISTS IX_InspectionResults_Result_Time ON InspectionResults(OverallResult, FinishedAtUtc DESC);",

            @"CREATE TABLE IF NOT EXISTS DefectInstances (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                InspectionResultId INTEGER NOT NULL,
                RoiId INTEGER NULL,
                CategoryId INTEGER NULL,
                RoiName TEXT NULL,
                CategoryCode TEXT NULL,
                Confidence REAL NOT NULL DEFAULT 0,
                X REAL NOT NULL DEFAULT 0,
                Y REAL NOT NULL DEFAULT 0,
                Area REAL NOT NULL DEFAULT 0,
                Width REAL NOT NULL DEFAULT 0,
                Height REAL NOT NULL DEFAULT 0,
                Result TEXT NOT NULL,
                FOREIGN KEY(InspectionResultId) REFERENCES InspectionResults(Id) ON DELETE CASCADE,
                FOREIGN KEY(RoiId) REFERENCES ProductRois(Id) ON DELETE SET NULL,
                FOREIGN KEY(CategoryId) REFERENCES DefectCategories(Id) ON DELETE SET NULL
            );",
            @"CREATE INDEX IF NOT EXISTS IX_DefectInstances_Result ON DefectInstances(InspectionResultId);"
        };
    }
}
