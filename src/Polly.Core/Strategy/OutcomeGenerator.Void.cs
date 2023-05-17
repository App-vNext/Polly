namespace Polly.Strategy;

public sealed partial class OutcomeGenerator<TArgs, TValue>
    where TArgs : IResilienceArguments
{
    /// <summary>
    /// Sets a result generator for void-based results.
    /// </summary>
    /// <param name="generator">The value generator.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="generator"/> is <see langword="null"/>.</exception>
    public OutcomeGenerator<TArgs, TValue> SetVoidGenerator(Func<Outcome, TArgs, TValue> generator)
    {
        Guard.NotNull(generator);

        return ConfigureVoidGenerator(g => g.SetGenerator((outcome, args) => generator(outcome, args)));
    }

    /// <summary>
    /// Sets a result generator for void-based results.
    /// </summary>
    /// <param name="generator">The value generator.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="generator"/> is <see langword="null"/>.</exception>
    public OutcomeGenerator<TArgs, TValue> SetVoidGenerator(Func<Outcome, TArgs, ValueTask<TValue>> generator)
    {
        Guard.NotNull(generator);

        return ConfigureVoidGenerator(g => g.SetGenerator((outcome, args) => generator(outcome, args)));
    }

    /// <summary>
    /// Sets a result generator for void-based results.
    /// </summary>
    /// <param name="generator">The generator builder.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="generator"/> is <see langword="null"/>.</exception>
    public OutcomeGenerator<TArgs, TValue> SetVoidGenerator(VoidOutcomeGenerator<TArgs, TValue> generator)
    {
        Guard.NotNull(generator);

        _generators[typeof(VoidResult)] = (generator, generator.CreateHandler);

        return this;
    }

    /// <summary>
    /// Sets a result generator for void-based results.
    /// </summary>
    /// <param name="configure">The callbacks that configures the generator.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    internal OutcomeGenerator<TArgs, TValue> ConfigureVoidGenerator(Action<VoidOutcomeGenerator<TArgs, TValue>> configure)
    {
        Guard.NotNull(configure);

        if (!_generators.TryGetValue(typeof(VoidResult), out var generator))
        {
            SetVoidGenerator(new VoidOutcomeGenerator<TArgs, TValue>());
            generator = _generators[typeof(VoidResult)];
        }

        configure((VoidOutcomeGenerator<TArgs, TValue>)generator.generator);
        return this;
    }
}
