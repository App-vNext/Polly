namespace Polly.Utils;

/// <summary>
/// This base strategy class is used to simplify the implementation of generic (reactive)
/// strategies by limiting the number of generic types this strategy receives.
/// </summary>
/// <typeparam name="T">The type of result this strategy handles.</typeparam>
/// <remarks>
/// For strategies that handle all result types the generic parameter must be of type <see cref="object"/>.
/// </remarks>
internal abstract class OutcomeResilienceStrategy<T> : ResilienceStrategy
{
    protected OutcomeResilienceStrategy()
    {
    }

    protected internal sealed override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        // Check if we can cast directly, thus saving some cycles and improving the performance
        if (context.ResultType == typeof(T))
        {
            // cast is safe here, because TResult and T are the same type
            var callbackCasted = (Func<ResilienceContext, TState, ValueTask<Outcome<T>>>)(object)callback;
            var valueTask = ExecuteCore(callbackCasted, context, state);

            return TaskHelper.ConvertValueTask<T, TResult>(valueTask, context);
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

    protected abstract ValueTask<Outcome<T>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback,
        ResilienceContext context,
        TState state);
}
