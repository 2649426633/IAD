using System;
using System.Data.SQLite;
using IAD.Infrastructure.Database;
using IAD.Models;

namespace IAD.Repositories
{
    internal sealed class ProductDefinitionSettingsRepository : IProductDefinitionSettingsRepository
    {
        private readonly SqliteConnectionFactory connectionFactory;

        public ProductDefinitionSettingsRepository(SqliteConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException("connectionFactory");
        }

        public ProductDefinitionSettings GetByProduct(long productId)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT ProductId, ImageSize, ProductCount, Pose, AcquisitionCondition, ReferenceImagePath,
                                               TemplateType, LocalizationMethod, ModelType, MinScore, AngleRange, ScaleRange,
                                               MatchCount, PixelX, PixelY, LengthUnit, AreaUnit, CalibrationVersion,
                                               CalibrationState, ProductDefinitionVersion, TemplateVersion, UpdatedAtUtc
                                        FROM ProductDefinitionSettings WHERE ProductId = @ProductId LIMIT 1;";
                command.Parameters.AddWithValue("@ProductId", productId);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public void Upsert(ProductDefinitionSettings settings)
        {
            using (SQLiteConnection connection = connectionFactory.CreateOpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT INTO ProductDefinitionSettings
                    (ProductId, ImageSize, ProductCount, Pose, AcquisitionCondition, ReferenceImagePath,
                     TemplateType, LocalizationMethod, ModelType, MinScore, AngleRange, ScaleRange,
                     MatchCount, PixelX, PixelY, LengthUnit, AreaUnit, CalibrationVersion, CalibrationState,
                     ProductDefinitionVersion, TemplateVersion, UpdatedAtUtc)
                    VALUES
                    (@ProductId, @ImageSize, @ProductCount, @Pose, @AcquisitionCondition, @ReferenceImagePath,
                     @TemplateType, @LocalizationMethod, @ModelType, @MinScore, @AngleRange, @ScaleRange,
                     @MatchCount, @PixelX, @PixelY, @LengthUnit, @AreaUnit, @CalibrationVersion, @CalibrationState,
                     @ProductDefinitionVersion, @TemplateVersion, @UpdatedAtUtc)
                    ON CONFLICT(ProductId) DO UPDATE SET
                     ImageSize = excluded.ImageSize,
                     ProductCount = excluded.ProductCount,
                     Pose = excluded.Pose,
                     AcquisitionCondition = excluded.AcquisitionCondition,
                     ReferenceImagePath = excluded.ReferenceImagePath,
                     TemplateType = excluded.TemplateType,
                     LocalizationMethod = excluded.LocalizationMethod,
                     ModelType = excluded.ModelType,
                     MinScore = excluded.MinScore,
                     AngleRange = excluded.AngleRange,
                     ScaleRange = excluded.ScaleRange,
                     MatchCount = excluded.MatchCount,
                     PixelX = excluded.PixelX,
                     PixelY = excluded.PixelY,
                     LengthUnit = excluded.LengthUnit,
                     AreaUnit = excluded.AreaUnit,
                     CalibrationVersion = excluded.CalibrationVersion,
                     CalibrationState = excluded.CalibrationState,
                     ProductDefinitionVersion = excluded.ProductDefinitionVersion,
                     TemplateVersion = excluded.TemplateVersion,
                     UpdatedAtUtc = excluded.UpdatedAtUtc;";
                AddParameters(command, settings);
                command.ExecuteNonQuery();
            }
        }

        private static void AddParameters(SQLiteCommand command, ProductDefinitionSettings settings)
        {
            command.Parameters.AddWithValue("@ProductId", settings.ProductId);
            command.Parameters.AddWithValue("@ImageSize", DbConvert.DbNullIfEmpty(settings.ImageSize));
            command.Parameters.AddWithValue("@ProductCount", settings.ProductCount);
            command.Parameters.AddWithValue("@Pose", DbConvert.DbNullIfEmpty(settings.Pose));
            command.Parameters.AddWithValue("@AcquisitionCondition", DbConvert.DbNullIfEmpty(settings.AcquisitionCondition));
            command.Parameters.AddWithValue("@ReferenceImagePath", DbConvert.DbNullIfEmpty(settings.ReferenceImagePath));
            command.Parameters.AddWithValue("@TemplateType", DbConvert.DbNullIfEmpty(settings.TemplateType));
            command.Parameters.AddWithValue("@LocalizationMethod", DbConvert.DbNullIfEmpty(settings.LocalizationMethod));
            command.Parameters.AddWithValue("@ModelType", DbConvert.DbNullIfEmpty(settings.ModelType));
            command.Parameters.AddWithValue("@MinScore", settings.MinScore);
            command.Parameters.AddWithValue("@AngleRange", DbConvert.DbNullIfEmpty(settings.AngleRange));
            command.Parameters.AddWithValue("@ScaleRange", DbConvert.DbNullIfEmpty(settings.ScaleRange));
            command.Parameters.AddWithValue("@MatchCount", settings.MatchCount);
            command.Parameters.AddWithValue("@PixelX", settings.PixelX);
            command.Parameters.AddWithValue("@PixelY", settings.PixelY);
            command.Parameters.AddWithValue("@LengthUnit", DbConvert.DbNullIfEmpty(settings.LengthUnit));
            command.Parameters.AddWithValue("@AreaUnit", DbConvert.DbNullIfEmpty(settings.AreaUnit));
            command.Parameters.AddWithValue("@CalibrationVersion", DbConvert.DbNullIfEmpty(settings.CalibrationVersion));
            command.Parameters.AddWithValue("@CalibrationState", DbConvert.DbNullIfEmpty(settings.CalibrationState));
            command.Parameters.AddWithValue("@ProductDefinitionVersion", DbConvert.DbNullIfEmpty(settings.ProductDefinitionVersion));
            command.Parameters.AddWithValue("@TemplateVersion", DbConvert.DbNullIfEmpty(settings.TemplateVersion));
            command.Parameters.AddWithValue("@UpdatedAtUtc", DbConvert.ToUtcText(settings.UpdatedAtUtc));
        }

        private static ProductDefinitionSettings Map(SQLiteDataReader reader)
        {
            return new ProductDefinitionSettings
            {
                ProductId = DbConvert.GetInt64(reader, "ProductId"),
                ImageSize = DbConvert.GetString(reader, "ImageSize"),
                ProductCount = DbConvert.GetInt32(reader, "ProductCount"),
                Pose = DbConvert.GetString(reader, "Pose"),
                AcquisitionCondition = DbConvert.GetString(reader, "AcquisitionCondition"),
                ReferenceImagePath = DbConvert.GetString(reader, "ReferenceImagePath"),
                TemplateType = DbConvert.GetString(reader, "TemplateType"),
                LocalizationMethod = DbConvert.GetString(reader, "LocalizationMethod"),
                ModelType = DbConvert.GetString(reader, "ModelType"),
                MinScore = DbConvert.GetDouble(reader, "MinScore"),
                AngleRange = DbConvert.GetString(reader, "AngleRange"),
                ScaleRange = DbConvert.GetString(reader, "ScaleRange"),
                MatchCount = DbConvert.GetInt32(reader, "MatchCount"),
                PixelX = DbConvert.GetDouble(reader, "PixelX"),
                PixelY = DbConvert.GetDouble(reader, "PixelY"),
                LengthUnit = DbConvert.GetString(reader, "LengthUnit"),
                AreaUnit = DbConvert.GetString(reader, "AreaUnit"),
                CalibrationVersion = DbConvert.GetString(reader, "CalibrationVersion"),
                CalibrationState = DbConvert.GetString(reader, "CalibrationState"),
                ProductDefinitionVersion = DbConvert.GetString(reader, "ProductDefinitionVersion"),
                TemplateVersion = DbConvert.GetString(reader, "TemplateVersion"),
                UpdatedAtUtc = DbConvert.GetUtcDateTime(reader, "UpdatedAtUtc")
            };
        }
    }
}
