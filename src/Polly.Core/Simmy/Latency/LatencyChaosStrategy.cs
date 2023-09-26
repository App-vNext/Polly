using Polly.Telemetry;

namespace Polly.Simmy.Latency;

#pragma warning disable S3928 // Custom ArgumentNullException message

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
        Latency = options.Latency;
        LatencyGenerator = options.LatencyGenerator is not null ? options.LatencyGenerator : (_) => new(options.Latency);
        OnLatency = options.OnLatency;

        _telemetry = telemetry;
        _timeProvider = timeProvider;
    }

    public Func<OnLatencyArguments, ValueTask>? OnLatency { get; }

    public Func<LatencyGeneratorArguments, ValueTask<TimeSpan>> LatencyGenerator { get; }

    public TimeSpan? Latency { get; }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        try
        {
            if (await ShouldInjectAsync(context).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                var latency = await LatencyGenerator(new(context)).ConfigureAwait(context.ContinueOnCapturedContext);
                if (latency <= TimeSpan.Zero)
                {
                    // do nothing
                    return await StrategyHelper.ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
                }

                var args = new OnLatencyArguments(context, latency);
                _telemetry.Report(new(ResilienceEventSeverity.Information, LatencyConstants.OnLatencyEvent), context, args);

                await _timeProvider.DelayAsync(latency, context).ConfigureAwait(context.ContinueOnCapturedContext);

                if (OnLatency is not null)
                {
                    await OnLatency(args).ConfigureAwait(context.ContinueOnCapturedContext);
                }
            }

            return await StrategyHelper.ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (OperationCanceledException e)
        {
            return new Outcome<TResult>(e);
        }
    }
}
