using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Bulkhead
{
    public partial class BulkheadPolicy
    {
        internal BulkheadPolicy(Func<Func<CancellationToken, Task>, Context, CancellationToken, bool, Task> asyncExceptionPolicy,
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

    public partial class BulkheadPolicy<TResult>
    {
        internal BulkheadPolicy(
            Func<Func<CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> asyncExecutionPolicy,
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