using System.Threading.Tasks;

namespace Polly.Strategy;

internal abstract class EventInvoker<TArgs>
    where TArgs : IResilienceArguments
{
    public static EventInvoker<TArgs>? NonGeneric(Func<Outcome<object>, TArgs, ValueTask>? callback)
        => callback == null ? null : new NonGenericEventInvoker(callback);

    public static EventInvoker<TArgs>? Generic<TResult>(Func<Outcome<TResult>, TArgs, ValueTask>? callback)
        => callback == null ? null : new GenericEventInvoker<TResult>(callback);

    public abstract ValueTask HandleAsync<TResult>(Outcome<TResult> outcome, TArgs args);

    private sealed class NonGenericEventInvoker : EventInvoker<TArgs>
    {
        private readonly Func<Outcome<object>, TArgs, ValueTask> _callback;

        public NonGenericEventInvoker(Func<Outcome<object>, TArgs, ValueTask> callback) => _callback = callback;

        public override ValueTask HandleAsync<TResult>(Outcome<TResult> outcome, TArgs args) => _callback(outcome.AsObjectOutcome(), args);
    }

    private sealed class GenericEventInvoker<T> : EventInvoker<TArgs>
    {
        private readonly object _callback;

        public GenericEventInvoker(Func<Outcome<T>, TArgs, ValueTask> callback) => _callback = callback;

        public override ValueTask HandleAsync<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (typeof(TResult) == typeof(T))
            {
                return ((Func<Outcome<TResult>, TArgs, ValueTask>)_callback)(outcome, args);
            }

            return default;
        }
    }
}
