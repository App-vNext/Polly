using System;

using Polly.CircuitBreaker;
using Polly.Utilities;

namespace Polly.Shared.CircuitBreaker
{
    internal class FrequencyCircuitController : ICircuitController
    {
        private readonly TimeSpan _durationOfBreak;
        private readonly int _faultsAllowedBeforeBreaking;
        private readonly long _measuringPeriodAsTicks;
        private readonly long[] _faultTimings;
        private int _faultIndex;
        private bool _statisticsComplete;

        private DateTime _blockedTill;
        private CircuitState _circuitState;
        private Exception _lastException;

        private readonly Action<Exception, TimeSpan, Context> _onBreak;
        private readonly Action<Context> _onReset;
        private readonly Action _onHalfOpen;

        private readonly object _lock = new object();

        public FrequencyCircuitController(int faultsAllowedBeforeBreaking, TimeSpan measuringPeriod, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            _durationOfBreak = durationOfBreak;
            _faultsAllowedBeforeBreaking = faultsAllowedBeforeBreaking;
            _measuringPeriodAsTicks = measuringPeriod.Ticks;
            _faultTimings = new long[_faultsAllowedBeforeBreaking];

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
                    if (_circuitState == CircuitState.Open && !IsInAutomatedBreak_NeedsLock)
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

        private bool IsInAutomatedBreak_NeedsLock
        {
            get
            {
                return SystemClock.UtcNow() < _blockedTill;
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
            _faultIndex = 0;
            _statisticsComplete = false;

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
                CircuitState priorState = _circuitState;

                ResetInternal_NeedsLock();

                if (priorState != CircuitState.Closed)
                {
                    _onReset(context ?? Context.Empty);
                }
            }
        }

        public void OnActionSuccess(Context context)
        {
            
        }

        public void OnActionFailure(Exception ex, Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = ex;

                long ticksNow = SystemClock.UtcNow().Ticks;
                _faultTimings[_faultIndex] = ticksNow;

                _faultIndex = (_faultIndex + 1) %_faultsAllowedBeforeBreaking;
                _statisticsComplete = _statisticsComplete || (_faultIndex == 0); // If at 0 after ++ operation, we must have wrapped.

                // Alternative to above - more readable?
                //_faultIndex++;
                //if (_faultIndex == _faultsAllowedBeforeBreaking)
                //{
                //    _statisticsComplete = true;
                //    _faultIndex = 0;
                //}

                if (_statisticsComplete)
                {
                    long ticksNFaultsAgo = _faultTimings[_faultIndex];
                    if (ticksNow - ticksNFaultsAgo <= _measuringPeriodAsTicks)
                    {
                        BreakFor_NeedsLock(_durationOfBreak, context);
                    }
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
