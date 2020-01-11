using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Bulkhead
{
    /// <summary>
    /// A bulkhead-isolation policy which can be applied to asynchronous executions.
    /// </summary>
    public class AsyncBulkheadPolicy : AsyncPolicyV8, IAsyncBulkheadPolicy
    {
        private readonly SemaphoreSlim _maxParallelizationSemaphore;
        private readonly SemaphoreSlim _maxQueuedActionsSemaphore;
        private readonly int _maxQueueingActions;
        private readonly Func<Context, Task> _onBulkheadRejectedAsync;

        internal AsyncBulkheadPolicy(
            int maxParallelization,
            int maxQueueingActions,
            Func<Context, Task> onBulkheadRejectedAsync)
        {
            _maxQueueingActions = maxQueueingActions;
            _onBulkheadRejectedAsync = onBulkheadRejectedAsync;

            (_maxParallelizationSemaphore, _maxQueuedActionsSemaphore) = BulkheadSemaphoreFactory.CreateBulkheadSemaphores(maxParallelization, maxQueueingActions);
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
        protected override Task<TResult> AsyncGenericImplementationV8<TExecutableAsync, TResult>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return AsyncBulkheadEngineV8.ImplementationAsync<TExecutableAsync, TResult>(action, context, _onBulkheadRejectedAsync, _maxParallelizationSemaphore, _maxQueuedActionsSemaphore, cancellationToken, continueOnCapturedContext);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _maxParallelizationSemaphore.Dispose();
            _maxQueuedActionsSemaphore.Dispose();
        }
    }

    /// <summary>
    /// A bulkhead-isolation policy which can be applied to asynchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class AsyncBulkheadPolicy<TResult> : AsyncPolicyV8<TResult>, IAsyncBulkheadPolicy<TResult>
    {
        private readonly SemaphoreSlim _maxParallelizationSemaphore;
        private readonly SemaphoreSlim _maxQueuedActionsSemaphore;
        private readonly int _maxQueueingActions;
        private readonly Func<Context, Task> _onBulkheadRejectedAsync;

        internal AsyncBulkheadPolicy(
            int maxParallelization,
            int maxQueueingActions,
            Func<Context, Task> onBulkheadRejectedAsync)
        {
            _maxQueueingActions = maxQueueingActions;
            _onBulkheadRejectedAsync = onBulkheadRejectedAsync;

            (_maxParallelizationSemaphore, _maxQueuedActionsSemaphore) = BulkheadSemaphoreFactory.CreateBulkheadSemaphores(maxParallelization, maxQueueingActions);
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> AsyncGenericImplementationV8<TExecutableAsync>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return AsyncBulkheadEngineV8.ImplementationAsync<TExecutableAsync, TResult>(action, context, _onBulkheadRejectedAsync, _maxParallelizationSemaphore, _maxQueuedActionsSemaphore, cancellationToken, continueOnCapturedContext);
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