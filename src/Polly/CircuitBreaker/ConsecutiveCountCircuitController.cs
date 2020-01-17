using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class ConsecutiveCountCircuitController<TResult> : CircuitStateController<TResult>
    {
        private readonly int _failureThreshold;
        private int _consecutiveFailuresFromClosedState;
        private int _consecutiveFailuresFromHalfOpenState;

        public ConsecutiveCountCircuitController(
            int failureThreshold,
            Func<int, TimeSpan> factoryForNextBreakDuration, 
            Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> onBreak, 
            Action<Context> onReset, 
            Action onHalfOpen
            ) : base(factoryForNextBreakDuration, onBreak, onReset, onHalfOpen)
        {
            _failureThreshold = failureThreshold;
        }

        public override void OnCircuitReset(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _consecutiveFailuresFromClosedState = 0;
                _consecutiveFailuresFromHalfOpenState = 0;

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
                        _consecutiveFailuresFromClosedState = 0;
                        break;

                    case CircuitState.Open:
                    case CircuitState.Isolated:
                        break; // A successful call result may arrive when the circuit is open, if it was placed before the circuit broke.  We take no action; only time passing governs transitioning from Open to HalfOpen state.

                    default:
                        throw new InvalidOperationException("Unhandled CircuitState.");
                }
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
                        _consecutiveFailuresFromHalfOpenState++;
                        Break_NeedsLock(_consecutiveFailuresFromHalfOpenState, context);
                        return;

                    case CircuitState.Closed:
                        _consecutiveFailuresFromClosedState++;
                        if (_consecutiveFailuresFromClosedState >= _failureThreshold)
                        {
                            Break_NeedsLock(0, context);
                        }
                        break;

                    case CircuitState.Open:
                    case CircuitState.Isolated:
                        break; // A failure call result may arrive when the circuit is open, if it was placed before the circuit broke.  We take no action; we do not want to duplicate-signal onBreak; we do not want to extend time for which the circuit is broken.  We do not want to mask the fact that the call executed (as replacing its result with a Broken/IsolatedCircuitException would do).

                    default:
                        throw new InvalidOperationException("Unhandled CircuitState.");
                }
            }
        }
    }
}
