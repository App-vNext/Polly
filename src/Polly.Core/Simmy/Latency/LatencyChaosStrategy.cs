using Polly.Telemetry;

namespace Polly.Simmy.Latency;

internal sealed class LatencyChaosStrategy : MonkeyStrategy
{
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;

    public LatencyChaosStrategy(
        LatencyStrategyOptions options,
        TimeProvider timeProvider,
        ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        Guard.NotNull(telemetry);
        Guard.NotNull(timeProvider);
        Guard.NotNull(options.LatencyGenerator);

        OnDelayed = options.OnDelayed;
        Latency = options.Latency;
        LatencyGenerator = options.Latency.HasValue ? (_) => new(options.Latency.Value) : options.LatencyGenerator;

        _telemetry = telemetry;
        _timeProvider = timeProvider;
    }

    public Func<OnDelayedArguments, ValueTask>? OnDelayed { get; }

    public Func<ResilienceContext, ValueTask<TimeSpan>> LatencyGenerator { get; }

    public TimeSpan? Latency { get; }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
    {
        try
        {
            if (await ShouldInject(context).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                var latency = await LatencyGenerator(context).ConfigureAwait(context.ContinueOnCapturedContext);
                await _timeProvider.DelayAsync(latency, context).ConfigureAwait(context.ContinueOnCapturedContext);

                var args = new OnDelayedArguments(context, latency);
                _telemetry.Report(new(ResilienceEventSeverity.Warning, LatencyConstants.OnDelayedEvent), context, args);

                if (OnDelayed is not null)
                {
                    await OnDelayed(args).ConfigureAwait(context.ContinueOnCapturedContext);
                }
            }

            return await ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (OperationCanceledException e)
        {
            return new Outcome<TResult>(e);
        }
    }
}
