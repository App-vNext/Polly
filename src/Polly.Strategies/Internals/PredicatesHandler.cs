namespace Polly.Internals;

internal abstract class PredicatesHandler<TContext>
{
    public abstract ValueTask<bool> HandleAsync<T>(Outcome<T> outcome, TContext context, ResilienceContext resilienceContext);

    public static PredicatesHandler<TContext> Create(Predicates<TContext> input) => new Many(input.Callbacks);

    private class Many : PredicatesHandler<TContext>
    {
        private readonly Dictionary<Type, List<object>> _callbacks;

        public Many(Dictionary<Type, List<object>> callbacks)
        {
            _callbacks = callbacks;
        }

        public override async ValueTask<bool> HandleAsync<T>(Outcome<T> outcome, TContext context, ResilienceContext resilienceContext)
        {
            if (!_callbacks.TryGetValue(typeof(T), out var predicates))
            {
                return false;
            }

            foreach (var predicate in predicates)
            {
                var outcomePredicate = (PredicatesCallback<T, TContext>)predicate;

                if (await outcomePredicate(outcome, context).ConfigureAwait(resilienceContext.ContinueOnCapturedContext))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
