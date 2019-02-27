using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Bulkhead
{
    /// <summary>
    /// A bulkhead-isolation policy which can be applied to delegates.
    /// </summary>
    public class AsyncBulkheadPolicy : AsyncPolicy, IBulkheadPolicy
    {
        private readonly SemaphoreSlim _maxParallelizationSemaphore;
        private readonly SemaphoreSlim _maxQueuedActionsSemaphore;
        private readonly int _maxParallelization;
        private readonly int _maxQueueingActions;
        private Func<Context, Task> _onBulkheadRejectedAsync;

        internal AsyncBulkheadPolicy(
            int maxParallelization,
            int maxQueueingActions,
            SemaphoreSlim maxParallelizationSemaphore,
            SemaphoreSlim maxQueuedActionsSemaphore,
            Func<Context, Task> onBulkheadRejectedAsync)
        {
            _maxParallelization = maxParallelization;
            _maxQueueingActions = maxQueueingActions;
            _maxParallelizationSemaphore = maxParallelizationSemaphore;
            _maxQueuedActionsSemaphore = maxQueuedActionsSemaphore;
            _onBulkheadRejectedAsync = onBulkheadRejectedAsync ?? throw new ArgumentNullException(nameof(onBulkheadRejectedAsync));
        }

        /// <summary>
        /// Gets the number of slots currently available for executing actions through the bulkhead.
        /// </summary>
        public int BulkheadAvailableCount => _maxParallelizationSemaphore.CurrentCount;

        /// <summary>
        /// Gets the number of slots currently available for queuing actions for execution through the bulkhead.
        /// </summary>
        public int QueueAvailableCount => Math.Min(_maxQueuedActionsSemaphore.CurrentCount, _maxQueueingActions);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            return AsyncBulkheadEngine.ImplementationAsync(action, context, _onBulkheadRejectedAsync, _maxParallelizationSemaphore, _maxQueuedActionsSemaphore, cancellationToken, continueOnCapturedContext);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _maxParallelizationSemaphore.Dispose();
            _maxQueuedActionsSemaphore.Dispose();
        }
    }

    /// <summary>
    /// A bulkhead-isolation policy which can be applied to delegates.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class AsyncBulkheadPolicy<TResult> : AsyncPolicy<TResult>, IBulkheadPolicy<TResult>
    {
        private readonly SemaphoreSlim _maxParallelizationSemaphore;
        private readonly SemaphoreSlim _maxQueuedActionsSemaphore;
        private readonly int _maxParallelization;
        private readonly int _maxQueueingActions;
        private Func<Context, Task> _onBulkheadRejectedAsync;

        internal AsyncBulkheadPolicy(
            int maxParallelization,
            int maxQueueingActions,
            SemaphoreSlim maxParallelizationSemaphore,
            SemaphoreSlim maxQueuedActionsSemaphore,
            Func<Context, Task> onBulkheadRejectedAsync)
        {
            _maxParallelization = maxParallelization;
            _maxQueueingActions = maxQueueingActions;
            _maxParallelizationSemaphore = maxParallelizationSemaphore;
            _maxQueuedActionsSemaphore = maxQueuedActionsSemaphore;
            _onBulkheadRejectedAsync = onBulkheadRejectedAsync ?? throw new ArgumentNullException(nameof(onBulkheadRejectedAsync));
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return AsyncBulkheadEngine.ImplementationAsync(action, context, _onBulkheadRejectedAsync, _maxParallelizationSemaphore, _maxQueuedActionsSemaphore, cancellationToken, continueOnCapturedContext);
        }

        /// <summary>
        /// Gets the number of slots currently available for executing actions through the bulkhead.
        /// </summary>
        public int BulkheadAvailableCount => _maxParallelizationSemaphore.CurrentCount;

        /// <summary>
        /// Gets the number of slots currently available for queuing actions for execution through the bulkhead.
        /// </summary>
        public int QueueAvailableCount => Math.Min(_maxQueuedActionsSemaphore.CurrentCount, _maxQueueingActions);

        /// <inheritdoc/>
        public void Dispose()
        {
            _maxParallelizationSemaphore.Dispose();
            _maxQueuedActionsSemaphore.Dispose();
        }
    }
}