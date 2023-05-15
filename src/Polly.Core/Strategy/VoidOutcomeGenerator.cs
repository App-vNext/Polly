using System;
using Polly.Strategy;

namespace Polly.Strategy;

/// <summary>
/// A class that generates values based on the void-based <see cref="Outcome"/> and <typeparamref name="TArgs"/>.
/// </summary>
/// <typeparam name="TArgs">The arguments the generator uses.</typeparam>
/// <typeparam name="TValue">The type of the generated value.</typeparam>
public sealed class VoidOutcomeGenerator<TArgs, TValue>
    where TArgs : IResilienceArguments
{
    private Func<Outcome, TArgs, ValueTask<TValue>>? _generator;

    /// <summary>
    /// Gets a value indicating whether the generator is empty.
    /// </summary>
    internal bool IsEmpty => _generator is null;

    /// <summary>
    /// Sets a result generator.
    /// </summary>
    /// <param name="generator">The value generator.</param>
    /// <returns>The current updated instance.</returns>
    public VoidOutcomeGenerator<TArgs, TValue> SetGenerator(Func<Outcome, TArgs, TValue> generator)
    {
        Guard.NotNull(generator);

        return SetGenerator((outcome, args) => new ValueTask<TValue>(generator(outcome, args)));
    }

    /// <summary>
    /// Sets a result generator.
    /// </summary>
    /// <param name="generator">The value generator.</param>
    /// <returns>The current updated instance.</returns>
    public VoidOutcomeGenerator<TArgs, TValue> SetGenerator(Func<Outcome, TArgs, ValueTask<TValue>> generator)
    {
        Guard.NotNull(generator);

        _generator = generator;

        return this;
    }

    /// <summary>
    /// Creates a generator handler.
    /// </summary>
    /// <returns>Handler instance or null if no generators are registered.</returns>
    public Func<Outcome, TArgs, ValueTask<TValue>>? CreateHandler() => _generator;
}
