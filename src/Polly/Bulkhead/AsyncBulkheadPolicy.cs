﻿#nullable enable
namespace Polly.Bulkhead;

/// <summary>
/// A bulkhead-isolation policy which can be applied to delegates.
/// </summary>
#pragma warning disable CA1063
public class AsyncBulkheadPolicy : AsyncPolicy, IBulkheadPolicy
#pragma warning restore CA1063
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
    protected override Task<TResult> ImplementationAsync<TResult>(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return AsyncBulkheadEngine.ImplementationAsync(
            action,
            context,
            _onBulkheadRejectedAsync,
            _maxParallelizationSemaphore,
            _maxQueuedActionsSemaphore,
            continueOnCapturedContext,
            cancellationToken);
    }

#pragma warning disable CA1063
    /// <inheritdoc/>
    public void Dispose()
#pragma warning restore CA1063
    {
        _maxParallelizationSemaphore.Dispose();
        _maxQueuedActionsSemaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// A bulkhead-isolation policy which can be applied to delegates.
/// </summary>
/// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
#pragma warning disable CA1063
public class AsyncBulkheadPolicy<TResult> : AsyncPolicy<TResult>, IBulkheadPolicy<TResult>
#pragma warning restore CA1063
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
    protected override Task<TResult> ImplementationAsync(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return AsyncBulkheadEngine.ImplementationAsync(
            action,
            context,
            _onBulkheadRejectedAsync,
            _maxParallelizationSemaphore,
            _maxQueuedActionsSemaphore,
            continueOnCapturedContext,
            cancellationToken);
    }

    /// <summary>
    /// Gets the number of slots currently available for executing actions through the bulkhead.
    /// </summary>
    public int BulkheadAvailableCount => _maxParallelizationSemaphore.CurrentCount;

    /// <summary>
    /// Gets the number of slots currently available for queuing actions for execution through the bulkhead.
    /// </summary>
    public int QueueAvailableCount => Math.Min(_maxQueuedActionsSemaphore.CurrentCount, _maxQueueingActions);

#pragma warning disable CA1063
    /// <inheritdoc/>
    public void Dispose()
#pragma warning restore CA1063
    {
        _maxParallelizationSemaphore.Dispose();
        _maxQueuedActionsSemaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}
