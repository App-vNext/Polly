using Polly.Telemetry;

namespace Polly.Simmy.Latency;

internal sealed class LatencyChaosStrategy : MonkeyStrategy
{
    private readonly RandomUtil _randomUtil;
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;

    public LatencyChaosStrategy(
        LatencyStrategyOptions options,
        TimeProvider timeProvider,
        ResilienceStrategyTelemetry telemetry)
        : base(options.InjectionRate, options.Enabled)
    {
        Guard.NotNull(telemetry);
        Guard.NotNull(timeProvider);
        Guard.NotNull(options.LatencyGenerator);

        OnDelayed = options.OnDelayed;
        LatencyGenerator = options.LatencyGenerator;
        _telemetry = telemetry;
        _timeProvider = timeProvider;
        _randomUtil = options.RandomUtil;
    }

    public Func<OnDelayedArguments, ValueTask>? OnDelayed { get; }

    public Func<ResilienceContext, ValueTask<TimeSpan>> LatencyGenerator { get; }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
    {
        if (await ShouldInject(context, _randomUtil).ConfigureAwait(context.ContinueOnCapturedContext))
        {
            try
            {
                var latency = await LatencyGenerator(context).ConfigureAwait(context.ContinueOnCapturedContext);
                await _timeProvider.DelayAsync(latency, context).ConfigureAwait(context.ContinueOnCapturedContext);

                var args = new OnDelayedArguments(context, latency);
                _telemetry.Report(LatencyConstants.OnDelayedEvent, context, args);

                if (OnDelayed is not null)
                {
                    await OnDelayed(args).ConfigureAwait(context.ContinueOnCapturedContext);
                }
            }
            catch (OperationCanceledException e)
            {
                return new Outcome<TResult>(e);
            }
        }

        return await ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
    }
}
