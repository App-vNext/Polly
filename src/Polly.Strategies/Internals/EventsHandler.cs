namespace Polly.Internals;

internal abstract class EventsHandler<TContext>
{
    public abstract ValueTask HandleAsync<T>(Outcome<T> outcome, TContext context, ResilienceContext resilienceContext);

    public static EventsHandler<TContext> Create(Events<TContext> input) => new Many(input.Callbacks);

    private class Many : EventsHandler<TContext>
    {
        private readonly Dictionary<Type, List<object>> _callbacks;
        private readonly List<EventsCallback<object, TContext>> _genericCallback;

        public Many(Dictionary<Type, List<object>> callbacks)
        {
            if (callbacks.TryGetValue(typeof(object), out var val))
            {
                _genericCallback = val.Select(v => (EventsCallback<object, TContext>)v).ToList();
            }
            else
            {
                _genericCallback = new List<EventsCallback<object, TContext>>();
            }

            _callbacks = callbacks;
        }

        public override async ValueTask HandleAsync<T>(Outcome<T> outcome, TContext context, ResilienceContext resilienceContext)
        {
            foreach (var callback in _genericCallback)
            {
                var objectOutcome = outcome.Exception == null ? new Outcome<object>(outcome.Result!) : new Outcome<object>(outcome.Exception);

                await callback.Invoke(objectOutcome, context).ConfigureAwait(false);
            }

            if (!_callbacks.TryGetValue(typeof(T), out var callbacks))
            {
                return;
            }

            foreach (var callback in callbacks)
            {
                var outcomeCallback = (EventsCallback<T, TContext>)callback;

                await outcomeCallback(outcome, context).ConfigureAwait(resilienceContext.ContinueOnCapturedContext);
            }
        }
    }
}
