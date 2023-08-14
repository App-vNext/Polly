using Polly.Telemetry;

namespace Polly.Utils;

internal sealed class ReloadableResiliencePipeline : ResiliencePipeline
{
    public const string ReloadFailedEvent = "ReloadFailed";

    public const string OnReloadEvent = "OnReload";

    private readonly Func<CancellationToken> _onReload;
    private readonly Func<ResiliencePipeline> _resiliencePipelineFactory;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private CancellationTokenRegistration _registration;

    public ReloadableResiliencePipeline(
        ResiliencePipeline initialPipeline,
        Func<CancellationToken> onReload,
        Func<ResiliencePipeline> resiliencePipelineFactory,
        ResilienceStrategyTelemetry telemetry)
    {
        Pipeline = initialPipeline;

        _onReload = onReload;
        _resiliencePipelineFactory = resiliencePipelineFactory;
        _telemetry = telemetry;

        RegisterOnReload(default);
    }

    public ResiliencePipeline Pipeline { get; private set; }

    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        return Pipeline.ExecuteCore(callback, context, state);
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
                Pipeline = _resiliencePipelineFactory();
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
