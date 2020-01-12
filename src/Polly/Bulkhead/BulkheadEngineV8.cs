using System;
using System.Threading;

namespace Polly.Bulkhead
{
    internal static class BulkheadEngine
    {
        internal static TResult Implementation<TExecutable, TResult>(
            in TExecutable action,
            Context context,
            Action<Context> onBulkheadRejected,
            SemaphoreSlim maxParallelizationSemaphore,
            SemaphoreSlim maxQueuedActionsSemaphore,
            CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>
        {
            if (!maxQueuedActionsSemaphore.Wait(TimeSpan.Zero, cancellationToken))
            {
                onBulkheadRejected?.Invoke(context);
                throw new BulkheadRejectedException();
            }
            
            try
            {
                maxParallelizationSemaphore.Wait(cancellationToken);
                try
                {
                    return action.Execute(context, cancellationToken);
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
