using System.Threading.Tasks;

namespace Polly.Strategy;

internal abstract class EventInvoker<TArgs>
{
    public static EventInvoker<TArgs>? Create<TResult>(Func<OutcomeArguments<TResult, TArgs>, ValueTask>? callback, bool isGeneric) => callback switch
    {
        Func<OutcomeArguments<object, TArgs>, ValueTask> generic when !isGeneric => new NonGenericEventInvoker(generic),
        { } => new GenericEventInvoker<TResult>(callback),
        _ => null,
    };

    public abstract ValueTask HandleAsync<TResult>(OutcomeArguments<TResult, TArgs> args);

    private sealed class NonGenericEventInvoker : EventInvoker<TArgs>
    {
        private readonly Func<OutcomeArguments<object, TArgs>, ValueTask> _callback;

        public NonGenericEventInvoker(Func<OutcomeArguments<object, TArgs>, ValueTask> callback) => _callback = callback;

        public override ValueTask HandleAsync<TResult>(OutcomeArguments<TResult, TArgs> args) => _callback(args.AsObjectArguments());
    }

    private sealed class GenericEventInvoker<T> : EventInvoker<TArgs>
    {
        private readonly object _callback;

        public GenericEventInvoker(Func<OutcomeArguments<T, TArgs>, ValueTask> callback) => _callback = callback;

        public override ValueTask HandleAsync<TResult>(OutcomeArguments<TResult, TArgs> args)
        {
            if (typeof(TResult) == typeof(T))
            {
                return ((Func<OutcomeArguments<TResult, TArgs>, ValueTask>)_callback)(args);
            }

            return default;
        }
    }
}
