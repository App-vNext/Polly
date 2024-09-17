namespace Polly.Utils.Pipeline;

internal sealed class ComponentWithDisposeCallbacks : PipelineComponent
{
    private readonly List<Action> _callbacks;

    public ComponentWithDisposeCallbacks(PipelineComponent component, List<Action> callbacks)
    {
        Component = component;
        _callbacks = callbacks;
    }

    internal PipelineComponent Component { get; }

    public override ValueTask DisposeAsync()
    {
        ExecuteCallbacks();

        return Component.DisposeAsync();
    }

    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state) => Component.ExecuteCore(callback, context, state);

    private void ExecuteCallbacks()
    {
        foreach (var callback in _callbacks)
        {
            callback();
        }

        _callbacks.Clear();
    }
}
