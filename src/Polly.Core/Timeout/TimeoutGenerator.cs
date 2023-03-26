namespace Polly.Timeout;

/// <summary>
/// Generator for the timeout for a given execution of a user-provided callback.
/// </summary>
public sealed class TimeoutGenerator
{
    private Func<TimeoutGeneratorArguments, ValueTask<TimeSpan>>? _generator;

    /// <summary>
    /// Sets the callback that dynamically generates a timeout for a given execution.
    /// </summary>
    /// <param name="generator">The timeout generator.</param>
    /// <returns>This instance.</returns>
    public TimeoutGenerator SetTimeout(Func<TimeoutGeneratorArguments, TimeSpan> generator)
    {
        Guard.NotNull(generator);

        _generator = c => new ValueTask<TimeSpan>(generator(c));
        return this;
    }

    /// <summary>
    /// Sets the asynchronous callback that dynamically generates a timeout for a given execution.
    /// </summary>
    /// <param name="generator">The timeout generator.</param>
    /// <returns>This instance.</returns>
    public TimeoutGenerator SetTimeout(Func<TimeoutGeneratorArguments, ValueTask<TimeSpan>> generator)
    {
        Guard.NotNull(generator);

        _generator = generator;
        return this;
    }

    internal Func<TimeoutGeneratorArguments, ValueTask<TimeSpan>>? CreateHandler() => _generator;
}
