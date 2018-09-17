using System;
using System.Threading;

#if NET40
using SemaphoreSlim = Nito.AsyncEx.AsyncSemaphore;
using Polly.Utilities;
#else
using SemaphoreSlim = System.Threading.SemaphoreSlim;
#endif

namespace Polly.Bulkhead
{
    internal static partial class BulkheadEngine
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
