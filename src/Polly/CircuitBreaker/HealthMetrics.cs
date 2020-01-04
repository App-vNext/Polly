using System;

namespace Polly.CircuitBreaker
{
    internal abstract class HealthMetrics
    {
        public void IncrementSuccess_NeedsLock()
        {
            ActualiseCurrentMetric_NeedsLock();

            _currentHealth.Successes++;
        }

        public void IncrementClosedFailure_NeedsLock()
        {
            ActualiseCurrentMetric_NeedsLock();

            _currentHealth.FailuresFromClosedState++;
        }

        public void IncrementHalfOpenFailure_NeedsLock()
        {
            ActualiseCurrentMetric_NeedsLock();

            _currentHealth.FailuresFromHalfOpenState++;
        }

        protected HealthMetrics(TimeSpan samplingDuration) => _samplingDuration = samplingDuration.Ticks;

        protected readonly long _samplingDuration;
        protected HealthCount _currentHealth;

        protected abstract void ActualiseCurrentMetric_NeedsLock();
        public abstract void Reset_NeedsLock();
        public abstract HealthCount GetHealthCount_NeedsLock();
    }
}
