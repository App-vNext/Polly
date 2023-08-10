namespace Polly.CircuitBreaker;

internal static class CircuitBreakerConstants
{
    public const string DefaultName = "CircuitBreaker";

    public const string OnCircuitClosed = "OnCircuitClosed";

    public const string OnHalfOpenEvent = "OnCircuitHalfOpened";

    public const string OnCircuitOpened = "OnCircuitOpened";

    public const double DefaultFailureRatio = 0.1;

    public const int DefaultMinimumThroughput = 100;

    public const int MinimumValidThroughput = 2;

    public static readonly TimeSpan DefaultBreakDuration = TimeSpan.FromSeconds(5);

    public static readonly TimeSpan DefaultSamplingDuration = TimeSpan.FromSeconds(30);
}
