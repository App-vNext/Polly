namespace Polly.Utils.Pipeline;

internal sealed class TracingComponent(
    PipelineComponent component,
    Func<ResilienceContext, IDisposable?> tracerFactory) : PipelineComponent
{
    private readonly PipelineComponent _component = component;
    private readonly Func<ResilienceContext, IDisposable?> _tracerFactory = tracerFactory;

    internal override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        using (_tracerFactory(context))
        {
            return await _component.ExecuteCore(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
    }

    public override async ValueTask DisposeAsync()
        => await _component.DisposeAsync().ConfigureAwait(false);
}
