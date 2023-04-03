using System;
using Polly.Strategy;

namespace Polly.Strategy;

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
#pragma warning disable CA1005 // Too many generic arguments, but this is the only way to make the API work without unnecessary extensions

/// <summary>
/// The base class for all generators that generate a value based on the <see cref="Outcome{TResult}"/>.
/// </summary>
/// <typeparam name="TGeneratedValue">The type of generated value.</typeparam>
/// <typeparam name="TArgs">The arguments the generator uses.</typeparam>
/// <typeparam name="TSelf">The class that implements <see cref="OutcomeGenerator{TGeneratedValue, TArgs, TSelf}"/>.</typeparam>
public abstract partial class OutcomeGenerator<TGeneratedValue, TArgs, TSelf>
    where TArgs : IResilienceArguments
    where TSelf : OutcomeGenerator<TGeneratedValue, TArgs, TSelf>
{
    private readonly Dictionary<Type, object> _generators = new();

    /// <summary>
    /// Gets the default value returned by the generator.
    /// </summary>
    protected abstract TGeneratedValue DefaultValue { get; }

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
    public TSelf SetGenerator<TResult>(Func<Outcome<TResult>, TArgs, TGeneratedValue> generator)
    {
        Guard.NotNull(generator);

        return SetGenerator<TResult>((outcome, args) => new ValueTask<TGeneratedValue>(generator(outcome, args)));
    }

    /// <summary>
    /// Adds a result generator for the specified result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to add a generator for.</typeparam>
    /// <param name="generator">The generator to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    public TSelf SetGenerator<TResult>(Func<Outcome<TResult>, TArgs, ValueTask<TGeneratedValue>> generator)
    {
        Guard.NotNull(generator);

        _generators[typeof(TResult)] = generator;

        return (TSelf)this;
    }

    /// <summary>
    /// Determines if the generated value is valid.
    /// </summary>
    /// <param name="value">The value returned by the user-provided generator.</param>
    /// <returns>True if generated value is valid, false otherwise.</returns>
    protected abstract bool IsValid(TGeneratedValue value);

    /// <summary>
    /// Creates a handler for the specified generators.
    /// </summary>
    /// <returns>Handler instance or null if no generators are registered.</returns>
    protected internal Handler? CreateHandler()
    {
        var pairs = _generators.ToArray();

        return pairs.Length switch
        {
            0 => null,
            1 => new TypeHandler(pairs[0].Key, pairs[0].Value, DefaultValue, IsValid),
            _ => new TypesHandler(pairs, DefaultValue, IsValid)
        };
    }
}
