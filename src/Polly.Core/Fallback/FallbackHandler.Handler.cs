using Polly.Strategy;

namespace Polly.Fallback;

internal sealed partial class FallbackHandler
{
    internal sealed class Handler
    {
        private readonly Dictionary<Type, object> _handlers;

        internal Handler(Dictionary<Type, object> handlers) => _handlers = handlers;

        public ValueTask<Func<OutcomeArguments<TResult, HandleFallbackArguments>, ValueTask<TResult>>?> ShouldHandleAsync<TResult>(OutcomeArguments<TResult, HandleFallbackArguments> args)
        {
            if (!_handlers.TryGetValue(typeof(TResult), out var handler))
            {
                return new((Func<OutcomeArguments<TResult, HandleFallbackArguments>, ValueTask<TResult>>?)null);
            }

            return ((FallbackHandler<TResult>)handler).ShouldHandleAsync(args);
        }
    }
}

