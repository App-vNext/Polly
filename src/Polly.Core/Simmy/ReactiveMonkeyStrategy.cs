namespace Polly.Simmy;

/// <summary>
/// This base strategy class is used to simplify the implementation of generic (reactive)
/// strategies by limiting the number of generic types the execute method receives.
/// </summary>
/// <typeparam name="T">The type of result this strategy handles.</typeparam>
/// <remarks>
/// For strategies that handle all result types the generic parameter must be of type <see cref="object"/>.
/// </remarks>
public abstract class ReactiveMonkeyStrategy<T> : MonkeyStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReactiveMonkeyStrategy{T}"/> class.
    /// </summary>
    /// <param name="options">The chaos strategy options.</param>
    protected ReactiveMonkeyStrategy(MonkeyStrategyOptions options)
        : base(options)
    {
    }

    /// <inheritdoc/>
    protected internal sealed override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        // Check if we can cast directly, thus saving some cycles and improving the performance
        if (callback is Func<ResilienceContext, TState, ValueTask<Outcome<T>>> casted)
        {
            return TaskHelper.ConvertValueTask<T, TResult>(
                ExecuteCore(casted, context, state),
                context);
        }
        else
        {
            var valueTask = ExecuteCore(
                static async (context, state) =>
                {
                    var outcome = await state.callback(context, state.state).ConfigureAwait(context.ContinueOnCapturedContext);
                    return outcome.AsOutcome<T>();
                },
                context,
                (callback, state));

            return TaskHelper.ConvertValueTask<T, TResult>(valueTask, context);
        }
    }

    /// <summary>
    /// An implementation of resilience strategy that executes the specified <paramref name="callback"/>.
    /// </summary>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>
    /// An instance of a pending <see cref="ValueTask"/> for asynchronous executions or a completed <see cref="ValueTask"/> task for synchronous executions.
    /// </returns>
    /// <remarks>
    /// <strong>This method is called for both synchronous and asynchronous execution flows.</strong>
    /// <para>
    /// You can use <see cref="ResilienceContext.IsSynchronous"/> to determine whether <paramref name="callback"/> is synchronous or asynchronous.
    /// This is useful when the custom strategy behaves differently in each execution flow. In general, for most strategies, the implementation
    /// is the same for both execution flows.
    /// See <seealso href="https://github.com/App-vNext/Polly/blob/main/src/Polly.Core/README.md#about-synchronous-and-asynchronous-executions"/> for more details.
    /// </para>
    /// <para>
    /// The provided callback never throws an exception. Instead, the exception is captured and converted to an <see cref="Outcome{TResult}"/>.
    /// Similarly, do not throw exceptions from your strategy implementation. Instead, return an exception instance as <see cref="Outcome{TResult}"/>.
    /// </para>
    /// </remarks>
    protected abstract ValueTask<Outcome<T>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback,
        ResilienceContext context,
        TState state);
}
