using Polly.Telemetry;

namespace Polly.Simmy.Outcomes;

#pragma warning disable S3928 // Custom ArgumentNullException message

internal class OutcomeChaosStrategy<T> : OutcomeMonkeyStrategy<T>
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public OutcomeChaosStrategy(OutcomeStrategyOptions<Exception> options, ResilienceStrategyTelemetry telemetry, bool isGeneric)
        : base(isGeneric, options)
    {
        Guard.NotNull(telemetry);

        if (options.Outcome.Exception is null && options.OutcomeGenerator is null)
        {
            throw new ArgumentNullException(nameof(options.Outcome), "Either Outcome or OutcomeGenerator is required.");
        }

        _telemetry = telemetry;
        Fault = options.Outcome;
        OnFaultInjected = options.OnOutcomeInjected;
        FaultGenerator = options.Outcome.Exception is not null ? (_) => new(options.Outcome) : options.OutcomeGenerator;
    }

    public OutcomeChaosStrategy(OutcomeStrategyOptions<T> options, ResilienceStrategyTelemetry telemetry, bool isGeneric)
        : base(isGeneric, options)
    {
        Guard.NotNull(telemetry);

        if (!options.Outcome.HasResult && options.OutcomeGenerator is null)
        {
            throw new ArgumentNullException(nameof(options.Outcome), "Either Outcome or OutcomeGenerator is required.");
        }

        _telemetry = telemetry;
        Outcome = options.Outcome;
        OnOutcomeInjected = options.OnOutcomeInjected;
        OutcomeGenerator = options.Outcome.HasResult ? (_) => new(options.Outcome) : options.OutcomeGenerator;
    }

    public Func<OnOutcomeInjectedArguments<T>, ValueTask>? OnOutcomeInjected { get; }

    public Func<OnOutcomeInjectedArguments<Exception>, ValueTask>? OnFaultInjected { get; }

    public Func<ResilienceContext, ValueTask<Outcome<T>>>? OutcomeGenerator { get; }

    public Func<ResilienceContext, ValueTask<Outcome<Exception>>>? FaultGenerator { get; }

    public Outcome<T>? Outcome { get; }

    public Outcome<Exception>? Fault { get; }

    protected override async ValueTask<Outcome<T>> ExecuteCallbackAsync<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
    {
        try
        {
            if (await ShouldInject(context).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                if (FaultGenerator is not null)
                {
                    var fault = await InjectFault(context).ConfigureAwait(context.ContinueOnCapturedContext);
                    if (fault is not null)
                    {
                        return new Outcome<T>(fault);
                    }
                }
                else if (OutcomeGenerator is not null)
                {
                    return await InjectOutcome(context).ConfigureAwait(context.ContinueOnCapturedContext);
                }
                else
                {
                    throw new InvalidOperationException("Either a fault or fake outcome to inject must be defined.");
                }
            }

            return await ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (OperationCanceledException e)
        {
            return new Outcome<T>(e);
        }
    }

    private async ValueTask<Outcome<T>> InjectOutcome(ResilienceContext context)
    {
        var outcome = await OutcomeGenerator!(context).ConfigureAwait(context.ContinueOnCapturedContext);
        var args = new OnOutcomeInjectedArguments<T>(context, outcome);
        _telemetry.Report(new(ResilienceEventSeverity.Warning, OutcomeConstants.OnOutcomeInjectedEvent), context, args);

        if (OnOutcomeInjected is not null)
        {
            await OnOutcomeInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        return outcome;
    }

    private async ValueTask<Exception?> InjectFault(ResilienceContext context)
    {
        var fault = await FaultGenerator!(context).ConfigureAwait(context.ContinueOnCapturedContext);
        if (fault.Exception is not null)
        {
            var args = new OnOutcomeInjectedArguments<Exception>(context, fault);
            _telemetry.Report(new(ResilienceEventSeverity.Warning, OutcomeConstants.OnFaultInjectedEvent), context, args);

            if (OnFaultInjected is not null)
            {
                await OnFaultInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
            }
        }

        return fault.Exception;
    }
}
