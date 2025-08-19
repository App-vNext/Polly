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
            var task = Strategy.ExecuteCore(casted, context, state);

            // Using Unsafe.As avoids boxing allocations that would occur with a cast through object.
            Debug.Assert(task is ValueTask<Outcome<TResult>>, "Callback return type is identical to strategy return type");
            return Unsafe.As<ValueTask<Outcome<T>>, ValueTask<Outcome<TResult>>>(ref task);
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

            if (valueTask.IsCompletedSuccessfully)
            {
                return new ValueTask<Outcome<TResult>>(ConvertOutcome<T, TResult>(valueTask.Result));
            }

            return ConvertValueTaskAsync(valueTask, context);
        }

        static async ValueTask<Outcome<TResult>> ConvertValueTaskAsync(ValueTask<Outcome<T>> valueTask, ResilienceContext resilienceContext)
        {
            var outcome = await valueTask.ConfigureAwait(resilienceContext.ContinueOnCapturedContext);
            return ConvertOutcome<T, TResult>(outcome);
        }
    }
}
