using Polly.CircuitBreaker;

namespace Polly.CircuitBreaker
{
    internal interface IHealthMetrics
    {
        void IncrementSuccess_NeedsLock();
        void IncrementFailure_NeedsLock();

        void Reset_NeedsLock();

        HealthCount GetHealthCount_NeedsLock();
    }
}
