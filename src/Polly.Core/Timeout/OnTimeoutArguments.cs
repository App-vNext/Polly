using Polly.Strategy;

namespace Polly.Timeout;

/// <summary>
/// Arguments used by the timeout strategy to notify that timeout occurred.
/// </summary>
/// <param name="Context">The context associated with the execution of user-provided callback.</param>
/// <param name="Exception">The original exception that caused the timeout.</param>
/// <param name="Timeout">The timeout value assigned.</param>
public readonly record struct OnTimeoutArguments(ResilienceContext Context, Exception Exception, TimeSpan Timeout) : IResilienceArguments;
