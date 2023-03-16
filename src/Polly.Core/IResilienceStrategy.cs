namespace Polly;

/// <summary>
/// A resilience strategy that is used to execute the user-provided callbacks.
/// </summary>
/// <remarks>
/// You do not execute any methods on this interface directly. Instead, use the dedicated extensions defined in <see cref="ResilienceStrategyExtensions"/> to execute your callbacks.
/// </remarks>
public interface IResilienceStrategy
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the execution callback.</typeparam>
    /// <typeparam name="TState">The type of state associated with the execution.</typeparam>
    /// <param name="execution">The execution callback.</param>
    /// <param name="context">The context associated with the execution.</param>
    /// <param name="state">The state associated with the execution.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents an asynchronous execution.</returns>
    /// <remarks>This method should not be used directly. Instead, use the dedicated extensions to execute user-provided callback.</remarks>
    internal ValueTask<TResult> ExecuteInternalAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> execution, ResilienceContext context, TState state);
}
