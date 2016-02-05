using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal class CircuitController : ICircuitController
    {
        private readonly TimeSpan _durationOfBreak;
        private readonly int _exceptionsAllowedBeforeBreaking;
        private int _count;
        private DateTime _blockedTill;
        private CircuitState _circuitState;
        private Exception _lastException;
        private readonly Action<Exception, TimeSpan, Context> _onBreak;
        private readonly Action<Context> _onReset;
        private readonly Action _onHalfOpen;
        private readonly object _lock = new object();

        public CircuitController(int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            _durationOfBreak = durationOfBreak;
            _exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
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
                    if (_circuitState == CircuitState.Open && !IsInAutomatedBreak)
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

        public bool IsInAutomatedBreak
        {
            get
            {
                using (TimedLock.Lock(_lock))
                {
                    return SystemClock.UtcNow() < _blockedTill;
                }
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
            _count = 0;
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
                ResetInternal_NeedsLock();

                _onReset(context ?? Context.Empty);
            }
        }

        public void OnActionSuccess(Context context)
        {
            Reset(context);
        }

        public void OnActionFailure(Exception ex, Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = ex;

                _count += 1;
                if (_count >= _exceptionsAllowedBeforeBreaking)
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
