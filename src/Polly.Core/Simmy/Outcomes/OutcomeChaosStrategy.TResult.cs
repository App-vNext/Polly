using Polly.Telemetry;

namespace Polly.Simmy.Outcomes;

#pragma warning disable S3928 // Custom ArgumentNullException message

internal class OutcomeChaosStrategy<T> : ReactiveMonkeyStrategy<T>
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public OutcomeChaosStrategy(OutcomeStrategyOptions<Exception> options, ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        if (options.Outcome is null && options.OutcomeGenerator is null)
        {
            throw new ArgumentNullException(nameof(options.Outcome), "Either Outcome or OutcomeGenerator is required.");
        }

        _telemetry = telemetry;
        Fault = options.Outcome;
        OnFaultInjected = options.OnOutcomeInjected;
        FaultGenerator = options.OutcomeGenerator is not null ? options.OutcomeGenerator : (_) => new(options.Outcome);
    }

    public OutcomeChaosStrategy(OutcomeStrategyOptions<T> options, ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        if (options.Outcome is null && options.OutcomeGenerator is null)
        {
            throw new ArgumentNullException(nameof(options.Outcome), "Either Outcome or OutcomeGenerator is required.");
        }

        _telemetry = telemetry;
        Outcome = options.Outcome;
        OnOutcomeInjected = options.OnOutcomeInjected;
        OutcomeGenerator = options.OutcomeGenerator is not null ? options.OutcomeGenerator : (_) => new(options.Outcome);
    }

    public Func<OutcomeArguments<T, OutcomeGeneratorArguments>, ValueTask>? OnOutcomeInjected { get; }

    public Func<OutcomeArguments<Exception, OutcomeGeneratorArguments>, ValueTask>? OnFaultInjected { get; }

    public Func<OutcomeGeneratorArguments, ValueTask<Outcome<T>?>>? OutcomeGenerator { get; }

    public Func<OutcomeGeneratorArguments, ValueTask<Outcome<Exception>?>>? FaultGenerator { get; }

    public Outcome<T>? Outcome { get; private set; }

    public Outcome<Exception>? Fault { get; }

    protected override async ValueTask<Outcome<T>> ExecuteCore<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
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
                    if (outcome.HasValue)
                    {
                        return new Outcome<T>(outcome.Value.Result);
                    }
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

    private async ValueTask<Outcome<T>?> InjectOutcome(ResilienceContext context)
    {
        var outcomeGeneratorArgs = new OutcomeGeneratorArguments(context);
        var outcome = await OutcomeGenerator!(outcomeGeneratorArgs).ConfigureAwait(context.ContinueOnCapturedContext);
        var args = new OutcomeArguments<T, OutcomeGeneratorArguments>(context, outcome.Value, outcomeGeneratorArgs);
        _telemetry.Report(new(ResilienceEventSeverity.Warning, OutcomeConstants.OnOutcomeInjectedEvent), context, args);

        if (OnOutcomeInjected is not null)
        {
            await OnOutcomeInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        return outcome;
    }

    private async ValueTask<Exception?> InjectFault(ResilienceContext context)
    {
        try
        {
            var outcomeGeneratorArgs = new OutcomeGeneratorArguments(context);
            var fault = await FaultGenerator!(outcomeGeneratorArgs).ConfigureAwait(context.ContinueOnCapturedContext);
            if (!fault.HasValue)
            {
                return null;
            }

            // to prevent injecting the fault if it was cancelled while executing the FaultGenerator
            context.CancellationToken.ThrowIfCancellationRequested();

            Outcome = new(fault.Value.Exception!);
            var args = new OutcomeArguments<Exception, OutcomeGeneratorArguments>(context, new Outcome<Exception>(fault.Value.Exception!), outcomeGeneratorArgs);
            _telemetry.Report(new(ResilienceEventSeverity.Warning, OutcomeConstants.OnFaultInjectedEvent), context, args);

            if (OnFaultInjected is not null)
            {
                await OnFaultInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
            }

            return fault.Value.Exception;
        }
        catch (OperationCanceledException)
        {
            // fault injection might be cancelled during FaultGenerator, if so we run the user's delegate normally
            context.CancellationToken = CancellationToken.None;
            return null;
        }
    }
}
