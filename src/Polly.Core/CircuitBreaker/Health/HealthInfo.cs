namespace Polly.CircuitBreaker.Health;

internal readonly record struct HealthInfo(int Throughput, double FailureRate)
{
    public static HealthInfo Create(int successes, int failures)
    {
        if (successes + failures == 0)
        {
          return new HealthInfo(0, 0);
        }

       double failureRate = failures / (double)(successes + failures);
       return new HealthInfo(successes + failures, failureRate);
    }
}
