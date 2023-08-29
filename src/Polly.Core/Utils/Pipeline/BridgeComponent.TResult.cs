using System.Runtime.CompilerServices;

namespace Polly.Utils.Pipeline;

[DebuggerDisplay("{Strategy}")]
internal sealed class BridgeComponent<T> : BridgeComponentBase
{
    public BridgeComponent(ResilienceStrategy<T> strategy)
        : base(strategy) => Strategy = strategy;

    public ResilienceStrategy<T> Strategy { get; }

    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        // Check if we can cast directly, thus saving some cycles and improving the performance
        if (callback is Func<ResilienceContext, TState, ValueTask<Outcome<T>>> casted)
        {
            return ConvertValueTask<TResult>(
                Strategy.ExecuteCore(casted, context, state),
                context);
        }
        else
        {
            var valueTask = Strategy.ExecuteCore(
                static async (context, state) =>
                {
                    var outcome = await state.callback(context, state.state).ConfigureAwait(context.ContinueOnCapturedContext);
                    return ConvertOutcome<TResult, T>(outcome);
                },
                context,
                (callback, state));

            return ConvertValueTask<TResult>(valueTask, context);
        }
    }

    private static ValueTask<Outcome<TTo>> ConvertValueTask<TTo>(ValueTask<Outcome<T>> valueTask, ResilienceContext resilienceContext)
    {
        if (valueTask.IsCompletedSuccessfully)
        {
            return new ValueTask<Outcome<TTo>>(ConvertOutcome<T, TTo>(valueTask.Result));
        }

        return ConvertValueTaskAsync(valueTask, resilienceContext);

        static async ValueTask<Outcome<TTo>> ConvertValueTaskAsync(ValueTask<Outcome<T>> valueTask, ResilienceContext resilienceContext)
        {
            var outcome = await valueTask.ConfigureAwait(resilienceContext.ContinueOnCapturedContext);
            return ConvertOutcome<T, TTo>(outcome);
        }
    }

    private static Outcome<TTo> ConvertOutcome<TFrom, TTo>(Outcome<TFrom> outcome)
    {
        if (outcome.ExceptionDispatchInfo is not null)
        {
            return new Outcome<TTo>(outcome.ExceptionDispatchInfo);
        }

        if (outcome.Result is null)
        {
            return new Outcome<TTo>(default(TTo));
        }

        if (typeof(TTo) == typeof(TFrom))
        {
            var result = outcome.Result;

            // We can use the unsafe cast here because we know for sure these two types are the same
            return new Outcome<TTo>(Unsafe.As<TFrom, TTo>(ref result));
        }

        return new Outcome<TTo>((TTo)(object)outcome.Result);
    }
}
