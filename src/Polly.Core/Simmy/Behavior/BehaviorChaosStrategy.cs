using Polly.Telemetry;

namespace Polly.Simmy.Behavior;

internal sealed class BehaviorChaosStrategy : ChaosStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public BehaviorChaosStrategy(
        ChaosBehaviorStrategyOptions options,
        ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        _telemetry = telemetry;
        OnBehaviorInjected = options.OnBehaviorInjected;
        Behavior = options.BehaviorAction!;
    }

    public Func<OnBehaviorInjectedArguments, ValueTask>? OnBehaviorInjected { get; }

    public Func<BehaviorActionArguments, ValueTask> Behavior { get; }

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

            return await StrategyHelper.ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (OperationCanceledException e)
        {
            return new Outcome<TResult>(e);
        }
    }
}
