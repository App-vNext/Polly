using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class SingleHealthMetrics : HealthMetrics
    {
        public SingleHealthMetrics(TimeSpan samplingDuration) : base(samplingDuration) {}

        public override void Reset_NeedsLock() => _currentHealth = null;

        public override HealthCount GetHealthCount_NeedsLock()
        {
            ActualiseCurrentMetric_NeedsLock();

            return _currentHealth;
        }

        protected override void ActualiseCurrentMetric_NeedsLock()
        {
            long now = SystemClock.UtcNow().Ticks;
            if (_currentHealth == null || now - _currentHealth.StartedAt >= _samplingDuration)
            {
                _currentHealth = new HealthCount { StartedAt = now };
            }
        }
    }
}
