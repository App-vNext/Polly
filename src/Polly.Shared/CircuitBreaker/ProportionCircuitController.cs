using System;

using Polly.CircuitBreaker;
using Polly.Utilities;

namespace Polly.Shared.CircuitBreaker
{
    internal class ProportionCircuitController : ICircuitController
    {
        private readonly TimeSpan _durationOfBreak;
        private readonly int _faultsAllowedBeforeBreaking;
        private readonly int _perTotalActions;
        private readonly bool[] _faultHistory;
        private int _faultCount;
        private int _historyIndex;

        private DateTime _blockedTill;
        private CircuitState _circuitState;
        private Exception _lastException;

        private readonly Action<Exception, TimeSpan, Context> _onBreak;
        private readonly Action<Context> _onReset;
        private readonly Action _onHalfOpen;

        private readonly object _lock = new object();

        public ProportionCircuitController(int faultsAllowedBeforeBreaking, int perTotalActions, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            _durationOfBreak = durationOfBreak;
            _faultsAllowedBeforeBreaking = faultsAllowedBeforeBreaking;
            _perTotalActions = perTotalActions;
            _faultHistory = new bool[_perTotalActions];

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
            for (int i = 0; i < _perTotalActions; i++) _faultHistory[i] = false;
            _faultCount = 0;
            _historyIndex = 0;

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
            using (TimedLock.Lock(_lock))
            {
                if (_faultHistory[_historyIndex]) _faultCount--; // We have success, this slot previously recorded a failure, so we are now one failure less.
                _faultHistory[_historyIndex] = false;
                _historyIndex = (_historyIndex + 1) % _perTotalActions;
            }
        }

        public void OnActionFailure(Exception ex, Context context)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = ex;

                if (!_faultHistory[_historyIndex]) _faultCount++; // We have failure, this slot previously recorded a success, so we are now one failure more.
                _faultHistory[_historyIndex] = true;
                _historyIndex = (_historyIndex + 1) % _perTotalActions;

                if (_faultCount >= _faultsAllowedBeforeBreaking)
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
