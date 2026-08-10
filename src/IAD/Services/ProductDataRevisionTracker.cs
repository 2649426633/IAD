using System.Collections.Concurrent;

namespace IAD.Services
{
    internal static class ProductDataRevisionTracker
    {
        private static readonly ConcurrentDictionary<long, long> Revisions = new ConcurrentDictionary<long, long>();

        public static long GetRevision(long productId)
        {
            if (productId <= 0) return 0;
            long revision;
            return Revisions.TryGetValue(productId, out revision) ? revision : 0;
        }

        public static long MarkChanged(long productId)
        {
            if (productId <= 0) return 0;
            return Revisions.AddOrUpdate(productId, 1, delegate(long _, long current)
            {
                return current == long.MaxValue ? 1 : current + 1;
            });
        }
    }

    internal static class InspectionConfigurationRevisionTracker
    {
        private static readonly ConcurrentDictionary<long, long> Revisions = new ConcurrentDictionary<long, long>();
        public static long GetRevision(long productId) { long value; return productId > 0 && Revisions.TryGetValue(productId, out value) ? value : 0; }
        public static long MarkChanged(long productId) { return productId <= 0 ? 0 : Revisions.AddOrUpdate(productId, 1, delegate(long _, long current) { return current == long.MaxValue ? 1 : current + 1; }); }
    }

    internal static class InspectionResultRevisionTracker
    {
        private static readonly ConcurrentDictionary<long, long> Revisions = new ConcurrentDictionary<long, long>();
        public static long GetRevision(long productId) { long value; return productId > 0 && Revisions.TryGetValue(productId, out value) ? value : 0; }
        public static long MarkChanged(long productId) { return productId <= 0 ? 0 : Revisions.AddOrUpdate(productId, 1, delegate(long _, long current) { return current == long.MaxValue ? 1 : current + 1; }); }
    }
}
