namespace Polly.Timeout;

/// <summary>
/// Generator for the timeout for a given execution of a user-provided callback.
/// </summary>
public sealed class TimeoutGenerator
{
    private Func<TimeoutGeneratorArguments, ValueTask<TimeSpan>> _generator = _ => new ValueTask<TimeSpan>(TimeoutConstants.InfiniteTimeout);

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutGenerator"/> class.
    /// </summary>
    public TimeoutGenerator()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutGenerator"/> class.
    /// </summary>
    /// <param name="timeout">The timeout that applies to all executions.</param>
    public TimeoutGenerator(TimeSpan timeout)
    {
        TimeoutUtil.ValidateTimeout(timeout);

        SetTimeout(timeout);
    }

    /// <summary>
    /// Sets the timeout that applies to all executions of the user-provided callback.
    /// </summary>
    /// <param name="timeout">The timeout value.</param>
    /// <returns>This instance.</returns>
    public TimeoutGenerator SetTimeout(TimeSpan timeout)
    {
        TimeoutUtil.ValidateTimeout(timeout);

        _generator = _ => new ValueTask<TimeSpan>(timeout);
        return this;
    }

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

    internal Func<TimeoutGeneratorArguments, ValueTask<TimeSpan>> CreateHandler() => _generator;
}
