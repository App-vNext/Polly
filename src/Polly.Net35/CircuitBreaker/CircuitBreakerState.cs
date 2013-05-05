using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class CircuitBreakerState : ICircuitBreakerState
    {
        private readonly TimeSpan _durationOfBreak;
        private readonly int _exceptionsAllowedBeforeBreaking;
        private int _count;
        private DateTime _blockedTill;
        private Exception _lastException;
        private readonly object _lock = new object();

        public CircuitBreakerState(int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak)
        {
            _durationOfBreak = durationOfBreak;
            _exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;

            Reset();
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

        public bool IsBroken
        {
            get
            {
                using (TimedLock.Lock(_lock))
                {
                    return SystemClock.UtcNow() < _blockedTill;
                }
            }
        }

        public void Reset()
        {
            using (TimedLock.Lock(_lock))
            {
                _count = 0;
                _blockedTill = DateTime.MinValue;

                _lastException = new InvalidOperationException("This exception should never be thrown");
            }
        }

        public void TryBreak(Exception ex)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = ex;

                _count += 1;
                if (_count >= _exceptionsAllowedBeforeBreaking)
                {
                    BreakTheCircuit();
                }
            }
        }

        void BreakTheCircuit()
        {
            var willDurationTakeUsPastDateTimeMaxValue = _durationOfBreak > DateTime.MaxValue - SystemClock.UtcNow();
            _blockedTill = willDurationTakeUsPastDateTimeMaxValue ?
                               DateTime.MaxValue :
                               SystemClock.UtcNow() + _durationOfBreak;
        }
    }
}