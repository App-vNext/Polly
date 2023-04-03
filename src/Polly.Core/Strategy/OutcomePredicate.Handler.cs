using System.Collections.Generic;

namespace Polly.Strategy;

#pragma warning disable CA1034 // Nested types should not be visible

public abstract partial class OutcomePredicate<TArgs, TSelf>
{
    /// <summary>
    /// The resulting predicate handler.
    /// </summary>
    public abstract class Handler
    {
        private protected Handler()
        {
        }

        /// <summary>
        /// Determines if the handler should handle the outcome.
        /// </summary>
        /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
        /// <param name="outcome">The operation outcome.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The result of the handle operation.</returns>
        public abstract ValueTask<bool> ShouldHandle<TResult>(Outcome<TResult> outcome, TArgs args);
    }

    private sealed class TypeHandler : Handler
    {
        private readonly Type _type;
        private readonly object[] _predicates;

        public TypeHandler(Type type, List<object> predicates)
        {
            _type = type;
            _predicates = predicates.ToArray();
        }

        public override ValueTask<bool> ShouldHandle<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            // special case for exception-based callbacks
            if (!outcome.HasResult && _type == typeof(ExceptionOutcome))
            {
                return ShouldHandlerCoreAsync(new Outcome<ExceptionOutcome>(outcome.Exception!), args);
            }

            if (typeof(TResult) != _type)
            {
                return new ValueTask<bool>(false);
            }

            return ShouldHandlerCoreAsync(outcome, args);
        }

        private ValueTask<bool> ShouldHandlerCoreAsync<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (_predicates.Length == 1)
            {
                var predicate = (Func<Outcome<TResult>, TArgs, ValueTask<bool>>)_predicates[0];

                return predicate(outcome, args);
            }

            return ExecutePredicatesAsync(outcome, args);
        }

        private async ValueTask<bool> ExecutePredicatesAsync<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            foreach (var obj in _predicates)
            {
                var predicate = (Func<Outcome<TResult>, TArgs, ValueTask<bool>>)obj;

                if (await predicate(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext))
                {
                    return true;
                }
            }

            return false;
        }
    }

    private sealed class TypesHandler : Handler
    {
        private readonly Dictionary<Type, TypeHandler> _predicates;

        public TypesHandler(IEnumerable<KeyValuePair<Type, List<object>>> predicates)
            => _predicates = predicates.ToDictionary(v => v.Key, v => new TypeHandler(v.Key, v.Value));

        public override async ValueTask<bool> ShouldHandle<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (_predicates.TryGetValue(typeof(TResult), out var handler) && await handler.ShouldHandle(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext))
            {
                return true;
            }

            if (!outcome.HasResult && _predicates.TryGetValue(typeof(ExceptionOutcome), out var exceptionHandler))
            {
                return await exceptionHandler.ShouldHandle(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
            }

            return false;
        }
    }
}
