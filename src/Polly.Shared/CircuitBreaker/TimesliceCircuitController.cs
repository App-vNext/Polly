using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class TimesliceCircuitController : CircuitStateController
    {
        private readonly long _timesliceDuration;
        private readonly double _failureThreshold;
        private readonly int _minimumThroughput;

        private HealthMetric _metric;

        private class HealthMetric // Perhaps this could be a struct (admittedly a mutable one), to avoid class allocations and de-allocations?  Or, if only one metric at a time is ever retained, it could be removed altogether and the properties incorporated in to the parent class.
        {
            public int Successes { get; set; }
            public int Failures { get; set; }
            public long StartedAt { get; set; }
        }

        public TimesliceCircuitController(TimeSpan timesliceDuration, double failureThreshold, int minimumThroughput, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen) : base(durationOfBreak, onBreak, onReset, onHalfOpen)
        {
            _timesliceDuration = timesliceDuration.Ticks;
            _failureThreshold = failureThreshold;
            _minimumThroughput = minimumThroughput;
        }

        public override void OnCircuitReset(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _metric = null;

                ResetInternal_NeedsLock(context);
            }
        }

        private void ActualiseCurrentMetric_NeedsLock()
        {
            // (future enhancement) Any operation in this method disposing of an existing _metric could emit it to a delegate, for health-monitoring capturing ...

            long now = SystemClock.UtcNow().Ticks;

            if (_metric == null || now - _metric.StartedAt > _timesliceDuration)
            {
                _metric = new HealthMetric { StartedAt = now };
            }
        }

        public override void OnActionSuccess(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                if (_circuitState == CircuitState.HalfOpen) { OnCircuitReset(context); }

                ActualiseCurrentMetric_NeedsLock();
                _metric.Successes++;
            }
        }

        public override void OnActionFailure(Exception ex, Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = ex;

                if (_circuitState == CircuitState.HalfOpen) { Break_NeedsLock(context); }

                ActualiseCurrentMetric_NeedsLock();
                _metric.Failures++;

                int throughput = _metric.Failures + _metric.Successes;
                if (throughput > _minimumThroughput && ((double)_metric.Failures) / throughput > _failureThreshold)
                {
                    Break_NeedsLock(context);
                }
                
            }
        }
    }
}
