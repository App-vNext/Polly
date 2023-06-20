using System.Runtime.CompilerServices;

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
    private readonly bool _isGeneric;

    protected OutcomeResilienceStrategy(bool isGeneric)
    {
        if (!isGeneric && typeof(T) != typeof(object))
        {
            throw new NotSupportedException("For non-generic strategies the generic paramater should be 'object' type.");
        }

        _isGeneric = isGeneric;
    }

    protected internal sealed override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (_isGeneric)
        {
            if (typeof(TResult) != typeof(T))
            {
                return callback(context, state);
            }

            // cast is safe here, because TResult and T are the same type
            var callbackCasted = (Func<ResilienceContext, TState, ValueTask<Outcome<T>>>)(object)callback;
            var valueTask = ExecuteCallbackAsync(callbackCasted, context, state);

            return ConvertValueTask<TResult>(valueTask, context);
        }
        else
        {
            var valueTask = ExecuteCallbackAsync(
                static async (context, state) =>
                {
                    var outcome = await state.callback(context, state.state).ConfigureAwait(context.ContinueOnCapturedContext);

                    // cast the outcome to "object" based one (T)
                    return outcome.AsOutcome<T>();
                },
                context,
                (callback, state));

            return ConvertValueTask<TResult>(valueTask, context);
        }
    }

    protected abstract ValueTask<Outcome<T>> ExecuteCallbackAsync<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback,
        ResilienceContext context,
        TState state);

    private static ValueTask<Outcome<TResult>> ConvertValueTask<TResult>(ValueTask<Outcome<T>> valueTask, ResilienceContext resilienceContext)
    {
        if (valueTask.IsCompletedSuccessfully)
        {
            return new ValueTask<Outcome<TResult>>(valueTask.Result.AsOutcome<TResult>());
        }

        return ConvertValueTaskAsync(valueTask, resilienceContext);

        static async ValueTask<Outcome<TResult>> ConvertValueTaskAsync(ValueTask<Outcome<T>> valueTask, ResilienceContext resilienceContext)
        {
            var outcome = await valueTask.ConfigureAwait(resilienceContext.ContinueOnCapturedContext);
            return outcome.AsOutcome<TResult>();
        }
    }
}
