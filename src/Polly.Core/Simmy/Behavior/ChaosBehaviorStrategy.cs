using Polly.Telemetry;

namespace Polly.Simmy.Behavior;

internal sealed class ChaosBehaviorStrategy : ChaosStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public ChaosBehaviorStrategy(
        ChaosBehaviorStrategyOptions options,
        ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        _telemetry = telemetry;
        OnBehaviorInjected = options.OnBehaviorInjected;
        Behavior = options.BehaviorGenerator!;
    }

    public Func<OnBehaviorInjectedArguments, ValueTask>? OnBehaviorInjected { get; }

    public Func<BehaviorGeneratorArguments, ValueTask> Behavior { get; }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        try
        {
            if (await ShouldInjectAsync(context).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                await Behavior(new(context)).ConfigureAwait(context.ContinueOnCapturedContext);

                var args = new OnBehaviorInjectedArguments(context);
                _telemetry.Report(new(ResilienceEventSeverity.Information, ChaosBehaviorConstants.OnBehaviorInjectedEvent), context, args);

                if (OnBehaviorInjected is not null)
                {
                    await OnBehaviorInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
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
