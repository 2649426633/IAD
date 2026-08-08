using System;
using System.Collections.Generic;
using IAD.Models;
using IAD.Repositories;

namespace IAD.Services
{
    public sealed class ResultService
    {
        private static readonly HashSet<string> ValidResults = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "OK", "NG", "ERROR"
        };

        private readonly IProductRepository products;
        private readonly IInspectionResultRepository results;

        internal ResultService(IProductRepository products, IInspectionResultRepository results)
        {
            this.products = products ?? throw new ArgumentNullException("products");
            this.results = results ?? throw new ArgumentNullException("results");
        }

        public long SaveInspectionResult(InspectionResult result)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (result.ProductId <= 0 || products.GetById(result.ProductId) == null)
                throw new InvalidOperationException("检测结果关联的产品不存在。Id=" + result.ProductId);

            string overall = string.IsNullOrWhiteSpace(result.OverallResult) ? null : result.OverallResult.Trim().ToUpperInvariant();
            if (overall == null || !ValidResults.Contains(overall))
                throw new ArgumentException("OverallResult只能是 OK、NG 或 ERROR。", "result");

            result.OverallResult = overall;
            if (result.StartedAtUtc == DateTime.MinValue) result.StartedAtUtc = DateTime.UtcNow;
            if (result.FinishedAtUtc == DateTime.MinValue) result.FinishedAtUtc = DateTime.UtcNow;
            if (result.FinishedAtUtc < result.StartedAtUtc)
                throw new ArgumentException("检测结束时间不能早于开始时间。", "result");

            foreach (DefectInstance defect in result.Defects)
            {
                defect.Result = string.IsNullOrWhiteSpace(defect.Result) ? "NG" : defect.Result.Trim().ToUpperInvariant();
                if (!ValidResults.Contains(defect.Result))
                    throw new ArgumentException("缺陷实例Result只能是 OK、NG 或 ERROR。", "result");
            }

            result.Id = results.Save(result);
            return result.Id;
        }

        public InspectionResult GetResult(long resultId)
        {
            return results.GetById(resultId);
        }

        public IList<InspectionResult> GetRecentResults(int limit)
        {
            if (limit <= 0) limit = 50;
            if (limit > 500) limit = 500;
            return results.GetRecent(limit);
        }
    }
}
