#nullable enable

namespace Polly.Bulkhead;

#pragma warning disable CA1062 // Validate arguments of public methods // Temporary stub

/// <summary>
/// A bulkhead-isolation policy which can be applied to delegates.
/// </summary>
public class AsyncBulkheadPolicy : AsyncPolicy, IBulkheadPolicy
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
        _onBulkheadRejectedAsync = onBulkheadRejectedAsync ?? throw new ArgumentNullException(nameof(onBulkheadRejectedAsync));

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
    protected override Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        AsyncBulkheadEngine.ImplementationAsync(action, context, _onBulkheadRejectedAsync, _maxParallelizationSemaphore, _maxQueuedActionsSemaphore, cancellationToken, continueOnCapturedContext);

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
    private readonly int _maxQueueingActions;
    private readonly Func<Context, Task> _onBulkheadRejectedAsync;

    internal AsyncBulkheadPolicy(
        int maxParallelization,
        int maxQueueingActions,
        Func<Context, Task> onBulkheadRejectedAsync)
    {
        _maxQueueingActions = maxQueueingActions;
        _onBulkheadRejectedAsync = onBulkheadRejectedAsync ?? throw new ArgumentNullException(nameof(onBulkheadRejectedAsync));

        (_maxParallelizationSemaphore, _maxQueuedActionsSemaphore) = BulkheadSemaphoreFactory.CreateBulkheadSemaphores(maxParallelization, maxQueueingActions);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        AsyncBulkheadEngine.ImplementationAsync(action, context, _onBulkheadRejectedAsync, _maxParallelizationSemaphore, _maxQueuedActionsSemaphore, cancellationToken, continueOnCapturedContext);

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
