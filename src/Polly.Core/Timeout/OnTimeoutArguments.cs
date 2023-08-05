namespace Polly.Timeout;

/// <summary>
/// Arguments used by the timeout strategy to notify that a timeout occurred.
/// </summary>
public sealed class OnTimeoutArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnTimeoutArguments"/> class.
    /// </summary>
    /// <param name="context">The context associated with the execution of a user-provided callback.</param>
    /// <param name="exception">The original exception that caused the timeout.</param>
    /// <param name="timeout">The timeout value assigned.</param>
    public OnTimeoutArguments(ResilienceContext context, Exception exception, TimeSpan timeout)
    {
        Context = context;
        Exception = exception;
        Timeout = timeout;
    }

    /// <summary>
    /// Gets the context associated with the execution of a user-provided callback.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets hte original exception that caused the timeout.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the timeout value assigned.
    /// </summary>
    public TimeSpan Timeout { get; }
}
