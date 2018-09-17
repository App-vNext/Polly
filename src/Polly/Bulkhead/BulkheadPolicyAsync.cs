using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

#if NET40
using SemaphoreSlim = Nito.AsyncEx.AsyncSemaphore;
#else
using SemaphoreSlim = System.Threading.SemaphoreSlim;
#endif

namespace Polly.Bulkhead
{
    public partial class BulkheadPolicy : IBulkheadPolicy
    {
        internal BulkheadPolicy(Func<Func<Context, CancellationToken, Task>, Context, CancellationToken, bool, Task> asyncExceptionPolicy,
            int maxParallelization,
            int maxQueueingActions,
            SemaphoreSlim maxParallelizationSemaphore,
            SemaphoreSlim maxQueuedActionsSemaphore)
           : base(asyncExceptionPolicy, PredicateHelper.EmptyExceptionPredicates)
        {
            _maxParallelization = maxParallelization;
            _maxQueueingActions = maxQueueingActions;
            _maxParallelizationSemaphore = maxParallelizationSemaphore;
            _maxQueuedActionsSemaphore = maxQueuedActionsSemaphore;
        }
    }

    public partial class BulkheadPolicy<TResult> : IBulkheadPolicy<TResult>
    {
        internal BulkheadPolicy(
            Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> asyncExecutionPolicy,
            int maxParallelization,
            int maxQueueingActions,
            SemaphoreSlim maxParallelizationSemaphore,
            SemaphoreSlim maxQueuedActionsSemaphore
            ) : base(asyncExecutionPolicy, PredicateHelper.EmptyExceptionPredicates, PredicateHelper<TResult>.EmptyResultPredicates)
        {
            _maxParallelization = maxParallelization;
            _maxQueueingActions = maxQueueingActions;
            _maxParallelizationSemaphore = maxParallelizationSemaphore;
            _maxQueuedActionsSemaphore = maxQueuedActionsSemaphore;
        }
    }
}