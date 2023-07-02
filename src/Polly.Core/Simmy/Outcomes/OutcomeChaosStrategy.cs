using Polly.Telemetry;

namespace Polly.Simmy.Outcomes;

#pragma warning disable S3928 // Custom ArgumentNullException message

internal sealed class OutcomeChaosStrategy<T> : MonkeyStrategy<T>
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public OutcomeChaosStrategy(OutcomeStrategyOptions<T> options,
        ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        Guard.NotNull(telemetry);

        if (!options.Outcome.HasResult && options.OutcomeGenerator is null)
        {
            throw new ArgumentNullException(nameof(options.Outcome), "Either Outcome or OutcomeGenerator is required.");
        }

        _telemetry = telemetry;
        Outcome = options.Outcome;
        OutcomeGenerator = options.Outcome.HasResult ? (_) => new(options.Outcome) : options.OutcomeGenerator;
        OnOutcomeInjected = options.OnOutcomeInjected;
    }

    public Func<OnOutcomeInjectedArguments<T>, ValueTask>? OnOutcomeInjected { get; }

    public Func<ResilienceContext, ValueTask<Outcome<T>>> OutcomeGenerator { get; }

    public Outcome<T> Outcome { get; }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
    {
        try
        {
            if (await ShouldInject(context).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                var outcome = await OutcomeGenerator(context).ConfigureAwait(context.ContinueOnCapturedContext);
                var args = new OnOutcomeInjectedArguments<T>(context, outcome);
                _telemetry.Report(new(ResilienceEventSeverity.Warning, OutcomeConstants.OnOutcomeInjectedEvent), context, args);

                if (OnOutcomeInjected is not null)
                {
                    await OnOutcomeInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
                }

                return outcome.AsOutcome<TResult>();
            }

            return await ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (OperationCanceledException e)
        {
            return new Outcome<TResult>(e);
        }
    }
}
