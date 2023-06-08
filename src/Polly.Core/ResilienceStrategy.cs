namespace Polly;

/// <summary>
/// Resilience strategy is used to execute the user-provided callbacks.
/// </summary>
/// <remarks>
/// Resilience strategy supports various types of callbacks and provides a unified way to execute them.
/// This includes overloads for synchronous and asynchronous callbacks, generic and non-generic callbacks.
/// </remarks>
public abstract partial class ResilienceStrategy
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents an asynchronous callback.</returns>
    /// <remarks>
    /// This method is called by various methods exposed on <see cref="ResilienceStrategy"/>. These methods make sure that
    /// <paramref name="context"/> is properly initialized with details about the execution mode.
    /// <para>
    /// The provided callback never throws an exception. Instead, the exception is captured and converted to an <see cref="Outcome{TResult}"/>.
    /// </para>
    /// <para>
    /// Do not throw exceptions from your strategy implementation. Instead, return an exception instance as <see cref="Outcome{TResult}"/>.
    /// </para>
    /// </remarks>
    protected internal abstract ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state);

    private Outcome<TResult> ExecuteCoreSync<TResult, TState>(
        Func<ResilienceContext, TState, Outcome<TResult>> callback,
        ResilienceContext context,
        TState state)
    {
        return ExecuteCoreAsync(
            static (context, state) =>
            {
                var result = state.callback(context, state.state);

                return new ValueTask<Outcome<TResult>>(result);
            },
            context,
            (callback, state)).GetResult();
    }
}
