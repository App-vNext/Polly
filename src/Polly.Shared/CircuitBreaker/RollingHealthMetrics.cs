using System;
using System.Collections.Generic;
using Polly.Utilities;

namespace Polly.Shared.CircuitBreaker
{
    class RollingHealthMetrics : IHealthMetrics
    {
        private readonly long _timesliceDuration;
        private readonly long _windowDuration;
        private readonly Queue<HealthCount> _windows;

        private HealthCount _currentWindow;

        public RollingHealthMetrics(TimeSpan timesliceDuration, short numberOfWindows)
        {
            _timesliceDuration = timesliceDuration.Ticks;

            _windowDuration = _timesliceDuration / numberOfWindows;
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

            while (_windows.Count > 0)
            {
                // If the time between now and when the window started is below the timesliceDuration
                // then it and the rest in the queue are still within the timesliceDuration and should 
                // therefore not be removed from the queue.
                if (now - _windows.Peek().StartedAt < _timesliceDuration)
                    break;

                _windows.Dequeue();
            }
        }
    }
}
