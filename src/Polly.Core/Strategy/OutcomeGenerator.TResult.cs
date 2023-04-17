using System;
using Polly.Strategy;

namespace Polly.Strategy;

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
#pragma warning disable CA1005 // Avoid excessive parameters on generic types

/// <summary>
/// A class that generates values based on the <see cref="Outcome{TResult}"/> and <typeparamref name="TArgs"/>.
/// </summary>
/// <typeparam name="TArgs">The arguments the generator uses.</typeparam>
/// <typeparam name="TValue">The type of generated value.</typeparam>
/// <typeparam name="TResult">The result type that this event handles.</typeparam>
public sealed class OutcomeGenerator<TArgs, TValue, TResult>
    where TArgs : IResilienceArguments
{
    private Func<Outcome<TResult>, TArgs, ValueTask<TValue>>? _generator;

    /// <summary>
    /// Gets a value indicating whether the generator is empty.
    /// </summary>
    public bool IsEmpty => _generator is null;

    /// <summary>
    /// Adds a result generator for a specific result type.
    /// </summary>
    /// <param name="generator">The value generator.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeGenerator<TArgs, TValue, TResult> SetGenerator(Func<Outcome<TResult>, TArgs, TValue> generator)
    {
        Guard.NotNull(generator);

        return SetGenerator((outcome, args) => new ValueTask<TValue>(generator(outcome, args)));
    }

    /// <summary>
    /// Adds a result generator for a specific result type.
    /// </summary>
    /// <param name="generator">The value generator.</param>
    /// <returns>The current updated instance.</returns>
    public OutcomeGenerator<TArgs, TValue, TResult> SetGenerator(Func<Outcome<TResult>, TArgs, ValueTask<TValue>> generator)
    {
        Guard.NotNull(generator);

        _generator = generator;

        return this;
    }

    /// <summary>
    /// Creates a handler for the specified generators.
    /// </summary>
    /// <returns>Handler instance or null if no generators are registered.</returns>
    public Func<Outcome<TResult>, TArgs, ValueTask<TValue>>? CreateHandler() => _generator;
}
