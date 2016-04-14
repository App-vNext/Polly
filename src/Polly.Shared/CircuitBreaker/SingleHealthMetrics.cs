using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class SingleHealthMetrics : IHealthMetrics
    {
        private readonly long _samplingDuration;

        private HealthCount _current;

        public SingleHealthMetrics(TimeSpan samplingDuration)
        {
            _samplingDuration = samplingDuration.Ticks;
        }

        public void IncrementSuccess_NeedsLock()
        {
            ActualiseCurrentMetric_NeedsLock();

            _current.Successes++;
        }

        public void IncrementFailure_NeedsLock()
        {
            ActualiseCurrentMetric_NeedsLock();

            _current.Failures++;
        }

        public void Reset_NeedsLock()
        {
            _current = null;
        }

        public HealthCount GetHealthCount_NeedsLock()
        {
            ActualiseCurrentMetric_NeedsLock();

            return _current;
        }

        private void ActualiseCurrentMetric_NeedsLock()
        {
            long now = SystemClock.UtcNow().Ticks;
            if (_current == null || now - _current.StartedAt >= _samplingDuration)
            {
                _current = new HealthCount { StartedAt = now };
            }
        }
    }
}
