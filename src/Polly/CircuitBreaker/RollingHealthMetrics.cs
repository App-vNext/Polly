using System;
using System.Collections.Generic;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class RollingHealthMetrics : HealthMetrics
    {
        private readonly long _windowDuration;
        private readonly Queue<HealthCount> _windows;

        public RollingHealthMetrics(TimeSpan samplingDuration, short numberOfWindows) : base(samplingDuration)
        {
            _windowDuration = _samplingDuration / numberOfWindows;
            _windows = new Queue<HealthCount>(numberOfWindows + 1);
        }

        public override void Reset_NeedsLock()
        {
            _currentHealth = null;
            _windows.Clear();
        }

        public override HealthCount GetHealthCount_NeedsLock()
        {
            ActualiseCurrentMetric_NeedsLock();

            int successes = 0;
            int failures = 0;
            foreach (var window in _windows)
            {
                successes += window.Successes;
                failures += window.FailuresFromClosedState;
            }

            return new HealthCount
            {
                Successes = successes,
                FailuresFromClosedState = failures,
                StartedAt = _windows.Peek().StartedAt
            };
        }

        protected override void ActualiseCurrentMetric_NeedsLock()
        {
            long now = SystemClock.UtcNow().Ticks;
            if (_currentHealth == null || now - _currentHealth.StartedAt >= _windowDuration)
            {
                _currentHealth = new HealthCount { StartedAt = now };
                _windows.Enqueue(_currentHealth);
            }

            while (_windows.Count > 0 && (now - _windows.Peek().StartedAt >= _samplingDuration))
            {
                _windows.Dequeue();
            }
        }
    }
}
