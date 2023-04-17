using System.Collections.Generic;

namespace Polly.Strategy;

#pragma warning disable CA1034 // Nested types should not be visible

public partial class OutcomeEvent<TArgs>
{
    /// <summary>
    /// The handler for this event.
    /// </summary>
    public abstract class Handler
    {
        private protected Handler()
        {
        }

        /// <summary>
        /// Invokes all registered callbacks.
        /// </summary>
        /// <typeparam name="TResult">The result type to invoke a callback for.</typeparam>
        /// <param name="outcome">The operation outcome.</param>
        /// <param name="args">The arguments passed to the registered callbacks.</param>
        /// <returns>The <see cref="ValueTask"/>.</returns>
        public abstract ValueTask HandleAsync<TResult>(Outcome<TResult> outcome, TArgs args);
    }

    private sealed class TypeHandler : Handler
    {
        private readonly Type _type;
        private readonly object _callback;

        public TypeHandler(Type type, object callback)
        {
            _type = type;
            _callback = callback;
        }

        public override ValueTask HandleAsync<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (typeof(TResult) != _type)
            {
                return default;
            }

            var callback = (Func<Outcome<TResult>, TArgs, ValueTask>)_callback;
            return callback(outcome, args);
        }
    }

    private sealed class TypesHandler : Handler
    {
        private readonly Dictionary<Type, TypeHandler> _handlers;

        public TypesHandler(IEnumerable<KeyValuePair<Type, object>> callbacks)
            => _handlers = callbacks.ToDictionary(v => v.Key, v => new TypeHandler(v.Key, v.Value));

        public override ValueTask HandleAsync<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (_handlers.TryGetValue(typeof(TResult), out var handler))
            {
                return handler.HandleAsync(outcome, args);
            }

            return default;
        }
    }
}
