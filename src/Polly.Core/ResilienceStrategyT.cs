namespace Polly;

#pragma warning disable CA1031 // Do not catch general exception types

/// <summary>
/// Resilience strategy is used to execute the user-provided callbacks.
/// </summary>
/// <typeparam name="T">The type of result this strategy supports.</typeparam>
/// <remarks>
/// Resilience strategy supports various types of callbacks of <typeparamref name="T"/> result type
/// and provides a unified way to execute them. This includes overloads for synchronous and asynchronous callbacks.
/// </remarks>
public abstract partial class ResilienceStrategy<T>
{
    internal static ResilienceContextPool Pool => ResilienceContextPool.Shared;

    internal ResilienceStrategyOptions? Options { get; set; }

    internal ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
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
    /// An instance of pending <see cref="ValueTask"/> for asynchronous executions or completed <see cref="ValueTask"/> task for synchronous executions.
    /// </returns>
    /// <remarks>
    /// <strong>This method is called for both synchronous and asynchronous execution flows.</strong>
    /// <para>
    /// You can use <see cref="ResilienceContext.IsSynchronous"/> to determine whether the <paramref name="callback"/> is synchronous or asynchronous one.
    /// This is useful when the custom strategy behaves differently in each execution flow. In general, for most strategies, the implementation
    /// is the same for both execution flows.
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

    internal static ValueTask<Outcome<TResult>> ExecuteCallbackSafeAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
        where TResult : T
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

        static async ValueTask<Outcome<TResult>> AwaitTask(ValueTask<Outcome<TResult>> task, bool continueOnCapturedContext)
        {
            try
            {
                return await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception e)
            {
                return Outcome.FromException<TResult>(e);
            }
        }
    }

    internal Outcome<TResult> ExecuteCoreSync<TResult, TState>(
        Func<ResilienceContext, TState, Outcome<TResult>> callback,
        ResilienceContext context,
        TState state)
        where TResult : T
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
