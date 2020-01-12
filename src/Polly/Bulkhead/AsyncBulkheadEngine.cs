using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Bulkhead
{
   internal static class AsyncBulkheadEngine
    {
        internal static async Task<TResult> ImplementationAsync<TExecutableAsync, TResult>(
            TExecutableAsync action,
            Context context,
            Func<Context, Task> onBulkheadRejectedAsync,
            SemaphoreSlim maxParallelizationSemaphore,
            SemaphoreSlim maxQueuedActionsSemaphore,
            CancellationToken cancellationToken, 
            bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            if (!await maxQueuedActionsSemaphore.WaitAsync(TimeSpan.Zero, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                if (onBulkheadRejectedAsync != null) { await onBulkheadRejectedAsync(context).ConfigureAwait(continueOnCapturedContext); }
                throw new BulkheadRejectedException();
            }
            try
            {
                await maxParallelizationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);

                try 
                {
                    return await action.ExecuteAsync(context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
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
}
