namespace Polly;

/// <summary>
/// Base class for all proactive resilience strategies.
/// </summary>
public abstract class ResilienceStrategy
{
    /// <summary>
    /// An implementation of a proactive resilience strategy that executes the specified <paramref name="callback"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>
    /// An instance of a pending <see cref="ValueTask"/> for asynchronous executions or a completed <see cref="ValueTask"/> task for synchronous executions.
    /// </returns>
    /// <remarks>
    /// The provided callback never throws an exception. Instead, the exception is captured and converted to an <see cref="Outcome{TResult}"/>.
    /// Similarly, do not throw exceptions from your strategy implementation. Instead, return an exception instance as <see cref="Outcome{TResult}"/>.
    /// </remarks>
    protected internal abstract ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state);
}
