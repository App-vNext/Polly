using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Bulkhead
{
   internal static partial class BulkheadEngine
    {
       internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            Context context,
            Func<Context, Task> onBulkheadRejectedAsync,
            SemaphoreSlim maxParallelizationSemaphore,
            SemaphoreSlim maxQueuedActionsSemaphore,
            CancellationToken cancellationToken, 
            bool continueOnCapturedContext)
        {
#if NET40
            if (!maxQueuedActionsSemaphore.Wait(TimeSpan.Zero, cancellationToken)) 
            {
                await onBulkheadRejectedAsync(context).ConfigureAwait(continueOnCapturedContext);
                throw new BulkheadRejectedException(); 
            }

#else
            if (!await maxQueuedActionsSemaphore.WaitAsync(TimeSpan.Zero, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                await onBulkheadRejectedAsync(context).ConfigureAwait(continueOnCapturedContext);
                throw new BulkheadRejectedException();
            }
#endif
            try
            {
#if NET40
                maxParallelizationSemaphore.Wait(cancellationToken);
#else
                await maxParallelizationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
#endif
                try 
                {
                    return await action(cancellationToken).ConfigureAwait(continueOnCapturedContext);
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
