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

        public override async ValueTask HandleAsync<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (typeof(TResult) == _type)
            {
                if (_type == typeof(VoidResult))
                {
                    var callback = (Func<Outcome, TArgs, ValueTask>)_callback;
                    await callback(outcome.AsOutcome(), args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
                }
                else
                {
                    var callback = (Func<Outcome<TResult>, TArgs, ValueTask>)_callback;
                    await callback(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
                }

            }
            else if (_type == typeof(object))
            {
                var callback = (Func<Outcome<object>, TArgs, ValueTask>)_callback;
                var objectOutcome = outcome.HasResult ? new Outcome<object>(outcome.Result!) : new Outcome<object>(outcome.Exception!);
                await callback(objectOutcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
            }
        }
    }

    private sealed class TypesHandler : Handler
    {
        private readonly Dictionary<Type, TypeHandler> _handlers;

        public TypesHandler(IEnumerable<KeyValuePair<Type, object>> callbacks)
            => _handlers = callbacks.ToDictionary(v => v.Key, v => new TypeHandler(v.Key, v.Value));

        public override async ValueTask HandleAsync<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (_handlers.TryGetValue(typeof(TResult), out var handler))
            {
                await handler.HandleAsync(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
            }

            if (_handlers.TryGetValue(typeof(object), out handler))
            {
                await handler.HandleAsync(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
            }
        }
    }
}
