namespace Polly.CircuitBreaker.Health;

internal readonly record struct HealthInfo(int Throughput, double FailureRate)
{
    public static HealthInfo Create(int successes, int failures)
    {
        var total = successes + failures;
        if (total == 0)
        {
            return new HealthInfo(0, 0);
        }

        return new(total, failures / (double)total);
    }
}
