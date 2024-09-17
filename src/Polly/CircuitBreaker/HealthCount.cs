namespace Polly.CircuitBreaker;

internal sealed class HealthCount
{
    public int Successes { get; set; }

    public int Failures { get; set; }

    public int Total => Successes + Failures;

    public long StartedAt { get; set; }
}
