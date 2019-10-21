using System.Threading;

namespace Polly.Bulkhead
{
    internal static class BulkheadSemaphoreFactory
    {
        public static (SemaphoreSlim, SemaphoreSlim) CreateBulkheadSemaphores(int maxParallelization, int maxQueueingActions)
        {
            var maxParallelizationSemaphore = new SemaphoreSlim(maxParallelization, maxParallelization);

            var maxQueuingCompounded = maxQueueingActions <= int.MaxValue - maxParallelization
                ? maxQueueingActions + maxParallelization
                : int.MaxValue;
            var maxQueuedActionsSemaphore = new SemaphoreSlim(maxQueuingCompounded, maxQueuingCompounded);

            return (maxParallelizationSemaphore, maxQueuedActionsSemaphore);
        }
    }
}
