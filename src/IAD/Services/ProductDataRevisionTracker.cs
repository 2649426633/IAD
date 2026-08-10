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
}
