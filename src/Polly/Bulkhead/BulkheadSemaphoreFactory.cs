#nullable enable
namespace Polly.Bulkhead;

internal static class BulkheadSemaphoreFactory
{
    public static (SemaphoreSlim MaxParallelizationSemaphore, SemaphoreSlim MaxQueuedActionsSemaphore) CreateBulkheadSemaphores(int maxParallelization, int maxQueueingActions)
    {
        var maxParallelizationSemaphore = new SemaphoreSlim(maxParallelization, maxParallelization);

        var maxQueuingCompounded = (int)Math.Min((long)maxQueueingActions + maxParallelization, int.MaxValue);
        var maxQueuedActionsSemaphore = new SemaphoreSlim(maxQueuingCompounded, maxQueuingCompounded);

        return (maxParallelizationSemaphore, maxQueuedActionsSemaphore);
    }
}
