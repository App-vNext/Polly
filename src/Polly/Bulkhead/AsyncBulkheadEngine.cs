#nullable enable
namespace Polly.Bulkhead;

internal static class AsyncBulkheadEngine
{
    internal static async Task<TResult> ImplementationAsync<TResult>(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        Func<Context, Task> onBulkheadRejectedAsync,
        SemaphoreSlim maxParallelizationSemaphore,
        SemaphoreSlim maxQueuedActionsSemaphore,
        bool continueOnCapturedContext,
        CancellationToken cancellationToken)
    {
        if (!await maxQueuedActionsSemaphore.WaitAsync(TimeSpan.Zero, cancellationToken).ConfigureAwait(continueOnCapturedContext))
        {
            await onBulkheadRejectedAsync(context).ConfigureAwait(continueOnCapturedContext);
            throw new BulkheadRejectedException();
        }

        try
        {
            await maxParallelizationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);

            try
            {
                return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                maxParallelizationSemaphore.Release();
            }
        }
        finally
        {
            maxQueuedActionsSemaphore.Release();
        }
    }
}
