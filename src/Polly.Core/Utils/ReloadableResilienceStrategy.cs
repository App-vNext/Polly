using Polly.Telemetry;

namespace Polly.Utils;

internal sealed class ReloadableResilienceStrategy<T> : ResilienceStrategy<T>
{
    public const string ReloadFailedEvent = "ReloadFailed";

    public const string OnReloadEvent = "OnReload";

    private readonly Func<CancellationToken> _onReload;
    private readonly Func<ResilienceStrategy<T>> _resilienceStrategyFactory;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private CancellationTokenRegistration _registration;

    public ReloadableResilienceStrategy(
        ResilienceStrategy<T> initialStrategy,
        Func<CancellationToken> onReload,
        Func<ResilienceStrategy<T>> resilienceStrategyFactory,
        ResilienceStrategyTelemetry telemetry)
    {
        Strategy = initialStrategy;

        _onReload = onReload;
        _resilienceStrategyFactory = resilienceStrategyFactory;
        _telemetry = telemetry;

        RegisterOnReload(default);
    }

    public ResilienceStrategy<T> Strategy { get; private set; }

    protected internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        return Strategy.ExecuteCore(callback, context, state);
    }

    private void RegisterOnReload(CancellationToken previousToken)
    {
        var token = _onReload();
        if (token == previousToken)
        {
            return;
        }

        _registration = token.Register(() =>
        {
            var context = ResilienceContextPool.Shared.Get().Initialize<VoidResult>(isSynchronous: true);

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                _telemetry.Report(new(ResilienceEventSeverity.Information, OnReloadEvent), context, new OnReloadArguments());
                Strategy = _resilienceStrategyFactory();
            }
            catch (Exception e)
            {
                var args = new OutcomeArguments<VoidResult, ReloadFailedArguments>(context, Outcome.FromException(e), new ReloadFailedArguments(e));
                _telemetry.Report(new(ResilienceEventSeverity.Error, ReloadFailedEvent), args);
                ResilienceContextPool.Shared.Return(context);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            _registration.Dispose();
            RegisterOnReload(token);
        });
    }

    internal readonly record struct ReloadFailedArguments(Exception Exception);

    internal readonly record struct OnReloadArguments();
}
