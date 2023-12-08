namespace Polly.Utils.Pipeline;

/// <summary>
/// Represents a single component of a resilience pipeline.
/// </summary>
/// <remarks>
/// The component of the pipeline can be either a strategy, a generic strategy or a whole pipeline.
/// </remarks>
internal abstract class PipelineComponent : IAsyncDisposable
{
    public static PipelineComponent Empty { get; } = new NullComponent();

    internal ResilienceStrategyOptions? Options { get; set; }

    internal abstract ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state);

    internal Outcome<TResult> ExecuteCoreSync<TResult, TState>(
        Func<ResilienceContext, TState, Outcome<TResult>> callback,
        ResilienceContext context,
        TState state)
        => ExecuteCore(
            static (context, state) => new ValueTask<Outcome<TResult>>(state.callbackMethod(context, state.stateObject)),
            context,
            (callbackMethod: callback, stateObject: state))
        .GetResult();

    public abstract ValueTask DisposeAsync();

    private sealed class NullComponent : PipelineComponent
    {
        internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
            => callback(context, state);

        public override ValueTask DisposeAsync() => default;
    }
}
