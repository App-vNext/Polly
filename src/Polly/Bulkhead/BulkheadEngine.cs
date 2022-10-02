using System;
using System.Threading;
using Polly.Bulkhead.Settings;

namespace Polly.Bulkhead
{
    internal static class BulkheadEngine
    {
        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            IBulkheadRejectedCallback onBulkheadRejectedCallback,
            SemaphoreSlim maxParallelizationSemaphore,
            SemaphoreSlim maxQueuedActionsSemaphore,
            CancellationToken cancellationToken)
        {
            if (!maxQueuedActionsSemaphore.Wait(TimeSpan.Zero, cancellationToken))
            {
                onBulkheadRejectedCallback?.OnBulkheadRejected(context);
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
