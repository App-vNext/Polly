using Polly.Strategy;

namespace Polly.Fallback;

internal sealed partial class FallbackHandler
{
    internal sealed class Handler
    {
        private readonly OutcomePredicate<HandleFallbackArguments>.Handler _handler;
        private readonly Dictionary<Type, object> _actions;

        internal Handler(OutcomePredicate<HandleFallbackArguments>.Handler handler, Dictionary<Type, object> generators)
        {
            _handler = handler;
            _actions = generators;
        }

        public async ValueTask<Func<Outcome<TResult>, HandleFallbackArguments, ValueTask<TResult>>?> ShouldHandleAsync<TResult>(Outcome<TResult> outcome, HandleFallbackArguments arguments)
        {
            if (!_actions.TryGetValue(typeof(TResult), out var action))
            {
                return null;
            }

            if (!await _handler.ShouldHandleAsync(outcome, arguments).ConfigureAwait(arguments.Context.ContinueOnCapturedContext))
            {
                return null;
            }

            return (Func<Outcome<TResult>, HandleFallbackArguments, ValueTask<TResult>>?)action;
        }
    }
}

