#nullable enable
namespace Polly.Bulkhead;

/// <summary>
/// A bulkhead-isolation policy which can be applied to delegates.
/// </summary>
#pragma warning disable CA1063
public class BulkheadPolicy : Policy, IBulkheadPolicy
#pragma warning restore CA1063
{
    private readonly SemaphoreSlim _maxParallelizationSemaphore;
    private readonly SemaphoreSlim _maxQueuedActionsSemaphore;
    private readonly Action<Context> _onBulkheadRejected;

    internal BulkheadPolicy(
        int maxParallelization,
        int maxQueueingActions,
        Action<Context> onBulkheadRejected)
    {
        MaxQueueingActions = maxQueueingActions;
        _onBulkheadRejected = onBulkheadRejected;

        (_maxParallelizationSemaphore, _maxQueuedActionsSemaphore) = BulkheadSemaphoreFactory.CreateBulkheadSemaphores(maxParallelization, maxQueueingActions);
    }

    private int MaxQueueingActions { get; }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return BulkheadEngine.Implementation(
            action,
            context,
            _onBulkheadRejected,
            _maxParallelizationSemaphore,
            _maxQueuedActionsSemaphore,
            cancellationToken);
    }

    /// <summary>
    /// Gets the number of slots currently available for executing actions through the bulkhead.
    /// </summary>
    public int BulkheadAvailableCount => _maxParallelizationSemaphore.CurrentCount;

    /// <summary>
    /// Gets the number of slots currently available for queuing actions for execution through the bulkhead.
    /// </summary>
    public int QueueAvailableCount => Math.Min(_maxQueuedActionsSemaphore.CurrentCount, MaxQueueingActions);

#pragma warning disable CA1063
    /// <summary>
    /// Disposes of the <see cref="BulkheadPolicy"/>, allowing it to dispose its internal resources.
    /// <remarks>Only call <see cref="Dispose()"/> on a <see cref="BulkheadPolicy"/> after all actions executed through the policy have completed.  If actions are still executing through the policy when <see cref="Dispose()"/> is called, an <see cref="ObjectDisposedException"/> may be thrown on the actions' threads when those actions complete.</remarks>
    /// </summary>
    public void Dispose()
#pragma warning restore CA1063
    {
        _maxParallelizationSemaphore.Dispose();
        _maxQueuedActionsSemaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// A bulkhead-isolation policy which can be applied to delegates returning a value of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
#pragma warning disable CA1063
public class BulkheadPolicy<TResult> : Policy<TResult>, IBulkheadPolicy<TResult>
#pragma warning restore CA1063
{
    private readonly SemaphoreSlim _maxParallelizationSemaphore;
    private readonly SemaphoreSlim _maxQueuedActionsSemaphore;
    private readonly Action<Context> _onBulkheadRejected;

    internal BulkheadPolicy(
        int maxParallelization,
        int maxQueueingActions,
        Action<Context> onBulkheadRejected)
    {
        MaxQueueingActions = maxQueueingActions;
        _onBulkheadRejected = onBulkheadRejected;

        (_maxParallelizationSemaphore, _maxQueuedActionsSemaphore) = BulkheadSemaphoreFactory.CreateBulkheadSemaphores(maxParallelization, maxQueueingActions);
    }

    private int MaxQueueingActions { get; }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return BulkheadEngine.Implementation(
            action,
            context,
            _onBulkheadRejected,
            _maxParallelizationSemaphore,
            _maxQueuedActionsSemaphore, cancellationToken);
    }

    /// <summary>
    /// Gets the number of slots currently available for executing actions through the bulkhead.
    /// </summary>
    public int BulkheadAvailableCount => _maxParallelizationSemaphore.CurrentCount;

    /// <summary>
    /// Gets the number of slots currently available for queuing actions for execution through the bulkhead.
    /// </summary>
    public int QueueAvailableCount => Math.Min(_maxQueuedActionsSemaphore.CurrentCount, MaxQueueingActions);

#pragma warning disable CA1063
    /// <summary>
    /// Disposes of the <see cref="BulkheadPolicy"/>, allowing it to dispose its internal resources.
    /// <remarks>Only call <see cref="Dispose()"/> on a <see cref="BulkheadPolicy"/> after all actions executed through the policy have completed.  If actions are still executing through the policy when <see cref="Dispose()"/> is called, an <see cref="ObjectDisposedException"/> may be thrown on the actions' threads when those actions complete.</remarks>
    /// </summary>
    public void Dispose()
#pragma warning restore CA1063
    {
        _maxParallelizationSemaphore.Dispose();
        _maxQueuedActionsSemaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}
