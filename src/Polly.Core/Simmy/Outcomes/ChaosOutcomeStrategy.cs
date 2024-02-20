using Polly.Telemetry;

namespace Polly.Simmy.Outcomes;

internal class ChaosOutcomeStrategy<T> : ChaosStrategy<T>
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public ChaosOutcomeStrategy(ChaosOutcomeStrategyOptions<T> options, ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        _telemetry = telemetry;
        OnOutcomeInjected = options.OnOutcomeInjected;
        OutcomeGenerator = options.OutcomeGenerator!;
    }

    public Func<OnOutcomeInjectedArguments<T>, ValueTask>? OnOutcomeInjected { get; }

    public Func<OutcomeGeneratorArguments, ValueTask<Outcome<T>>> OutcomeGenerator { get; }

    protected internal override async ValueTask<Outcome<T>> ExecuteCore<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
    {
        try
        {
            if (await ShouldInjectAsync(context).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                var outcome = await OutcomeGenerator(new(context)).ConfigureAwait(context.ContinueOnCapturedContext);
                var args = new OnOutcomeInjectedArguments<T>(context, outcome);
                _telemetry.Report(new(ResilienceEventSeverity.Information, ChaosOutcomeConstants.OnOutcomeInjectedEvent), context, args);

                if (OnOutcomeInjected is not null)
                {
                    await OnOutcomeInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
                }

                return new Outcome<T>(outcome.Result);
            }

            return await StrategyHelper.ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (OperationCanceledException e)
        {
            return new Outcome<T>(e);
        }
    }
}
