namespace Polly.Utils.Pipeline;

[DebuggerDisplay("{Component}")]
internal sealed class ExternalComponent : PipelineComponent
{
    public ExternalComponent(PipelineComponent component) => Component = component;

    internal PipelineComponent Component { get; }

    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state) => Component.ExecuteCore(callback, context, state);

    public override ValueTask DisposeAsync() => default; // Don't dispose component that is external
}
