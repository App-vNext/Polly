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
    /// </remarks>
    protected internal abstract ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<TResult>> callback,
        ResilienceContext context, TState state);
}
