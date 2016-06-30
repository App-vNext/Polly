using System;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal abstract class CircuitStateController<TResult> : ICircuitController<TResult>
    {
        protected readonly TimeSpan _durationOfBreak;
        protected DateTime _blockedTill;
        protected CircuitState _circuitState;
        protected DelegateResult<TResult> _lastOutcome;
        protected readonly Action<DelegateResult<TResult>, TimeSpan, Context> _onBreak;
        protected readonly Action<Context> _onReset;
        protected readonly Action _onHalfOpen;
        protected readonly object _lock = new object();

        protected CircuitStateController(
            TimeSpan durationOfBreak, 
            Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, 
            Action<Context> onReset, 
            Action onHalfOpen)
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
                if (_circuitState != CircuitState.Open)
                {
                    return _circuitState;
                }

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
                    return _lastOutcome.Exception;
                }
            }
        }

        public TResult LastHandledResult
        {
            get
            {
                using (TimedLock.Lock(_lock))
                {
                    return _lastOutcome.Result;
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
                _lastOutcome = new DelegateResult<TResult>(new IsolatedCircuitException("The circuit is manually held open and is not allowing calls."));
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

            _onBreak(_lastOutcome, durationOfBreak, context ?? Context.Empty);
        }

        public void Reset()
        {
            OnCircuitReset(Context.Empty);
        }

        protected void ResetInternal_NeedsLock(Context context)
        {
            _blockedTill = DateTime.MinValue;
            _lastOutcome = new DelegateResult<TResult>(new InvalidOperationException("This exception should never be thrown"));

            CircuitState priorState = _circuitState;
            _circuitState = CircuitState.Closed;
            if (priorState != CircuitState.Closed)
            {
                _onReset(context ?? Context.Empty);
            }
        }

        public void OnActionPreExecute()
        {
            switch (CircuitState)
            {
                case CircuitState.Closed:
                case CircuitState.HalfOpen:
                    break;
                case CircuitState.Open:
                    throw _lastOutcome.Exception != null
                        ? new BrokenCircuitException("The circuit is now open and is not allowing calls.", _lastOutcome.Exception)
                        : new BrokenCircuitException<TResult>("The circuit is now open and is not allowing calls.", _lastOutcome.Result);
                case CircuitState.Isolated:
                    throw new IsolatedCircuitException("The circuit is manually held open and is not allowing calls.");
                default:
                    throw new InvalidOperationException("Unhandled CircuitState.");
            }
        }

        public abstract void OnActionSuccess(Context context);

        public abstract void OnActionFailure(DelegateResult<TResult> outcome, Context context);

        public abstract void OnCircuitReset(Context context);
    }
}

