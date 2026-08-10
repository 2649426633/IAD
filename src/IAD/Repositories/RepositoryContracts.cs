using System;
using System.Collections.Generic;
using IAD.Models;

namespace IAD.Repositories
{
    public interface IProductRepository
    {
        IList<Product> GetAll();
        Product GetById(long id);
        Product GetByCode(string productCode);
        long Insert(Product product);
        void Update(Product product);
    }

    public interface IProductDefinitionSettingsRepository
    {
        ProductDefinitionSettings GetByProduct(long productId);
        void Upsert(ProductDefinitionSettings settings);
    }

    public interface IDefectCategoryRepository
    {
        IList<DefectCategory> GetByProduct(long productId);
        DefectCategory GetById(long id);
        long Insert(DefectCategory category);
        void Update(DefectCategory category);
        void Delete(long id, long productId);
    }

    public interface IRoiRepository
    {
        IList<RoiDefinition> GetByProduct(long productId);
        RoiDefinition GetById(long id);
        long Insert(RoiDefinition roi);
        void Update(RoiDefinition roi);
        void Delete(long id, long productId);
        void DeleteByProduct(long productId);
    }

    public interface IInspectionRecipeRepository
    {
        IList<InspectionRecipe> GetByProduct(long productId);
        InspectionRecipe GetById(long id);
        InspectionRecipe GetActiveByProduct(long productId);
        long Insert(InspectionRecipe recipe);
        void Update(InspectionRecipe recipe);
        void Activate(long productId, long recipeId);
    }

    public interface IInspectionResultRepository
    {
        long Save(InspectionResult result);
        InspectionResult GetById(long id);
        IList<InspectionResult> GetRecent(int limit);
    }

    public interface IDatasetRepository
    {
        IList<DatasetImage> GetImagesByProduct(long productId);
        DatasetImage GetImageById(long imageId);
        DatasetImage GetImageByContentHash(long productId, string contentHash);
        long InsertImage(DatasetImage image);
        void UpdateImageStatus(long imageId, string status, DateTime updatedAtUtc);
        void UpdateImageWorkflow(DatasetImage image);
        void UpdateImageContentHash(long imageId, string contentHash, DateTime updatedAtUtc);
        bool IsImageReferencedByVersion(long imageId);
        void DeleteImage(long imageId, long productId);
        IList<DatasetAnnotation> GetAnnotationsByImage(long imageId);
        long InsertAnnotation(DatasetAnnotation annotation);
        void UpdateAnnotation(DatasetAnnotation annotation);
        void DeleteAnnotation(long annotationId, long imageId);
        DatasetVersion GetLatestVersion(long productId);
        IList<DatasetVersion> GetVersions(long productId);
        IList<DatasetVersionImage> GetVersionImages(long versionId);
        IList<DatasetVersionAnnotation> GetVersionAnnotations(long versionId);
        IList<DatasetVersionMask> GetVersionMasks(long versionId);
        void RestoreVersion(long productId, long versionId, DateTime restoredAtUtc);
        long InsertVersion(DatasetVersion version);
        int CountImages(long productId);
        int CountAnnotations(long productId);
        int CountMasks(long productId);
        IDictionary<long, int> GetClassCounts(long productId);
        IList<string> GetAllReferencedImagePaths();
    }

    public interface IDatasetMaskRepository
    {
        IList<DatasetMask> GetByImage(long imageId);
        DatasetMask GetByImageAndCategory(long imageId, long categoryId);
        DatasetMask GetById(long maskId);
        long Insert(DatasetMask mask);
        void Update(DatasetMask mask);
        void Delete(long maskId, long imageId);
        bool IsRelativePathReferencedByVersion(string relativePath);
        IList<string> GetAllReferencedRelativePaths();
    }

    public interface IDefectRecognitionRepository
    {
        DefectRecognitionSettings GetSettings(long productId, long categoryId);
        void UpsertSettings(DefectRecognitionSettings settings);
        void ReplacePendingCandidates(long productId, long categoryId, string runCode, IList<DefectRecognitionCandidate> candidates);
        IList<DefectRecognitionCandidate> GetLatestCandidates(long productId, long categoryId);
        DefectRecognitionCandidate GetCandidateById(long candidateId);
        void UpdateCandidate(DefectRecognitionCandidate candidate);
        IList<DefectHardNegative> GetHardNegatives(long productId, long categoryId);
        long InsertHardNegative(DefectHardNegative hardNegative);
        DefectRecognitionSummary GetSummary(long productId, long categoryId);
    }
}
