namespace Polly.Utils.Pipeline;

internal sealed class ExecutionTrackingComponent : PipelineComponent
{
    public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    public static readonly TimeSpan SleepDelay = TimeSpan.FromSeconds(1);

    private readonly TimeProvider _timeProvider;
    private int _pendingExecutions;

    public ExecutionTrackingComponent(PipelineComponent component, TimeProvider timeProvider)
    {
        Component = component;
        _timeProvider = timeProvider;
    }

    public PipelineComponent Component { get; }

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
        var stopwatch = Stopwatch.StartNew();

        // We don't want to introduce locks or any synchronization primitives to main execution path
        // so we will do "dummy" retries until there are no more executions.
        while (Interlocked.CompareExchange(ref _pendingExecutions, 0, 0) != 0)
        {
            await _timeProvider.Delay(SleepDelay).ConfigureAwait(false);

            if (_timeProvider.GetElapsedTime(start) > Timeout)
            {
                break;
            }
        }

        await Component.DisposeAsync().ConfigureAwait(false);
    }
}
