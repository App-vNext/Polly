using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class ConsecutiveCountCircuitController<TResult> : CircuitStateController<TResult>
    {
        private readonly int _exceptionsAllowedBeforeBreaking;
        private int _count;

        public ConsecutiveCountCircuitController(
            int exceptionsAllowedBeforeBreaking, 
            TimeSpan durationOfBreak, 
            Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, 
            Action<Context> onReset, 
            Action onHalfOpen
            ) : base(durationOfBreak, onBreak, onReset, onHalfOpen)
        {
            _exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
        }

        public override void OnCircuitReset(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _count = 0;

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
                        _count = 0;
                        break;
                }
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

                _count += 1;
                if (_count >= _exceptionsAllowedBeforeBreaking)
                {
                    Break_NeedsLock(context);
                }
            }
        }
    }
}
