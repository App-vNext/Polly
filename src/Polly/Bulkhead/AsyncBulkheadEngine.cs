using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Bulkhead.Options;

namespace Polly.Bulkhead
{
   internal static class AsyncBulkheadEngine
    {
       internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            AsyncBulkheadRejectionHandlerBase asyncBulkheadRejectionHandler,
            SemaphoreSlim maxParallelizationSemaphore,
            SemaphoreSlim maxQueuedActionsSemaphore,
            CancellationToken cancellationToken, 
            bool continueOnCapturedContext)
        {
            if (!await maxQueuedActionsSemaphore.WaitAsync(TimeSpan.Zero, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                var handlerTask = asyncBulkheadRejectionHandler?.OnBulkheadRejected(context);
                if (handlerTask is not null) await handlerTask.ConfigureAwait(continueOnCapturedContext);
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
}
