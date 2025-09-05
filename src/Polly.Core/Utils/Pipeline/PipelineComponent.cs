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

    internal TResult ExecuteCoreSync<TResult, TState>(
        Func<ResilienceContext, TState, TResult> callback,
        ResilienceContext context,
        TState state)
        => ExecuteCore([DebuggerDisableUserUnhandledExceptions] static (context, state) =>
        {
            try
            {
                return new ValueTask<Outcome<TResult>>(new Outcome<TResult>(state.callback(context, state.state)));
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031
            {
                return new ValueTask<Outcome<TResult>>(new Outcome<TResult>(e));
            }
        },
        context, (callback, state)).GetResult().GetResultOrRethrow();

    public abstract ValueTask DisposeAsync();

    private sealed class NullComponent : PipelineComponent
    {
        internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
            => callback(context, state);

        public override ValueTask DisposeAsync() => default;
    }
}
