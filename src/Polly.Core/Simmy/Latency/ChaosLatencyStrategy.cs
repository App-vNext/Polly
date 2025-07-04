using Polly.Telemetry;

namespace Polly.Simmy.Latency;

internal sealed class ChaosLatencyStrategy : ChaosStrategy
{
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;

    public ChaosLatencyStrategy(
        ChaosLatencyStrategyOptions options,
        TimeProvider timeProvider,
        ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        Latency = options.Latency;
        LatencyGenerator = options.LatencyGenerator is not null ? options.LatencyGenerator : (_) => new(options.Latency);
        OnLatencyInjected = options.OnLatencyInjected;

        _telemetry = telemetry;
        _timeProvider = timeProvider;
    }

    public Func<OnLatencyInjectedArguments, ValueTask>? OnLatencyInjected { get; }

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
                if (latency > TimeSpan.Zero)
                {
                    var args = new OnLatencyInjectedArguments(context, latency);
                    _telemetry.Report(new(ResilienceEventSeverity.Information, ChaosLatencyConstants.OnLatencyInjectedEvent), context, args);

                    await _timeProvider.DelayAsync(latency, context).ConfigureAwait(context.ContinueOnCapturedContext);

                    if (OnLatencyInjected is not null)
                    {
                        await OnLatencyInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
                    }
                }
            }

            try
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
            }
#pragma warning disable CA1031
            catch (Exception ex)
            {
                return new(ex);
            }
#pragma warning restore CA1031
        }
        catch (OperationCanceledException e)
        {
            return new Outcome<TResult>(e);
        }
    }
}
