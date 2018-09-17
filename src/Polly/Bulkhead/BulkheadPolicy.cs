using System;
using System.Threading;
using Polly.Utilities;

#if NET40
using SemaphoreSlim = Nito.AsyncEx.AsyncSemaphore;
#else
using SemaphoreSlim = System.Threading.SemaphoreSlim;
#endif

namespace Polly.Bulkhead
{
    /// <summary>
    /// A bulkhead-isolation policy which can be applied to delegates.
    /// </summary>
    public partial class BulkheadPolicy : Policy, IBulkheadPolicy
    {
        private readonly SemaphoreSlim _maxParallelizationSemaphore;
        private readonly SemaphoreSlim _maxQueuedActionsSemaphore;
        private readonly int _maxParallelization;
        private readonly int _maxQueueingActions;

        internal BulkheadPolicy(
            Action<Action<Context, CancellationToken>, Context, CancellationToken> exceptionPolicy, 
            int maxParallelization,
            int maxQueueingActions,
            SemaphoreSlim maxParallelizationSemaphore, 
            SemaphoreSlim maxQueuedActionsSemaphore
            ) : base(exceptionPolicy, PredicateHelper.EmptyExceptionPredicates)
        {
            _maxParallelization = maxParallelization;
            _maxQueueingActions = maxQueueingActions;
            _maxParallelizationSemaphore = maxParallelizationSemaphore;
            _maxQueuedActionsSemaphore = maxQueuedActionsSemaphore;
        }

        /// <summary>
        /// Gets the number of slots currently available for executing actions through the bulkhead.
        /// </summary>
        public int BulkheadAvailableCount => _maxParallelizationSemaphore.CurrentCount;

        /// <summary>
        /// Gets the number of slots currently available for queuing actions for execution through the bulkhead.
        /// </summary>
        public int QueueAvailableCount => Math.Min(_maxQueuedActionsSemaphore.CurrentCount, _maxQueueingActions);

        /// <summary>
        /// Disposes of the <see cref="BulkheadPolicy"/>, allowing it to dispose its internal resources.  
        /// <remarks>Only call <see cref="Dispose()"/> on a <see cref="BulkheadPolicy"/> after all actions executed through the policy have completed.  If actions are still executing through the policy when <see cref="Dispose()"/> is called, an <see cref="ObjectDisposedException"/> may be thrown on the actions' threads when those actions complete.</remarks>
        /// </summary>
        public void Dispose()
        {
            _maxParallelizationSemaphore.Dispose();
            _maxQueuedActionsSemaphore.Dispose();
        }
    }

    /// <summary>
    /// A bulkhead-isolation policy which can be applied to delegates returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    public partial class BulkheadPolicy<TResult> : Policy<TResult>, IBulkheadPolicy<TResult>
    {
        private readonly SemaphoreSlim _maxParallelizationSemaphore;
        private readonly SemaphoreSlim _maxQueuedActionsSemaphore;
        private readonly int _maxParallelization;
        private readonly int _maxQueueingActions;

        internal BulkheadPolicy(
            Func<Func<Context, CancellationToken, TResult>, Context, CancellationToken, TResult> executionPolicy,
            int maxParallelization,
            int maxQueueingActions,
            SemaphoreSlim maxParallelizationSemaphore,
            SemaphoreSlim maxQueuedActionsSemaphore
            ) : base(executionPolicy, PredicateHelper.EmptyExceptionPredicates, PredicateHelper<TResult>.EmptyResultPredicates)
        {
            _maxParallelization = maxParallelization;
            _maxQueueingActions = maxQueueingActions;
            _maxParallelizationSemaphore = maxParallelizationSemaphore;
            _maxQueuedActionsSemaphore = maxQueuedActionsSemaphore;
        }


        /// <summary>
        /// Gets the number of slots currently available for executing actions through the bulkhead.
        /// </summary>
        public int BulkheadAvailableCount => _maxParallelizationSemaphore.CurrentCount;

        /// <summary>
        /// Gets the number of slots currently available for queuing actions for execution through the bulkhead.
        /// </summary>
        public int QueueAvailableCount => Math.Min(_maxQueuedActionsSemaphore.CurrentCount, _maxQueueingActions);

        /// <summary>
        /// Disposes of the <see cref="BulkheadPolicy"/>, allowing it to dispose its internal resources.  
        /// <remarks>Only call <see cref="Dispose()"/> on a <see cref="BulkheadPolicy"/> after all actions executed through the policy have completed.  If actions are still executing through the policy when <see cref="Dispose()"/> is called, an <see cref="ObjectDisposedException"/> may be thrown on the actions' threads when those actions complete.</remarks>
        /// </summary>
        public void Dispose()
        {
            _maxParallelizationSemaphore.Dispose();
            _maxQueuedActionsSemaphore.Dispose();
        }
    }
}
