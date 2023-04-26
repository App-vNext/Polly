using Polly.Strategy;

namespace Polly.Hedging;

public partial class HedgingHandler
{
    internal sealed class Handler
    {
        private readonly OutcomePredicate<HandleHedgingArguments>.Handler _handler;
        private readonly Dictionary<Type, object> _generators;

        internal Handler(OutcomePredicate<HandleHedgingArguments>.Handler handler, Dictionary<Type, object> generators)
        {
            _handler = handler;
            _generators = generators;
        }

        public bool HandlesHedging<TResult>() => _generators.ContainsKey(typeof(TResult));

        public ValueTask<bool> ShouldHandleAsync<TResult>(Outcome<TResult> outcome, HandleHedgingArguments arguments) => _handler.ShouldHandleAsync(outcome, arguments);

        public Func<Task<TResult>>? TryCreateHedgedAction<TResult>(ResilienceContext context)
        {
            if (!_generators.TryGetValue(typeof(TResult), out var generator))
            {
                return null;
            }

            return ((HedgingActionGenerator<TResult>)generator)(new HedgingActionGeneratorArguments<TResult>(context));
        }
    }
}

