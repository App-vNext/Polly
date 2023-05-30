using Polly.Strategy;

namespace Polly.Hedging;

internal partial class HedgingHandler
{
    internal sealed class Handler
    {
        private readonly Dictionary<Type, object> _predicates;
        private readonly Dictionary<Type, object> _generators;

        internal Handler(Dictionary<Type, object> predicates, Dictionary<Type, object> generators)
        {
            _predicates = predicates;
            _generators = generators;
        }

        public bool HandlesHedging<TResult>() => _generators.ContainsKey(typeof(TResult));

        public ValueTask<bool> ShouldHandleAsync<TResult>(Outcome<TResult> outcome, HandleHedgingArguments arguments)
        {
            if (!_predicates.TryGetValue(typeof(TResult), out var predicate))
            {
                return new ValueTask<bool>(false);
            }

            if (typeof(TResult) == typeof(VoidResult))
            {
                return ((Func<Outcome<object>, HandleHedgingArguments, ValueTask<bool>>)predicate)(outcome.AsOutcome(), arguments);
            }
            else
            {
                return ((Func<Outcome<TResult>, HandleHedgingArguments, ValueTask<bool>>)predicate)(outcome, arguments);

            }
        }

        public Func<Task<TResult>>? TryCreateHedgedAction<TResult>(ResilienceContext context, int attempt)
        {
            if (!_generators.TryGetValue(typeof(TResult), out var generator))
            {
                return null;
            }

            return ((Func<HedgingActionGeneratorArguments<TResult>, Func<Task<TResult>>?>)generator)(new HedgingActionGeneratorArguments<TResult>(context, attempt));
        }
    }
}

