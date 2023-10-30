namespace Polly.CircuitBreaker.Health;

internal readonly record struct HealthInfo(int Throughput, double FailureRate, int FailureCount)
{
    public static HealthInfo Create(int successes, int failures)
    {
        var total = successes + failures;
        if (total == 0)
        {
            return new HealthInfo(0, 0, failures);
        }

        return new(total, failures / (double)total, failures);
    }
}
