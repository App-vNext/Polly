using Polly.Strategy;

namespace Polly.Hedging;

public partial class HedgingHandler
{
    internal sealed class Handler
    {
        private readonly OutcomePredicate<HandleHedgingArguments>.Handler? _predicateHandler;
        private readonly Dictionary<Type, object> _generators;

        internal Handler(OutcomePredicate<HandleHedgingArguments>.Handler? handler, Dictionary<Type, object> generators)
        {
            _predicateHandler = handler;
            _generators = generators;
        }

        public bool HandlesHedging<TResult>() => _generators.ContainsKey(typeof(TResult));

        public ValueTask<bool> ShouldHandleAsync<TResult>(Outcome<TResult> outcome, HandleHedgingArguments arguments)
        {
            if (_predicateHandler == null)
            {
                return new ValueTask<bool>(false);
            }

            return _predicateHandler.ShouldHandleAsync(outcome, arguments);
        }

        public Func<Task<TResult>>? TryCreateHedgedAction<TResult>(ResilienceContext context, int attempt)
        {
            if (!_generators.TryGetValue(typeof(TResult), out var generator))
            {
                return null;
            }

            return ((HedgingActionGenerator<TResult>)generator)(new HedgingActionGeneratorArguments<TResult>(context, attempt));
        }
    }
}

