using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class AdvancedCircuitController<TResult> : CircuitStateController<TResult>
    {
        private const short NumberOfWindows = 10;
        internal static readonly long ResolutionOfCircuitTimer = TimeSpan.FromMilliseconds(20).Ticks;

        private readonly IHealthMetrics _metrics;
        private readonly double _failureThreshold;
        private readonly int _minimumThroughput;

        public AdvancedCircuitController(
            double failureThreshold, 
            TimeSpan samplingDuration, 
            int minimumThroughput, 
            TimeSpan durationOfBreak, 
            Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, 
            Action<Context> onReset, 
            Action onHalfOpen
            ) : base(durationOfBreak, onBreak, onReset, onHalfOpen)
        {
            _metrics = samplingDuration.Ticks < ResolutionOfCircuitTimer * NumberOfWindows
                ? (IHealthMetrics)new SingleHealthMetrics(samplingDuration)
                : (IHealthMetrics)new RollingHealthMetrics(samplingDuration, NumberOfWindows);

            _failureThreshold = failureThreshold;
            _minimumThroughput = minimumThroughput;
        }

        public override void OnCircuitReset(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                // Is only null during initialization of the current class
                // as the variable is not set, before the base class calls
                // current method from constructor.
                if (_metrics != null)
                    _metrics.Reset_NeedsLock();

                ResetInternal_NeedsLock(context);
            }
        }

        public override void OnActionSuccess(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                if (_circuitState == CircuitState.HalfOpen) { OnCircuitReset(context); }

                _metrics.IncrementSuccess_NeedsLock();
            }
        }

        public override void OnActionFailure(DelegateResult<TResult> outcome, Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastOutcome = outcome;

                if (_circuitState == CircuitState.HalfOpen)
                {
                    Break_NeedsLock(context);
                    return;
                }

                _metrics.IncrementFailure_NeedsLock();
                var healthCount = _metrics.GetHealthCount_NeedsLock();

                int throughput = healthCount.Total;
                if (throughput >= _minimumThroughput && ((double)healthCount.Failures) / throughput >= _failureThreshold)
                {
                    Break_NeedsLock(context);
                }

            }
        }


    }
}
