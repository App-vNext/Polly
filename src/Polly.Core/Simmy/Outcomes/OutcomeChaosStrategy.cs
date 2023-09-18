﻿using Polly.Telemetry;

namespace Polly.Simmy.Outcomes;

#pragma warning disable S3928 // Custom ArgumentNullException message

internal class OutcomeChaosStrategy<T> : MonkeyStrategy<T>
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public OutcomeChaosStrategy(FaultStrategyOptions options, ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        if (options.Fault is null && options.FaultGenerator is null)
        {
            throw new InvalidOperationException("Either Fault or FaultGenerator is required.");
        }

        _telemetry = telemetry;
        Fault = options.Fault;
        OnFaultInjected = options.OnFaultInjected;
        FaultGenerator = options.FaultGenerator is not null ? options.FaultGenerator : (_) => new(options.Fault);
    }

    public OutcomeChaosStrategy(OutcomeStrategyOptions<T> options, ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        if (options.Outcome is null && options.OutcomeGenerator is null)
        {
            throw new InvalidOperationException("Either Outcome or OutcomeGenerator is required.");
        }

        _telemetry = telemetry;
        Outcome = options.Outcome;
        OnOutcomeInjected = options.OnOutcomeInjected;
        OutcomeGenerator = options.OutcomeGenerator is not null ? options.OutcomeGenerator : (_) => new(options.Outcome);
    }

    public Func<OnOutcomeInjectedArguments<T>, ValueTask>? OnOutcomeInjected { get; }

    public Func<OnFaultInjectedArguments, ValueTask>? OnFaultInjected { get; }

    public Func<OutcomeGeneratorArguments, ValueTask<Outcome<T>?>>? OutcomeGenerator { get; }

    public Func<FaultGeneratorArguments, ValueTask<Exception?>>? FaultGenerator { get; }

    public Outcome<T>? Outcome { get; }

    public Exception? Fault { get; }

    protected internal override async ValueTask<Outcome<T>> ExecuteCore<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
    {
        try
        {
            if (await ShouldInjectAsync(context).ConfigureAwait(context.ContinueOnCapturedContext))
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
                    var outcome = await InjectOutcome(context).ConfigureAwait(context.ContinueOnCapturedContext);
                    return new Outcome<T>(outcome.Value.Result);
                }
            }

            return await StrategyHelper.ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (OperationCanceledException e)
        {
            return new Outcome<T>(e);
        }
    }

    private async ValueTask<Outcome<T>?> InjectOutcome(ResilienceContext context)
    {
        var outcome = await OutcomeGenerator!(new(context)).ConfigureAwait(context.ContinueOnCapturedContext);
        var args = new OnOutcomeInjectedArguments<T>(context, outcome.Value);
        _telemetry.Report(new(ResilienceEventSeverity.Information, OutcomeConstants.OnOutcomeInjectedEvent), context, args);

        if (OnOutcomeInjected is not null)
        {
            await OnOutcomeInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        return outcome;
    }

    private async ValueTask<Exception?> InjectFault(ResilienceContext context)
    {
        var fault = await FaultGenerator!(new(context)).ConfigureAwait(context.ContinueOnCapturedContext);
        if (fault is null)
        {
            return null;
        }

        var args = new OnFaultInjectedArguments(context, fault);
        _telemetry.Report(new(ResilienceEventSeverity.Information, OutcomeConstants.OnFaultInjectedEvent), context, args);

        if (OnFaultInjected is not null)
        {
            await OnFaultInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        return fault;
    }
}
