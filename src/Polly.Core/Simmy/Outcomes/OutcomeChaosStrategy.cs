using Polly.Telemetry;

namespace Polly.Simmy.Outcomes;

#pragma warning disable S3928 // Custom ArgumentNullException message

internal class OutcomeChaosStrategy<T> : MonkeyStrategy<T>
{
    private readonly ResilienceStrategyTelemetry _telemetry;

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

    public Func<OutcomeGeneratorArguments, ValueTask<Outcome<T>?>>? OutcomeGenerator { get; }

    public Outcome<T>? Outcome { get; }

    protected internal override async ValueTask<Outcome<T>> ExecuteCore<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
    {
        try
        {
            if (await ShouldInjectAsync(context).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                var outcome = await OutcomeGenerator!(new(context)).ConfigureAwait(context.ContinueOnCapturedContext);
                var args = new OnOutcomeInjectedArguments<T>(context, outcome.Value);
                _telemetry.Report(new(ResilienceEventSeverity.Information, OutcomeConstants.OnOutcomeInjectedEvent), context, args);

                if (OnOutcomeInjected is not null)
                {
                    await OnOutcomeInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
                }

                return new Outcome<T>(outcome.Value.Result);
            }

            return await StrategyHelper.ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (OperationCanceledException e)
        {
            return new Outcome<T>(e);
        }
    }
}
