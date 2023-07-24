namespace Polly;

#pragma warning disable CA1031 // Do not catch general exception types

/// <summary>
/// Resilience strategy is used to execute the user-provided callbacks.
/// </summary>
/// <remarks>
/// Resilience strategy supports various types of callbacks and provides a unified way to execute them.
/// This includes overloads for synchronous and asynchronous callbacks, generic and non-generic callbacks.
/// </remarks>
public abstract partial class ResilienceStrategy
{
    internal static ResilienceContextPool Pool => ResilienceContextPool.Shared;

    internal ResilienceStrategyOptions? Options { get; set; }

    /// <summary>
    /// An implementation of resilience strategy that executes the specified <paramref name="callback"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>
    /// An instance of pending <see cref="ValueTask"/> for asynchronous executions or completed <see cref="ValueTask"/> task for synchronous exexutions.
    /// </returns>
    /// <remarks>
    /// <c>This method is called for both synchronous and asynchronous execution flows.</c>
    /// <para>
    /// You can use <see cref="ResilienceContext.IsSynchronous"/> to dermine wheether the <paramref name="callback"/> is synchronous or asynchronous one.
    /// This is useful when the custom strategy behaves differently in each execution flow. In general, for most strategies, the implementation
    /// is the same for both execution flows.
    /// </para>
    /// <para>
    /// The provided callback never throws an exception. Instead, the exception is captured and converted to an <see cref="Outcome{TResult}"/>.
    /// Similarly, do not throw exceptions from your strategy implementation. Instead, return an exception instance as <see cref="Outcome{TResult}"/>.
    /// </para>
    /// </remarks>
    protected internal abstract ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
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

    internal static ValueTask<Outcome<TResult>> ExecuteCallbackSafeAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (context.CancellationToken.IsCancellationRequested)
        {
            return new ValueTask<Outcome<TResult>>(Outcome.FromException<TResult>(new OperationCanceledException(context.CancellationToken)));
        }

        try
        {
            var callbackTask = callback(context, state);
            if (callbackTask.IsCompleted)
            {
                return new ValueTask<Outcome<TResult>>(callbackTask.GetResult());
            }

            return AwaitTask(callbackTask, context.ContinueOnCapturedContext);
        }
        catch (Exception e)
        {
            return new ValueTask<Outcome<TResult>>(Outcome.FromException<TResult>(e));
        }

        static async ValueTask<Outcome<T>> AwaitTask<T>(ValueTask<Outcome<T>> task, bool continueOnCapturedContext)
        {
            try
            {
                return await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception e)
            {
                return Outcome.FromException<T>(e);
            }
        }
    }
}
