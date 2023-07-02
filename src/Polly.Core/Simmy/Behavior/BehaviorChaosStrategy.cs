using Polly.Telemetry;

namespace Polly.Simmy.Behavior;

internal sealed class BehaviorChaosStrategy : MonkeyStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public BehaviorChaosStrategy(
        BehaviorStrategyOptions options,
        ResilienceStrategyTelemetry telemetry)
        : base(options)
    {
        Guard.NotNull(telemetry);
        Guard.NotNull(options.Behavior);

        _telemetry = telemetry;
        OnBehaviorInjected = options.OnBehaviorInjected;
        Behavior = options.Behavior;
    }

    public Func<OnBehaviorInjectedArguments, ValueTask>? OnBehaviorInjected { get; }

    public Func<ResilienceContext, ValueTask> Behavior { get; }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
    {
        try
        {
            if (await ShouldInject(context).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                await Behavior(context).ConfigureAwait(context.ContinueOnCapturedContext);

                var args = new OnBehaviorInjectedArguments(context);
                _telemetry.Report(new(ResilienceEventSeverity.Warning, BehaviorConstants.OnBehaviorInjectedEvent), context, args);

                if (OnBehaviorInjected is not null)
                {
                    await OnBehaviorInjected(args).ConfigureAwait(context.ContinueOnCapturedContext);
                }
            }

            return await ExecuteCallbackSafeAsync(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        catch (OperationCanceledException e)
        {
            return new Outcome<TResult>(e);
        }
    }
}
