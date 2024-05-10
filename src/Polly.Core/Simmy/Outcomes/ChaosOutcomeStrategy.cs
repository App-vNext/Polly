﻿using Polly.Telemetry;

namespace Polly.Simmy.Outcomes;

internal class ChaosOutcomeStrategy<T> : ChaosStrategy<T>
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly Func<OnOutcomeInjectedArguments<T>, ValueTask>? _onOutcomeInjected;
    private readonly Func<OutcomeGeneratorArguments, ValueTask<Outcome<T>?>> _outcomeGenerator;

    public ChaosOutcomeStrategy(ChaosOutcomeStrategyOptions<T> options, ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        _telemetry = telemetry;
        _onOutcomeInjected = options.OnOutcomeInjected;
        _outcomeGenerator = options.OutcomeGenerator!;
    }

    protected internal override async ValueTask<Outcome<T>> ExecuteCore<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
    {
        try
        {
            if (await ShouldInjectAsync(context).ConfigureAwait(context.ContinueOnCapturedContext)
                && await _outcomeGenerator(new(context)).ConfigureAwait(context.ContinueOnCapturedContext) is Outcome<T> outcome)
            {
                var args = new OnOutcomeInjectedArguments<T>(context, outcome);
                _telemetry.Report(new(ResilienceEventSeverity.Information, ChaosOutcomeConstants.OnOutcomeInjectedEvent), context, args);

                if (_onOutcomeInjected is not null)
                {
                    await _onOutcomeInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
                }

                if (outcome.HasResult)
                {
                    return new Outcome<T>(outcome.Result);
                }

                return new Outcome<T>(outcome.Exception!);
            }

            return await StrategyHelper.ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (OperationCanceledException e)
        {
            return new Outcome<T>(e);
        }
    }
}
