namespace Polly.CircuitBreaker;

internal static class CircuitBreakerConstants
{
    public const string StrategyType = "CircuitBreaker";

    public const string OnResetEvent = "OnCircuitReset";

    public const string OnHalfOpenEvent = "OnCircuitHalfOpen";

    public const string OnBreakEvent = "OnCircuitBreak";

    public const double DefaultAdvancedFailureThreshold = 0.1;

    public const int DefaultMinimumThroughput = 100;

    public const int MinMinimumThroughput = 2;

    public const int DefaultFailureThreshold = 100;

    public static readonly TimeSpan DefaultBreakDuration = TimeSpan.FromSeconds(5);

    public static readonly TimeSpan DefaultSamplingDuration = TimeSpan.FromSeconds(30);
}
