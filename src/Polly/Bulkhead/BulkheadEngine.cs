#nullable enable
namespace Polly.Bulkhead;

internal static class BulkheadEngine
{
    internal static TResult Implementation<TResult>(
        Func<Context, CancellationToken, TResult> action,
        Context context,
        Action<Context> onBulkheadRejected,
        SemaphoreSlim maxParallelizationSemaphore,
        SemaphoreSlim maxQueuedActionsSemaphore,
        CancellationToken cancellationToken)
    {
        if (!maxQueuedActionsSemaphore.Wait(TimeSpan.Zero, cancellationToken))
        {
            onBulkheadRejected(context);
            throw new BulkheadRejectedException();
        }

        try
        {
            maxParallelizationSemaphore.Wait(cancellationToken);
            try
            {
                return action(context, cancellationToken);
            }
            finally
            {
                SafeRelease(maxParallelizationSemaphore);
            }
        }
        finally
        {
            SafeRelease(maxQueuedActionsSemaphore);
        }

        static void SafeRelease(SemaphoreSlim semaphore)
        {
            try
            {
                semaphore.Release();
            }
            catch (ObjectDisposedException)
            {
                // Ignore - this can happen if the semaphore was not acquired.
            }
        }
    }
}
