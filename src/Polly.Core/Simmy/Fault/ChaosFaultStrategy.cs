using Polly.Telemetry;

namespace Polly.Simmy.Fault;

internal sealed class ChaosFaultStrategy : ChaosStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public ChaosFaultStrategy(ChaosFaultStrategyOptions options, ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        _telemetry = telemetry;

        OnFaultInjected = options.OnFaultInjected;
        FaultGenerator = options.FaultGenerator!;
    }

    public Func<OnFaultInjectedArguments, ValueTask>? OnFaultInjected { get; }

    public Func<FaultGeneratorArguments, ValueTask<Exception?>> FaultGenerator { get; }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        try
        {
            if (await ShouldInjectAsync(context).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                var fault = await FaultGenerator(new(context)).ConfigureAwait(context.ContinueOnCapturedContext);
                if (fault is not null)
                {
                    var args = new OnFaultInjectedArguments(context, fault);
                    _telemetry.Report(new(ResilienceEventSeverity.Information, ChaosFaultConstants.OnFaultInjectedEvent), context, args);

                    if (OnFaultInjected is not null)
                    {
                        await OnFaultInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
                    }

                    return new Outcome<TResult>(fault);
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
