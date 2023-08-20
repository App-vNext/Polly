namespace Polly;

/// <summary>
/// Resilience strategy is used to execute the user-provided callbacks.
/// </summary>
/// <remarks>
/// Resilience strategy supports various types of callbacks and provides a unified way to execute them.
/// This includes overloads for synchronous and asynchronous callbacks, generic and non-generic callbacks.
/// </remarks>
public partial class ResilienceStrategy
{
    internal static ResilienceContextPool Pool => ResilienceContextPool.Shared;

    internal ResilienceStrategyOptions? Options { get; set; }

    internal ResilienceStrategy()
    {
    }

    internal abstract ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state);

    private Outcome<TResult> ExecuteCoreSync<TResult, TState>(
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
}
