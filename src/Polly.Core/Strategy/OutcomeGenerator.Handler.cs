using System.Collections.Generic;

namespace Polly.Strategy;

#pragma warning disable CA1034 // Nested types should not be visible

public sealed partial class OutcomeGenerator<TArgs, TValue>
{
    /// <summary>
    /// The resulting handler for the outcome.
    /// </summary>
    public abstract class Handler
    {
        private protected Handler(TValue defaultValue, Predicate<TValue> isValid)
        {
            DefaultValue = defaultValue;
            IsValid = isValid;
        }

        internal TValue DefaultValue { get; }

        internal Predicate<TValue> IsValid { get; }

        /// <summary>
        /// Determines if the handler should handle the outcome.
        /// </summary>
        /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
        /// <param name="outcome">The operation outcome.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The result of the handle operation.</returns>
        public abstract ValueTask<TValue> GenerateAsync<TResult>(Outcome<TResult> outcome, TArgs args);
    }

    private sealed class TypeHandler : Handler
    {
        private readonly Type _type;
        private readonly object _generator;

        public TypeHandler(
            Type type,
            object generator,
            TValue defaultValue,
            Predicate<TValue> isValid)
            : base(defaultValue, isValid)
        {
            _type = type;
            _generator = generator;
        }

        public override async ValueTask<TValue> GenerateAsync<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            TValue value = DefaultValue;

            if (_type == typeof(AnyResult))
            {
                value = await ((Func<Outcome, TArgs, ValueTask<TValue>>)_generator)(outcome.AsOutcome(), args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
            }
            else if (typeof(TResult) == _type)
            {
                value = await ((Func<Outcome<TResult>, TArgs, ValueTask<TValue>>)_generator)(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
            }

            if (IsValid(value))
            {
                return value;
            }

            return DefaultValue;
        }
    }

    private sealed class TypesHandler : Handler
    {
        private readonly Dictionary<Type, TypeHandler> _generators;

        public TypesHandler(
            IEnumerable<KeyValuePair<Type, object>> generators,
            TValue defaultValue,
            Predicate<TValue> isValid)
            : base(defaultValue, isValid)
            => _generators = generators.ToDictionary(v => v.Key, v => new TypeHandler(v.Key, v.Value, defaultValue, isValid));

        public override async ValueTask<TValue> GenerateAsync<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (_generators.TryGetValue(typeof(TResult), out var handler))
            {
                var value = await handler.GenerateAsync(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
                if (IsValid(value))
                {
                    return value;
                }
            }

            if (_generators.TryGetValue(typeof(AnyResult), out handler))
            {
                return await handler.GenerateAsync(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext);
            }

            return DefaultValue;
        }
    }
}
