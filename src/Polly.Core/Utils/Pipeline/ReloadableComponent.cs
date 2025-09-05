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
    private CancellationTokenSource? _tokenSource;

    public ReloadableComponent(Entry entry, Func<Entry> factory)
    {
        Component = entry.Component;

        _factory = factory;
        _telemetry = entry.Telemetry;

        TryRegisterOnReload(entry.ReloadTokens);
    }

    public PipelineComponent Component { get; private set; }

    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state) => Component.ExecuteCore(callback, context, state);

    public override ValueTask DisposeAsync()
    {
        _tokenSource?.Dispose();
        return Component.DisposeAsync();
    }

    private void TryRegisterOnReload(List<CancellationToken> reloadTokens)
    {
        if (reloadTokens.Count == 0)
        {
            return;
        }

        _tokenSource = CancellationTokenSource.CreateLinkedTokenSource([.. reloadTokens]);
#if NET
        _ = _tokenSource.Token.UnsafeRegister(static s => ((ReloadableComponent)s!).Reload(), this);
#else
        _ = _tokenSource.Token.Register(static s => ((ReloadableComponent)s!).Reload(), this);
#endif
    }

    private void Reload()
    {
        _tokenSource!.Dispose();
        _tokenSource = null;

        var context = ResilienceContextPool.Shared.Get().Initialize<VoidResult>(isSynchronous: true);
        _telemetry.Report(new(ResilienceEventSeverity.Information, OnReloadEvent), context, new OnReloadArguments());
        ResilienceContextPool.Shared.Return(context);

        var previousComponent = Component;
        List<CancellationToken> reloadTokens;
        try
        {
            (Component, reloadTokens, _telemetry) = _factory();
        }
        catch (Exception e)
        {
            context = new ResilienceContext().Initialize<VoidResult>(isSynchronous: true);
            _telemetry.Report(new(ResilienceEventSeverity.Error, ReloadFailedEvent), context, Outcome.FromException(e), new ReloadFailedArguments(e));
            return;
        }

        TryRegisterOnReload(reloadTokens);
        _ = DisposeDiscardedComponentSafeAsync(previousComponent);
    }

    private async Task DisposeDiscardedComponentSafeAsync(PipelineComponent component)
    {
        try
        {
            await component.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            var context = new ResilienceContext().Initialize<VoidResult>(isSynchronous: false);
            _telemetry.Report(new(ResilienceEventSeverity.Error, DisposeFailedEvent), context, Outcome.FromException(e), new DisposedFailedArguments(e));
        }
    }

    internal sealed record ReloadFailedArguments(Exception Exception);

    internal sealed record DisposedFailedArguments(Exception Exception);

#pragma warning disable S2094 // Classes should not be empty
#pragma warning disable S3253 // Constructor and destructor declarations should not be redundant
    internal sealed record OnReloadArguments();
#pragma warning restore S3253 // Constructor and destructor declarations should not be redundant
#pragma warning restore S2094 // Classes should not be empty

    internal sealed record Entry(PipelineComponent Component, List<CancellationToken> ReloadTokens, ResilienceStrategyTelemetry Telemetry);
}
