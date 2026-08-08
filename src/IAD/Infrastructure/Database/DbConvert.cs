using System;
using System.Globalization;
using System.Data.SQLite;

namespace IAD.Infrastructure.Database
{
    internal static class DbConvert
    {
        public static string ToUtcText(DateTime value)
        {
            DateTime utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
            return utc.ToString("O", CultureInfo.InvariantCulture);
        }

        public static DateTime GetUtcDateTime(SQLiteDataReader reader, string column)
        {
            string text = Convert.ToString(reader[column], CultureInfo.InvariantCulture);
            DateTime value;
            if (!DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out value))
                return DateTime.MinValue;

            return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        }

        public static string GetString(SQLiteDataReader reader, string column)
        {
            object value = reader[column];
            return value == DBNull.Value ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static long GetInt64(SQLiteDataReader reader, string column)
        {
            return Convert.ToInt64(reader[column], CultureInfo.InvariantCulture);
        }

        public static long? GetNullableInt64(SQLiteDataReader reader, string column)
        {
            object value = reader[column];
            return value == DBNull.Value ? (long?)null : Convert.ToInt64(value, CultureInfo.InvariantCulture);
        }

        public static int GetInt32(SQLiteDataReader reader, string column)
        {
            return Convert.ToInt32(reader[column], CultureInfo.InvariantCulture);
        }

        public static double GetDouble(SQLiteDataReader reader, string column)
        {
            object value = reader[column];
            return value == DBNull.Value ? 0D : Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        public static bool GetBoolean(SQLiteDataReader reader, string column)
        {
            return Convert.ToInt32(reader[column], CultureInfo.InvariantCulture) != 0;
        }

        public static object DbNullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        public static object DbNullIfMissing(long? value)
        {
            return value.HasValue ? (object)value.Value : DBNull.Value;
        }
    }
}
