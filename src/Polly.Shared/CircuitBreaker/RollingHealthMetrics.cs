using System;
using System.Collections.Generic;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class RollingHealthMetrics : IHealthMetrics
    {
        private readonly long _samplingDuration;
        private readonly long _windowDuration;
        private readonly Queue<HealthCount> _windows;

        private HealthCount _currentWindow;

        public RollingHealthMetrics(TimeSpan samplingDuration, short numberOfWindows)
        {
            _samplingDuration = samplingDuration.Ticks;

            _windowDuration = _samplingDuration / numberOfWindows;
            _windows = new Queue<HealthCount>(numberOfWindows + 1);
        }

        public void IncrementSuccess_NeedsLock()
        {
            ActualiseCurrentMetric_NeedsLock();

            _currentWindow.Successes++;
        }

        public void IncrementFailure_NeedsLock()
        {
            ActualiseCurrentMetric_NeedsLock();

            _currentWindow.Failures++;
        }

        public void Reset_NeedsLock()
        {
            _currentWindow = null;
            _windows.Clear();
        }

        public HealthCount GetHealthCount_NeedsLock()
        {
            ActualiseCurrentMetric_NeedsLock();

            int successes = 0;
            int failures = 0;
            foreach (var window in _windows)
            {
                successes += window.Successes;
                failures += window.Failures;
            }

            return new HealthCount
            {
                Successes = successes,
                Failures = failures,
                StartedAt = _windows.Peek().StartedAt
            };
        }

        private void ActualiseCurrentMetric_NeedsLock()
        {
            long now = SystemClock.UtcNow().Ticks;
            if (_currentWindow == null || now - _currentWindow.StartedAt >= _windowDuration)
            {
                _currentWindow = new HealthCount { StartedAt = now };
                _windows.Enqueue(_currentWindow);
            }

            while (_windows.Count > 0 && (now - _windows.Peek().StartedAt >= _samplingDuration))
                _windows.Dequeue();
        }
    }
}
