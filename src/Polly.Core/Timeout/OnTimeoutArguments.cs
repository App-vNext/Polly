using Polly.Strategy;

namespace Polly.Timeout;

#pragma warning disable CA1815 // Equals not overridden because this class is just a data holder.

/// <summary>
/// Arguments used by the timeout strategy to notify that timeout occurred.
/// </summary>
public readonly struct OnTimeoutArguments : IResilienceArguments
{
    internal OnTimeoutArguments(ResilienceContext context, Exception exception, TimeSpan timeout)
    {
        Context = context;
        Exception = exception;
        Timeout = timeout;
    }

    /// <inheritdoc/>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the original exception that caused the timeout.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the timeout value assigned.
    /// </summary>
    public TimeSpan Timeout { get; }
}
