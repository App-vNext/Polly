using System;
using Polly.Strategy;

namespace Polly.Strategy;

/// <summary>
/// The base class for all generators that generate a value based on the <see cref="Outcome{TResult}"/>.
/// </summary>
/// <typeparam name="TArgs">The arguments the generator uses.</typeparam>
/// <typeparam name="TValue">The type of generated value.</typeparam>
public sealed partial class OutcomeGenerator<TArgs, TValue>
    where TArgs : IResilienceArguments
{
    private readonly Dictionary<Type, object> _generators = new();

    /// <summary>
    /// Gets a value indicating whether the generator is empty.
    /// </summary>
    public bool IsEmpty => _generators.Count == 0;

    /// <summary>
    /// Adds a result generator for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a generator for.</typeparam>
    /// <param name="generator">The generator to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeGenerator<TArgs, TValue> SetGenerator<TResult>(Func<Outcome<TResult>, TArgs, TValue> generator)
    {
        Guard.NotNull(generator);

        return SetGenerator<TResult>((outcome, args) => new ValueTask<TValue>(generator(outcome, args)));
    }

    /// <summary>
    /// Adds a result generator for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a generator for.</typeparam>
    /// <param name="generator">The generator to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeGenerator<TArgs, TValue> SetGenerator<TResult>(Func<Outcome<TResult>, TArgs, ValueTask<TValue>> generator)
    {
        Guard.NotNull(generator);

        _generators[typeof(TResult)] = generator;

        return this;
    }

    /// <summary>
    /// Adds a result generator for all result types including the void-based results.
    /// </summary>
    /// <param name="generator">The generator to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeGenerator<TArgs, TValue> SetGenerator(Func<Outcome, TArgs, TValue> generator)
    {
        Guard.NotNull(generator);

        return SetGenerator((outcome, args) => new ValueTask<TValue>(generator(outcome, args)));
    }

    /// <summary>
    /// Adds a result generator for all result types including the void-based results.
    /// </summary>
    /// <param name="generator">The generator to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeGenerator<TArgs, TValue> SetGenerator(Func<Outcome, TArgs, ValueTask<TValue>> generator)
    {
        Guard.NotNull(generator);

        _generators[typeof(AnyResult)] = generator;

        return this;
    }

    /// <summary>
    /// Creates a handler for the specified generators.
    /// </summary>
    /// <param name="defaultValue">The default value returned by the generator.</param>
    /// <param name="valueValidator">The validator that determines if the generated value is valid.</param>
    /// <returns>Handler instance or null if no generators are registered.</returns>
    public Handler? CreateHandler(TValue defaultValue, Predicate<TValue> valueValidator)
    {
        var pairs = _generators.ToArray();

        return pairs.Length switch
        {
            0 => null,
            1 => new TypeHandler(pairs[0].Key, pairs[0].Value, defaultValue, valueValidator),
            _ => new TypesHandler(pairs, defaultValue, valueValidator)
        };
    }

    private sealed class AnyResult
    {
    }
}
