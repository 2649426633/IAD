using System;
using System.IO;
using System.Data.SQLite;

namespace IAD.Infrastructure.Database
{
    internal sealed class SqliteConnectionFactory
    {
        public SqliteConnectionFactory(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                throw new ArgumentException("数据库路径不能为空。", "databasePath");

            DatabasePath = databasePath;
        }

        public string DatabasePath { get; private set; }

        public SQLiteConnection CreateOpenConnection()
        {
            string directory = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = DatabasePath;
            builder.ForeignKeys = true;

            SQLiteConnection connection = new SQLiteConnection(builder.ConnectionString);
            connection.Open();

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON; PRAGMA busy_timeout = 5000;";
                command.ExecuteNonQuery();
            }

            return connection;
        }
    }
}
