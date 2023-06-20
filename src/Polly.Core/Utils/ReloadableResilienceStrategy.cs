using Polly.Telemetry;

namespace Polly.Utils;

internal sealed class ReloadableResilienceStrategy : ResilienceStrategy
{
    public const string StrategyName = "ReloadableStrategy";

    public const string StrategyType = "ReloadableStrategy";

    public const string ReloadFailedEvent = "ReloadFailed";

    private readonly Func<CancellationToken> _onReload;
    private readonly Func<ResilienceStrategy> _resilienceStrategyFactory;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private CancellationTokenRegistration _registration;

    public ReloadableResilienceStrategy(
        ResilienceStrategy initialStrategy,
        Func<CancellationToken> onReload,
        Func<ResilienceStrategy> resilienceStrategyFactory,
        ResilienceStrategyTelemetry telemetry)
    {
        Strategy = initialStrategy;

        _onReload = onReload;
        _resilienceStrategyFactory = resilienceStrategyFactory;
        _telemetry = telemetry;

        RegisterOnReload(default);
    }

    public ResilienceStrategy Strategy { get; private set; }

    protected internal override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        return Strategy.ExecuteCoreAsync(callback, context, state);
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
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                Strategy = _resilienceStrategyFactory();
            }
            catch (Exception e)
            {
                var context = ResilienceContext.Get().Initialize<VoidResult>(isSynchronous: true);
                var args = new OutcomeArguments<VoidResult, ReloadFailedArguments>(context, Outcome.FromException(e), new ReloadFailedArguments(e));
                _telemetry.Report(ReloadFailedEvent, args);
                ResilienceContext.Return(context);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            _registration.Dispose();
            RegisterOnReload(token);
        });
    }

    internal readonly record struct ReloadFailedArguments(Exception Exception);
}
