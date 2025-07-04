namespace Polly.Utils.Pipeline;

internal sealed class ExecutionTrackingComponent : PipelineComponent
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    public static readonly TimeSpan SleepDelay = TimeSpan.FromSeconds(1);

    private readonly TimeProvider _timeProvider;
    private int _pendingExecutions;

    public ExecutionTrackingComponent(PipelineComponent component, TimeProvider timeProvider)
    {
        Component = component;
        _timeProvider = timeProvider;
    }

    public PipelineComponent Component { get; }

    public bool HasPendingExecutions => Volatile.Read(ref _pendingExecutions) > 0;

    internal override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        Interlocked.Increment(ref _pendingExecutions);

        try
        {
            return await Component.ExecuteCore(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        finally
        {
            Interlocked.Decrement(ref _pendingExecutions);
        }
    }

    public override async ValueTask DisposeAsync()
    {
        var start = _timeProvider.GetTimestamp();

        // We don't want to introduce locks or any synchronization primitives to main execution path
        // so we will do "dummy" retries until there are no more executions.
        while (HasPendingExecutions)
        {
#if NET8_0_OR_GREATER
            await Task.Delay(SleepDelay, _timeProvider).ConfigureAwait(false);
#else
            await _timeProvider.Delay(SleepDelay).ConfigureAwait(false);
#endif

            // stryker disable once equality : no means to test this
            if (_timeProvider.GetElapsedTime(start) > Timeout)
            {
                break;
            }
        }

        await Component.DisposeAsync().ConfigureAwait(false);
    }
}
