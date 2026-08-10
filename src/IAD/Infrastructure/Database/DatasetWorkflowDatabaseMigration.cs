using System;
using System.Data.SQLite;

namespace IAD.Infrastructure.Database
{
    internal static class DatasetWorkflowDatabaseMigration
    {
        public const int SchemaVersion = 9;

        public static void Apply(SqliteConnectionFactory connectionFactory)
        {
            if (connectionFactory == null) throw new ArgumentNullException("connectionFactory");

            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                EnsureColumn(connection, transaction, "DatasetImages", "ReviewStatus", "TEXT NOT NULL DEFAULT 'Pending'");
                EnsureColumn(connection, transaction, "DatasetImages", "DatasetSplit", "TEXT NOT NULL DEFAULT 'Unassigned'");
                EnsureColumn(connection, transaction, "DatasetImages", "ContentHash", "TEXT NULL");
                EnsureColumn(connection, transaction, "DatasetImages", "ReviewComment", "TEXT NULL");
                EnsureColumn(connection, transaction, "DatasetImages", "ReviewedBy", "TEXT NULL");
                EnsureColumn(connection, transaction, "DatasetImages", "ReviewedAtUtc", "TEXT NULL");
                EnsureColumn(connection, transaction, "DatasetVersions", "MaskCount", "INTEGER NOT NULL DEFAULT 0");
                EnsureColumn(connection, transaction, "DatasetVersionImages", "ReviewStatus", "TEXT NOT NULL DEFAULT 'Pending'");
                EnsureColumn(connection, transaction, "DatasetVersionImages", "DatasetSplit", "TEXT NOT NULL DEFAULT 'Unassigned'");
                EnsureColumn(connection, transaction, "DatasetVersionImages", "ContentHash", "TEXT NULL");
                EnsureColumn(connection, transaction, "DatasetVersionImages", "ReviewComment", "TEXT NULL");
                EnsureColumn(connection, transaction, "DatasetVersionImages", "ReviewedBy", "TEXT NULL");
                EnsureColumn(connection, transaction, "DatasetVersionImages", "ReviewedAtUtc", "TEXT NULL");

                Execute(connection, transaction, @"
                    UPDATE DatasetImages
                    SET ReviewStatus = CASE
                        WHEN Status = '已通过' THEN 'Approved'
                        WHEN Status = '正常样本' THEN 'Normal'
                        WHEN Status = '已驳回' THEN 'Rejected'
                        WHEN Status = '已忽略' THEN 'Ignored'
                        ELSE COALESCE(NULLIF(ReviewStatus, ''), 'Pending') END;

                    UPDATE DatasetVersionImages
                    SET ReviewStatus = COALESCE(NULLIF(ReviewStatus, ''), 'Pending'),
                        DatasetSplit = COALESCE(NULLIF(DatasetSplit, ''), 'Unassigned');

                    UPDATE DatasetVersions
                    SET MaskCount = (SELECT COUNT(*) FROM DatasetVersionMasks vm WHERE vm.VersionId = DatasetVersions.Id);

                    CREATE UNIQUE INDEX IF NOT EXISTS UX_DatasetImages_Product_ContentHash
                        ON DatasetImages(ProductId, ContentHash)
                        WHERE ContentHash IS NOT NULL AND TRIM(ContentHash) <> '';
                    CREATE INDEX IF NOT EXISTS IX_DatasetImages_Product_Review
                        ON DatasetImages(ProductId, ReviewStatus, DatasetSplit);
                ");

                using (SQLiteCommand versionCommand = new SQLiteCommand(
                    "INSERT OR REPLACE INTO SchemaInfo (Key, Value) VALUES ('SchemaVersion', @Value);",
                    connection,
                    transaction))
                {
                    versionCommand.Parameters.AddWithValue("@Value", SchemaVersion.ToString());
                    versionCommand.ExecuteNonQuery();
                }

                transaction.Commit();
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
                    if (!string.Equals(Convert.ToString(reader["name"]), columnName, StringComparison.OrdinalIgnoreCase)) continue;
                    exists = true;
                    break;
                }
            }
            if (exists) return;
            using (SQLiteCommand alter = new SQLiteCommand(
                "ALTER TABLE " + tableName + " ADD COLUMN " + columnName + " " + definition + ";",
                connection,
                transaction))
                alter.ExecuteNonQuery();
        }

        private static void Execute(SQLiteConnection connection, SQLiteTransaction transaction, string sql)
        {
            using (SQLiteCommand command = new SQLiteCommand(sql, connection, transaction))
                command.ExecuteNonQuery();
        }
    }
}
