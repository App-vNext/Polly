namespace Polly.Utils;

[DebuggerDisplay("{Strategy}")]
internal sealed class ResiliencePipelineBridge : ResiliencePipeline
{
    public ResiliencePipelineBridge(ResilienceStrategy strategy) => Strategy = strategy;

    public ResilienceStrategy Strategy { get; }

    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state) => Strategy.ExecuteCore(callback, context, state);
}
