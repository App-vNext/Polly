using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class AdvancedCircuitController<TResult> : CircuitStateController<TResult>
    {
        private const short NumberOfWindows = 10;
        internal static readonly long ResolutionOfCircuitTimer = TimeSpan.FromMilliseconds(20).Ticks;

        private readonly HealthMetrics _metrics;
        private readonly double _failureThreshold;
        private readonly int _minimumThroughput;

        public AdvancedCircuitController(
            double failureThreshold, 
            TimeSpan samplingDuration, 
            int minimumThroughput,
            Func<int, TimeSpan> factoryForNextBreakDuration,
            Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> onBreak, 
            Action<Context> onReset, 
            Action onHalfOpen
            ) : base(factoryForNextBreakDuration, onBreak, onReset, onHalfOpen)
        {
            _metrics = samplingDuration.Ticks < ResolutionOfCircuitTimer * NumberOfWindows
                ? (HealthMetrics)new SingleHealthMetrics(samplingDuration)
                : (HealthMetrics)new RollingHealthMetrics(samplingDuration, NumberOfWindows);

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
                _metrics?.Reset_NeedsLock();

                ResetInternal_NeedsLock(context);
            }
        }

        public override void OnActionSuccess(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                switch (_circuitState)
                {
                    case CircuitState.HalfOpen:
                        OnCircuitReset(context);
                        break;

                    case CircuitState.Closed:
                        break;

                    case CircuitState.Open:
                    case CircuitState.Isolated:
                        break; // A successful call result may arrive when the circuit is open, if it was placed before the circuit broke.  We take no special action; only time passing governs transitioning from Open to HalfOpen state.

                    default:
                        throw new InvalidOperationException("Unhandled CircuitState.");
                }

                _metrics.IncrementSuccess_NeedsLock();
            }
        }

        public override void OnActionFailure(DelegateResult<TResult> outcome, Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastOutcome = outcome;

                switch (_circuitState)
                {
                    case CircuitState.HalfOpen:
                        _metrics.IncrementHalfOpenFailure_NeedsLock();
                        var halfOpenFailuresSinceLastReset = _metrics.GetHealthCount_NeedsLock().FailuresFromHalfOpenState;

                        Break_NeedsLock(halfOpenFailuresSinceLastReset, context);
                        return;

                    case CircuitState.Closed:
                        _metrics.IncrementClosedFailure_NeedsLock();
                        var healthCount = _metrics.GetHealthCount_NeedsLock();

                        int throughput = healthCount.Total;
                        if (throughput >= _minimumThroughput && ((double)healthCount.FailuresFromClosedState) / throughput >= _failureThreshold)
                        {
                            Break_NeedsLock(0, context);
                        }
                        break;

                    case CircuitState.Open:
                    case CircuitState.Isolated:
                        _metrics.IncrementClosedFailure_NeedsLock();
                        break; // A failure call result may arrive when the circuit is open, if it was placed before the circuit broke.  We take no action beyond tracking the metric; we do not want to duplicate-signal onBreak; we do not want to extend time for which the circuit is broken.  We do not want to mask the fact that the call executed (as replacing its result with a Broken/IsolatedCircuitException would do).

                    default:
                        throw new InvalidOperationException("Unhandled CircuitState.");
                }
            }
        }
    }
}
