namespace Polly.Timeout;

/// <summary>
/// Arguments used by the timeout strategy to notify that a timeout occurred.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
/// <param name="Exception">The original exception that caused the timeout.</param>
/// <param name="Timeout">The timeout value assigned.</param>
public record OnTimeoutArguments(ResilienceContext Context, Exception Exception, TimeSpan Timeout);
