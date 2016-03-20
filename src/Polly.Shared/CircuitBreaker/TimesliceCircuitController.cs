using System;
using System.Collections.Generic;
using System.Text;
using Polly.CircuitBreaker;
using Polly.Utilities;

namespace Polly.Shared.CircuitBreaker
{
    internal class TimesliceCircuitController : ICircuitController
    {
        private readonly TimeSpan _durationOfBreak;

        private readonly long _timesliceDuration;

        private readonly double _failureThreshold;
        private readonly int _minimumThroughput;

        private HealthMetric _metric;

        private DateTime _blockedTill;
        private CircuitState _circuitState;
        private Exception _lastException;

        private readonly Action<Exception, TimeSpan, Context> _onBreak;
        private readonly Action<Context> _onReset;
        private readonly Action _onHalfOpen;
        private readonly object _lock = new object();

        private class HealthMetric // Could this be a struct (admittedly a mutable one), to avoid allocations and de-allocations?  Or, if only one metric is ever retained at a time, it could be removed altogether and the properties incorporated in to the parent class.
        {
            public int Successes { get; set; }
            public int Failures { get; set; }
            public long StartedAt { get; set; }
        }

        public TimesliceCircuitController(TimeSpan timesliceDuration, double failureThreshold, int minimumThroughput, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            _timesliceDuration = timesliceDuration.Ticks;
            _failureThreshold = failureThreshold;
            _minimumThroughput = minimumThroughput;

            _durationOfBreak = durationOfBreak;

            _onBreak = onBreak;
            _onReset = onReset;
            _onHalfOpen = onHalfOpen;

            ResetInternal_NeedsLock(); // Lock not needed when constructing.
        }

        public CircuitState CircuitState
        {
            get
            {
                using (TimedLock.Lock(_lock))
                {
                    if (_circuitState == CircuitState.Open && !IsInAutomatedBreak_NeedsLock)
                    {
                        _circuitState = CircuitState.HalfOpen;
                        _onHalfOpen();
                    }
                    return _circuitState;
                }
            }
        }

        public Exception LastException
        {
            get
            {
                using (TimedLock.Lock(_lock))
                {
                    return _lastException;
                }
            }
        }

        private bool IsInAutomatedBreak_NeedsLock
        {
            get
            {
                return SystemClock.UtcNow() < _blockedTill;
            }
        }

        public void Isolate()
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = new IsolatedCircuitException("The circuit is manually held open and is not allowing calls.");
                BreakFor_NeedsLock(TimeSpan.MaxValue, Context.Empty);
                _circuitState = CircuitState.Isolated;
            }
        }

        void ResetInternal_NeedsLock()
        {
            _metric = null;

            _blockedTill = DateTime.MinValue;
            _circuitState = CircuitState.Closed;

            _lastException = new InvalidOperationException("This exception should never be thrown");
        }

        public void Reset()
        {
            Reset(Context.Empty);
        }

        private void Reset(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                CircuitState priorState = _circuitState;

                ResetInternal_NeedsLock();

                if (priorState != CircuitState.Closed)
                {
                    _onReset(context ?? Context.Empty);
                }
            }
        }

        private void ActualiseCurrentMetric_NeedsLock()
        {
            // (future enhancement) Any operation in this method disposing of a _metric could emit it to a delegate, for health-monitoring capturing ...

            long now = SystemClock.UtcNow().Ticks;

            if (_metric == null || now - _metric.StartedAt > _timesliceDuration)
            {
                _metric = new HealthMetric { StartedAt = now };
            }
           
        }

        public void OnActionSuccess(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                ActualiseCurrentMetric_NeedsLock();
                _metric.Successes++;
            }
        }

        public void OnActionFailure(Exception ex, Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = ex;

                ActualiseCurrentMetric_NeedsLock();
                _metric.Failures++;

                int throughput = _metric.Failures + _metric.Successes;
                if (throughput > _minimumThroughput && ((double)_metric.Failures) / throughput > _failureThreshold)
                {
                    BreakFor_NeedsLock(_durationOfBreak, context);
                }
            }
        }

        void BreakFor_NeedsLock(TimeSpan durationOfBreak, Context context)
        {
            bool willDurationTakeUsPastDateTimeMaxValue = durationOfBreak > DateTime.MaxValue - SystemClock.UtcNow();
            _blockedTill = willDurationTakeUsPastDateTimeMaxValue
                ? DateTime.MaxValue
                : SystemClock.UtcNow() + durationOfBreak;
            _circuitState = CircuitState.Open;

            _onBreak(_lastException, durationOfBreak, context ?? Context.Empty);
        }

    }

}
