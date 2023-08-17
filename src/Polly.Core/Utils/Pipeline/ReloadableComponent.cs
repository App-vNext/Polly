using Polly.Telemetry;

namespace Polly.Utils.Pipeline;

#pragma warning disable CA1031 // Do not catch general exception types

internal sealed class ReloadableComponent : PipelineComponent
{
    public const string ReloadFailedEvent = "ReloadFailed";

    public const string OnReloadEvent = "OnReload";

    private readonly Func<CancellationToken> _onReload;
    private readonly Func<PipelineComponent> _factory;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private CancellationTokenRegistration _registration;

    public ReloadableComponent(
        PipelineComponent initialComponent,
        Func<CancellationToken> onReload,
        Func<PipelineComponent> factory,
        ResilienceStrategyTelemetry telemetry)
    {
        Component = initialComponent;

        _onReload = onReload;
        _factory = factory;
        _telemetry = telemetry;

        RegisterOnReload(default);
    }

    public PipelineComponent Component { get; private set; }

    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        return Component.ExecuteCore(callback, context, state);
    }

    public override void Dispose()
    {
        DisposeRegistration();
        Component.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        DisposeRegistration();
        return Component.DisposeAsync();
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
            PipelineComponent previousComponent = Component;

            try
            {
                _telemetry.Report(new(ResilienceEventSeverity.Information, OnReloadEvent), context, new OnReloadArguments());
                Component = _factory();

                previousComponent.Dispose();
            }
            catch (Exception e)
            {
                var args = new OutcomeArguments<VoidResult, ReloadFailedArguments>(context, Outcome.FromException(e), new ReloadFailedArguments(e));
                _telemetry.Report(new(ResilienceEventSeverity.Error, ReloadFailedEvent), args);
                ResilienceContextPool.Shared.Return(context);
            }

            DisposeRegistration();
            RegisterOnReload(token);
        });
    }

#pragma warning disable S2952 // Classes should "Dispose" of members from the classes' own "Dispose" methods
    private void DisposeRegistration() => _registration.Dispose();
#pragma warning restore S2952 // Classes should "Dispose" of members from the classes' own "Dispose" methods

    internal readonly record struct ReloadFailedArguments(Exception Exception);

    internal readonly record struct OnReloadArguments();
}
