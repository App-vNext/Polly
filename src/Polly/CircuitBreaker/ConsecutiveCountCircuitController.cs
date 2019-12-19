using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class ConsecutiveCountCircuitController<TResult> : CircuitStateController<TResult>
    {
        private readonly int _exceptionsAllowedBeforeBreaking;
        private int _consecutiveFailures;
        private int _consecutiveHalfOpenFailures;

        public ConsecutiveCountCircuitController(
            int exceptionsAllowedBeforeBreaking,
            Func<int, TimeSpan> factoryForNextBreakDuration, 
            Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> onBreak, 
            Action<Context> onReset, 
            Action onHalfOpen
            ) : base(factoryForNextBreakDuration, onBreak, onReset, onHalfOpen)
        {
            _exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
        }

        public override void OnCircuitReset(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _consecutiveFailures = 0;
                _consecutiveHalfOpenFailures = 0;

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
                        _consecutiveFailures = 0;
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
                        _consecutiveHalfOpenFailures++;
                        Break_NeedsLock(_consecutiveHalfOpenFailures, context);
                        return;

                    case CircuitState.Closed:
                        _consecutiveFailures++;
                        if (_consecutiveFailures >= _exceptionsAllowedBeforeBreaking)
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
