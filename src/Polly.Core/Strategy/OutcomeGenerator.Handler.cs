using System.Collections.Generic;

namespace Polly.Strategy;

#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA1005 // Avoid excessive parameters on generic types
#pragma warning disable S2436 // Types and methods should not have too many generic parameters

public abstract partial class OutcomeGenerator<TGeneratedValue, TArgs, TSelf>
{
    /// <summary>
    /// The resulting handler for the outcome.
    /// </summary>
    public abstract class Handler
    {
        private protected Handler(TGeneratedValue defaultValue, Predicate<TGeneratedValue> isValid)
        {
            DefaultValue = defaultValue;
            IsValid = isValid;
        }

        internal TGeneratedValue DefaultValue { get; }

        internal Predicate<TGeneratedValue> IsValid { get; }

        /// <summary>
        /// Determines if the handler should handle the outcome.
        /// </summary>
        /// <typeparam name="TResult">The result type to add a predicate for.</typeparam>
        /// <param name="outcome">The operation outcome.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The result of the handle operation.</returns>
        public abstract ValueTask<TGeneratedValue> Generate<TResult>(Outcome<TResult> outcome, TArgs args);
    }

    private sealed class TypeHandler : Handler
    {
        private readonly Type _type;
        private readonly object _generator;

        public TypeHandler(
            Type type,
            object generator,
            TGeneratedValue defaultValue,
            Predicate<TGeneratedValue> isValid)
            : base(defaultValue, isValid)
        {
            _type = type;
            _generator = generator;
        }

        public override async ValueTask<TGeneratedValue> Generate<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (typeof(TResult) == _type)
            {
                var value = await ((Func<Outcome<TResult>, TArgs, ValueTask<TGeneratedValue>>)_generator)(outcome, args).ConfigureAwait(args.Context.ContinueOnCapturedContext);

                if (IsValid(value))
                {
                    return value;
                }

                return DefaultValue;
            }

            return DefaultValue;
        }
    }

    private sealed class TypesHandler : Handler
    {
        private readonly Dictionary<Type, TypeHandler> _generators;

        public TypesHandler(
            IEnumerable<KeyValuePair<Type, object>> generators,
            TGeneratedValue defaultValue,
            Predicate<TGeneratedValue> isValid)
            : base(defaultValue, isValid)
            => _generators = generators.ToDictionary(v => v.Key, v => new TypeHandler(v.Key, v.Value, defaultValue, isValid));

        public override ValueTask<TGeneratedValue> Generate<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (_generators.TryGetValue(typeof(TResult), out var handler))
            {
                return handler.Generate(outcome, args);
            }

            return new ValueTask<TGeneratedValue>(DefaultValue);
        }
    }
}
