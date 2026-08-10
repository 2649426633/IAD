using System;
using System.Data.SQLite;

namespace IAD.Infrastructure.Database
{
    internal static class DatasetMaskDatabaseMigration
    {
        public const int SchemaVersion = 7;

        public static void Apply(SqliteConnectionFactory connectionFactory)
        {
            if (connectionFactory == null) throw new ArgumentNullException("connectionFactory");

            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                Execute(connection, transaction, @"
                    CREATE TABLE IF NOT EXISTS DatasetMasks (
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        DatasetImageId INTEGER NOT NULL,
                        CategoryId INTEGER NULL,
                        CategoryCode TEXT NULL,
                        CategoryName TEXT NOT NULL,
                        RelativePath TEXT NOT NULL,
                        Width INTEGER NOT NULL,
                        Height INTEGER NOT NULL,
                        Revision INTEGER NOT NULL DEFAULT 1,
                        PixelCount INTEGER NOT NULL DEFAULT 0,
                        IsVisible INTEGER NOT NULL DEFAULT 1,
                        CreatedAtUtc TEXT NOT NULL,
                        UpdatedAtUtc TEXT NOT NULL,
                        FOREIGN KEY(DatasetImageId) REFERENCES DatasetImages(Id) ON DELETE CASCADE,
                        FOREIGN KEY(CategoryId) REFERENCES DefectCategories(Id) ON DELETE SET NULL
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS UX_DatasetMasks_Image_Category
                        ON DatasetMasks(DatasetImageId, CategoryId)
                        WHERE CategoryId IS NOT NULL;
                    CREATE INDEX IF NOT EXISTS IX_DatasetMasks_Image ON DatasetMasks(DatasetImageId);

                    CREATE TABLE IF NOT EXISTS DatasetVersionMasks (
                        VersionId INTEGER NOT NULL,
                        SourceMaskId INTEGER NOT NULL,
                        SourceImageId INTEGER NOT NULL,
                        CategoryCode TEXT NULL,
                        CategoryName TEXT NOT NULL,
                        RelativePath TEXT NOT NULL,
                        Width INTEGER NOT NULL,
                        Height INTEGER NOT NULL,
                        Revision INTEGER NOT NULL,
                        PixelCount INTEGER NOT NULL,
                        IsVisible INTEGER NOT NULL,
                        PRIMARY KEY(VersionId, SourceMaskId),
                        FOREIGN KEY(VersionId) REFERENCES DatasetVersions(Id) ON DELETE CASCADE
                    );
                    CREATE INDEX IF NOT EXISTS IX_DatasetVersionMasks_Image
                        ON DatasetVersionMasks(VersionId, SourceImageId);

                    CREATE TRIGGER IF NOT EXISTS TR_DatasetVersions_SnapshotMasks
                    AFTER INSERT ON DatasetVersions
                    BEGIN
                        INSERT INTO DatasetVersionMasks
                            (VersionId, SourceMaskId, SourceImageId, CategoryCode, CategoryName,
                             RelativePath, Width, Height, Revision, PixelCount, IsVisible)
                        SELECT NEW.Id, m.Id, m.DatasetImageId,
                               COALESCE(c.CategoryCode, m.CategoryCode),
                               COALESCE(c.CategoryName, m.CategoryName),
                               m.RelativePath, m.Width, m.Height, m.Revision, m.PixelCount, m.IsVisible
                        FROM DatasetMasks m
                        INNER JOIN DatasetImages i ON i.Id = m.DatasetImageId
                        LEFT JOIN DefectCategories c ON c.Id = m.CategoryId
                        WHERE i.ProductId = NEW.ProductId;
                    END;
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

        private static void Execute(SQLiteConnection connection, SQLiteTransaction transaction, string sql)
        {
            using (SQLiteCommand command = new SQLiteCommand(sql, connection, transaction))
                command.ExecuteNonQuery();
        }
    }
}
