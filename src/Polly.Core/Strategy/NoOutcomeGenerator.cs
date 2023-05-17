namespace Polly.Strategy;

/// <summary>
/// Generator that generates values based on the <typeparamref name="TArgs"/>.
/// </summary>
/// <typeparam name="TArgs">The arguments the generator uses.</typeparam>
/// <typeparam name="TValue">The type of the generated value.</typeparam>
public sealed class NoOutcomeGenerator<TArgs, TValue>
    where TArgs : IResilienceArguments
{
    private Func<TArgs, ValueTask<TValue>>? _generator;

    /// <summary>
    /// Gets a value indicating whether the generator is empty.
    /// </summary>
    internal bool IsEmpty => _generator is null;

    /// <summary>
    /// Adds a result generator.
    /// </summary>
    /// <param name="generator">The generator to determine if the result should be retried.</param>
    /// <returns>The current updated instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="generator"/> is <see langword="null"/>.</exception>
    public NoOutcomeGenerator<TArgs, TValue> SetGenerator(Func<TArgs, TValue> generator)
    {
        Guard.NotNull(generator);

        _generator = c => new ValueTask<TValue>(generator(c));
        return this;
    }

    /// <summary>
    /// Sets the asynchronous callback that dynamically generates a timeout for a given execution.
    /// </summary>
    /// <param name="generator">The timeout generator.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="generator"/> is <see langword="null"/>.</exception>
    public NoOutcomeGenerator<TArgs, TValue> SetGenerator(Func<TArgs, ValueTask<TValue>> generator)
    {
        Guard.NotNull(generator);

        _generator = generator;
        return this;
    }

    /// <summary>
    /// Creates a handler for the specified generators.
    /// </summary>
    /// <param name="defaultValue">The default value returned by the generator.</param>
    /// <param name="valueValidator">The validator that determines if the generated value is valid.</param>
    /// <returns>Handler instance or null if no generators are registered.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="valueValidator"/> is <see langword="null"/>.</exception>
    public Func<TArgs, ValueTask<TValue>>? CreateHandler(TValue defaultValue, Predicate<TValue> valueValidator)
    {
        Guard.NotNull(valueValidator);

        if (_generator == null)
        {
            return null;
        }

        var generator = _generator;

        return async args =>
        {
            var value = await generator(args).ConfigureAwait(args.Context.ContinueOnCapturedContext);

            if (!valueValidator(value))
            {
                return defaultValue;
            }

            return value;
        };
    }
}
