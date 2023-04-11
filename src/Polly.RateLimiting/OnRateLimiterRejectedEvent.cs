using Polly.Strategy;

namespace Polly.RateLimiting;

/// <summary>
/// Event raised when a rate limiter rejects the execution.
/// </summary>
public sealed class OnRateLimiterRejectedEvent : SimpleEvent<OnRateLimiterRejectedArguments, OnRateLimiterRejectedEvent>
{
    internal Func<OnRateLimiterRejectedArguments, ValueTask>? CreateHandlerInternal() => CreateHandler();
}
