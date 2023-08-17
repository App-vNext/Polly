namespace Polly.Utils.Pipeline;

/// <summary>
/// Represents a single component of a resilience pipeline.
/// </summary>
/// <remarks>
/// The component of the pipeline can be either a strategy, a generic strategy or a whole pipeline.
/// </remarks>
internal abstract class PipelineComponent : IDisposable, IAsyncDisposable
{
    public static PipelineComponent Null { get; } = new NullComponent();

    internal ResilienceStrategyOptions? Options { get; set; }

    internal abstract ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state);

    internal Outcome<TResult> ExecuteCoreSync<TResult, TState>(
        Func<ResilienceContext, TState, Outcome<TResult>> callback,
        ResilienceContext context,
        TState state)
    {
        return ExecuteCore(
            static (context, state) =>
            {
                var result = state.callback(context, state.state);

                return new ValueTask<Outcome<TResult>>(result);
            },
            context,
            (callback, state)).GetResult();
    }

    public abstract void Dispose();

    public abstract ValueTask DisposeAsync();

    private class NullComponent : PipelineComponent
    {
        internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
            => callback(context, state);

        public override void Dispose()
        {
        }

        public override ValueTask DisposeAsync() => default;
    }
}
