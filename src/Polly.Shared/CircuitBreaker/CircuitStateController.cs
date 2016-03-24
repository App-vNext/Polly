using System;
using System.Collections.Generic;
using System.Text;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal abstract class CircuitStateController : ICircuitController
    {
        protected readonly TimeSpan _durationOfBreak;
        protected DateTime _blockedTill;
        protected CircuitState _circuitState;
        protected Exception _lastException;
        protected readonly Action<Exception, TimeSpan, Context> _onBreak;
        protected readonly Action<Context> _onReset;
        protected readonly Action _onHalfOpen;
        protected readonly object _lock = new object();

        protected CircuitStateController(TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            _durationOfBreak = durationOfBreak;
            _onBreak = onBreak;
            _onReset = onReset;
            _onHalfOpen = onHalfOpen;

            _circuitState = CircuitState.Closed;
            Reset();
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

        protected bool IsInAutomatedBreak_NeedsLock
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

        protected void Break_NeedsLock(Context context)
        {
            BreakFor_NeedsLock(_durationOfBreak, context);
        }

        private void BreakFor_NeedsLock(TimeSpan durationOfBreak, Context context)
        {
            bool willDurationTakeUsPastDateTimeMaxValue = durationOfBreak > DateTime.MaxValue - SystemClock.UtcNow();
            _blockedTill = willDurationTakeUsPastDateTimeMaxValue
                ? DateTime.MaxValue
                : SystemClock.UtcNow() + durationOfBreak;
            _circuitState = CircuitState.Open;

            _onBreak(_lastException, durationOfBreak, context ?? Context.Empty);
        }

        public void Reset()
        {
            OnCircuitReset(Context.Empty);
        }

        protected void ResetInternal_NeedsLock(Context context)
        {
            _blockedTill = DateTime.MinValue;
            _lastException = new InvalidOperationException("This exception should never be thrown");

            CircuitState priorState = _circuitState;
            _circuitState = CircuitState.Closed;
            if (priorState != CircuitState.Closed)
            {
                _onReset(context ?? Context.Empty);
            }
        }

        public void OnActionPreExecute()
        {
            using (TimedLock.Lock(_lock))
            {
                switch (_circuitState)
                {
                    case CircuitState.Closed:
                    case CircuitState.HalfOpen:
                        break;
                    case CircuitState.Open:
                        throw new BrokenCircuitException("The circuit is now open and is not allowing calls.", _lastException);
                    case CircuitState.Isolated:
                        throw new IsolatedCircuitException("The circuit is manually held open and is not allowing calls.");
                    default:
                        throw new InvalidOperationException("Unhandled CircuitState.");
                }
            }
        }

        public abstract void OnActionSuccess(Context context);

        public abstract void OnActionFailure(Exception ex, Context context);

        public abstract void OnCircuitReset(Context context);
    }
}

