namespace Polly.Simmy.Latency;

/// <summary>
/// Arguments used by the latency chaos strategy to notify that a delayed occurred.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
/// <param name="Latency">The timeout value assigned.</param>
public readonly record struct OnDelayedArguments(ResilienceContext Context, TimeSpan Latency);
