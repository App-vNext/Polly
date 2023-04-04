using System.Collections.Generic;

namespace Polly.Strategy;

#pragma warning disable CA1034 // Nested types should not be visible

public abstract partial class OutcomeEvent<TArgs, TSelf>
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
        public abstract ValueTask Handle<TResult>(Outcome<TResult> outcome, TArgs args);
    }

    private sealed class TypeHandler : Handler
    {
        private readonly Type _type;
        private readonly object[] _callbacks;

        public TypeHandler(Type type, List<object> callbacks)
        {
            _type = type;
            _callbacks = callbacks.ToArray();
        }

        public override ValueTask Handle<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (typeof(TResult) != _type)
            {
                return default;
            }

            return HandleCoreAsync(outcome, args);
        }

        private ValueTask HandleCoreAsync<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (_callbacks.Length == 1)
            {
                var callback = (Func<Outcome<TResult>, TArgs, ValueTask>)_callbacks[0];

                return callback(outcome, args);
            }

            return InvokeCallbacksAsync(outcome, args);
        }

        private async ValueTask InvokeCallbacksAsync<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            foreach (var obj in _callbacks)
            {
                var callback = (Func<Outcome<TResult>, TArgs, ValueTask>)obj;

                await callback(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
            }
        }
    }

    private sealed class TypesHandler : Handler
    {
        private readonly Dictionary<Type, TypeHandler> _handlers;

        public TypesHandler(IEnumerable<KeyValuePair<Type, List<object>>> callbacks)
            => _handlers = callbacks.ToDictionary(v => v.Key, v => new TypeHandler(v.Key, v.Value));

        public override ValueTask Handle<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (_handlers.TryGetValue(typeof(TResult), out var handler))
            {
                return handler.Handle(outcome, args);
            }

            return default;
        }
    }
}
