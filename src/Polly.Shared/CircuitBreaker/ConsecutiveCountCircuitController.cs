using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class ConsecutiveCountCircuitController : CircuitStateController
    {
        private readonly int _exceptionsAllowedBeforeBreaking;
        private int _count;

        public ConsecutiveCountCircuitController(int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen) : base(durationOfBreak, onBreak, onReset, onHalfOpen)
        {
            _exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
        }

        public override void OnCircuitReset()
        {
            _count = 0;
        }

        public override void OnActionSuccess(Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                if (_circuitState == CircuitState.HalfOpen) { Reset(context); }
            }
        }

        public override void OnActionFailure(Exception ex, Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = ex;

                _count += 1;
                if (_count >= _exceptionsAllowedBeforeBreaking)
                {
                    Break_NeedsLock(context);
                }
            }
        }
    }
}
