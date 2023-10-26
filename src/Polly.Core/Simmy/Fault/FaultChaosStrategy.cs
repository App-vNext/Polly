﻿using Polly.Telemetry;

namespace Polly.Simmy.Fault;

internal class FaultChaosStrategy : MonkeyStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public FaultChaosStrategy(FaultStrategyOptions options, ResilienceStrategyTelemetry telemetry)
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

    public Func<OnFaultInjectedArguments, ValueTask>? OnFaultInjected { get; }

    public Func<FaultGeneratorArguments, ValueTask<Exception?>> FaultGenerator { get; }

    public Exception? Fault { get; }

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
                    _telemetry.Report(new(ResilienceEventSeverity.Information, FaultConstants.OnFaultInjectedEvent), context, args);

                    if (OnFaultInjected is not null)
                    {
                        await OnFaultInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
                    }

                    return new Outcome<TResult>(fault);
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
