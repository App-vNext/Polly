using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class ServiceLevelCircuitBreakerState : ICircuitBreakerState
    {
        private readonly TimeSpan _durationOfBreak;
        private readonly double _serviceLevelPercent;
        private int _successCount;
        private int _failCount;
        private DateTime _blockedTill;
        private Exception _lastException;
        private readonly object _lock = new object();

        public ServiceLevelCircuitBreakerState(double serviceLevelPercent, TimeSpan durationOfBreak)
        {
            _durationOfBreak = durationOfBreak;
            _serviceLevelPercent = serviceLevelPercent;
            Initialize();
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
                _successCount += 1;
                Initialize();
            }
        }

        public void TryBreak(Exception ex)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = ex;
                _failCount += 1;

                var currentServiceLevel = ((double)_successCount / (_successCount + _failCount)) * 100;
                if (currentServiceLevel < _serviceLevelPercent)
                {
                    BreakTheCircuit();
                }
            }
        }

        private void BreakTheCircuit()
        {
            var willDurationTakeUsPastDateTimeMaxValue = _durationOfBreak > DateTime.MaxValue - SystemClock.UtcNow();
            _blockedTill = willDurationTakeUsPastDateTimeMaxValue ?
                               DateTime.MaxValue :
                               SystemClock.UtcNow() + _durationOfBreak;

            using (TimedLock.Lock(_lock))
            {
                _successCount = 0;
                _failCount = 0;
            }
        }

        private void Initialize()
        {
            _blockedTill = DateTime.MinValue;

            _lastException = new InvalidOperationException("This exception should never be thrown");
        }

    }
}