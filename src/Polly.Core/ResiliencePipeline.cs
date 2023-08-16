namespace Polly;

/// <summary>
/// Resilience pipeline is used to execute the user-provided callbacks.
/// </summary>
/// <remarks>
/// Resilience pipeline supports various types of callbacks and provides a unified way to execute them.
/// This includes overloads for synchronous and asynchronous callbacks, generic and non-generic callbacks.
/// </remarks>
public sealed partial class ResiliencePipeline
{
    /// <summary>
    /// Resilience pipeline that executes the user-provided callback without any additional logic.
    /// </summary>
    public static readonly ResiliencePipeline Null = new(PipelineComponent.Null);

    internal ResiliencePipeline(PipelineComponent component) => Component = component;

    internal static ResilienceContextPool Pool => ResilienceContextPool.Shared;

    internal PipelineComponent Component { get; }

    internal ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state) => Component.ExecuteCore(callback, context, state);

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
