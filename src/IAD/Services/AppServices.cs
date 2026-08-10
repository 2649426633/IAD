using System;
using IAD.Infrastructure.Database;
using IAD.Infrastructure.Storage;
using IAD.Repositories;

namespace IAD.Services
{
    internal static class AppServices
    {
        private static bool initialized;

        public static ProductService Products { get; private set; }
        public static DatasetService Datasets { get; private set; }
        public static AnnotationEditingService AnnotationEditing { get; private set; }
        public static DatasetMaskService Masks { get; private set; }
        public static DefectRecognitionService DefectRecognition { get; private set; }
        public static RecipeService Recipes { get; private set; }
        public static ResultService Results { get; private set; }
        public static string DatabasePath { get { return ProjectStoragePaths.DatabasePath; } }

        public static void Initialize()
        {
            if (initialized) return;

            ProjectStoragePaths.EnsureCreated();

            SqliteConnectionFactory connectionFactory = new SqliteConnectionFactory(ProjectStoragePaths.DatabasePath);
            DatabaseInitializer.Initialize(connectionFactory);
            DatasetMaskDatabaseMigration.Apply(connectionFactory);

            IProductRepository productRepository = new ProductRepository(connectionFactory);
            IProductDefinitionSettingsRepository definitionSettingsRepository = new ProductDefinitionSettingsRepository(connectionFactory);
            IDefectCategoryRepository categoryRepository = new DefectCategoryRepository(connectionFactory);
            IRoiRepository roiRepository = new RoiRepository(connectionFactory);
            IInspectionRecipeRepository recipeRepository = new InspectionRecipeRepository(connectionFactory);
            IInspectionResultRepository resultRepository = new InspectionResultRepository(connectionFactory);
            IDatasetRepository datasetRepository = new DatasetRepository(connectionFactory);
            IDatasetMaskRepository maskRepository = new DatasetMaskRepository(connectionFactory);
            IDefectRecognitionRepository recognitionRepository = new DefectRecognitionRepository(connectionFactory);

            Products = new ProductService(productRepository, definitionSettingsRepository, categoryRepository, roiRepository);
            Datasets = new DatasetService(productRepository, definitionSettingsRepository, categoryRepository, datasetRepository);
            AnnotationEditing = new AnnotationEditingService(datasetRepository, categoryRepository);
            Masks = new DatasetMaskService(datasetRepository, categoryRepository, maskRepository);
            DefectRecognition = new DefectRecognitionService(productRepository, categoryRepository, Datasets, recognitionRepository);
            Recipes = new RecipeService(productRepository, recipeRepository);
            Results = new ResultService(productRepository, resultRepository);

            try { Masks.CleanupOrphanFiles(); }
            catch { }

            initialized = true;
        }

        public static void EnsureInitialized()
        {
            if (!initialized)
                throw new InvalidOperationException("应用服务尚未初始化，请先调用 AppServices.Initialize()。 ");
        }
    }
}
