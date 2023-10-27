using Polly.Telemetry;

namespace Polly.Utils.Pipeline;

#pragma warning disable CA1031 // Do not catch general exception types

internal sealed class ReloadableComponent : PipelineComponent
{
    public const string ReloadFailedEvent = "ReloadFailed";

    public const string DisposeFailedEvent = "DisposeFailed";

    public const string OnReloadEvent = "OnReload";

    private readonly Func<Entry> _factory;
    private ResilienceStrategyTelemetry _telemetry;
    private CancellationTokenSource _tokenSource = null!;
    private CancellationTokenRegistration _registration;
    private List<CancellationToken> _reloadTokens;

    public ReloadableComponent(Entry entry, Func<Entry> factory)
    {
        Component = entry.Component;

        _reloadTokens = entry.ReloadTokens;
        _factory = factory;
        _telemetry = entry.Telemetry;

        TryRegisterOnReload();
    }

    public PipelineComponent Component { get; private set; }

    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        return Component.ExecuteCore(callback, context, state);
    }

    public override ValueTask DisposeAsync()
    {
        DisposeRegistration();
        return Component.DisposeAsync();
    }

    private void TryRegisterOnReload()
    {
        if (_reloadTokens.Count == 0)
        {
            return;
        }

        _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_reloadTokens.ToArray());
        _registration = _tokenSource.Token.Register(() =>
        {
            var context = ResilienceContextPool.Shared.Get().Initialize<VoidResult>(isSynchronous: true);
            var previousComponent = Component;

            try
            {
                _telemetry.Report(new(ResilienceEventSeverity.Information, OnReloadEvent), context, new OnReloadArguments());
                (Component, _reloadTokens, _telemetry) = _factory();
            }
            catch (Exception e)
            {
                _reloadTokens = new List<CancellationToken>();
                _telemetry.Report(new(ResilienceEventSeverity.Error, ReloadFailedEvent), context, Outcome.FromException(e), new ReloadFailedArguments(e));
                ResilienceContextPool.Shared.Return(context);
            }

            DisposeRegistration();
            TryRegisterOnReload();

            _ = DisposeDiscardedComponentSafeAsync(previousComponent);
        });
    }

    private async Task DisposeDiscardedComponentSafeAsync(PipelineComponent component)
    {
        var context = ResilienceContextPool.Shared.Get().Initialize<VoidResult>(isSynchronous: false);

        try
        {
            await component.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _telemetry.Report(new(ResilienceEventSeverity.Error, DisposeFailedEvent), context, Outcome.FromException(e), new DisposedFailedArguments(e));
        }

        ResilienceContextPool.Shared.Return(context);
    }

#pragma warning disable S2952 // Classes should "Dispose" of members from the classes' own "Dispose" methods
    private void DisposeRegistration()
    {
        _registration.Dispose();
        _tokenSource.Dispose();
    }
#pragma warning restore S2952 // Classes should "Dispose" of members from the classes' own "Dispose" methods

    internal record ReloadFailedArguments(Exception Exception);

    internal record DisposedFailedArguments(Exception Exception);

#pragma warning disable S2094 // Classes should not be empty
    internal record OnReloadArguments();
#pragma warning restore S2094 // Classes should not be empty

    internal record Entry(PipelineComponent Component, List<CancellationToken> ReloadTokens, ResilienceStrategyTelemetry Telemetry);
}
