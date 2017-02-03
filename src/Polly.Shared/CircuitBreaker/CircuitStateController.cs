using System;
using System.Threading;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal abstract class CircuitStateController<TResult> : ICircuitController<TResult>
    {
        protected readonly TimeSpan _durationOfBreak;
        protected long _blockedTill;
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
                    return _lastOutcome?.Exception;
                }
            }
        }

        public TResult LastHandledResult
        {
            get
            {
                using (TimedLock.Lock(_lock))
                {
                    return _lastOutcome != null 
                        ? _lastOutcome.Result : default(TResult);
                }
            }
        }

        protected bool IsInAutomatedBreak_NeedsLock
        {
            get
            {
                return SystemClock.UtcNow().Ticks < _blockedTill;
            }
        }

        public void Isolate()
        {
            using (TimedLock.Lock(_lock))
            {
                _lastOutcome = new DelegateResult<TResult>(new IsolatedCircuitException("The circuit is manually held open and is not allowing calls."));
                BreakFor_NeedsLock(TimeSpan.MaxValue, Context.None);
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
                ? DateTime.MaxValue.Ticks
                : (SystemClock.UtcNow() + durationOfBreak).Ticks;
            _circuitState = CircuitState.Open;

            _onBreak(_lastOutcome, durationOfBreak, context);
        }

        public void Reset()
        {
            OnCircuitReset(Context.None);
        }

        protected void ResetInternal_NeedsLock(Context context)
        {
            _blockedTill = DateTime.MinValue.Ticks;
            _lastOutcome = null;

            CircuitState priorState = _circuitState;
            _circuitState = CircuitState.Closed;
            if (priorState != CircuitState.Closed)
            {
                _onReset(context);
            }
        }

        protected bool PermitHalfOpenCircuitTest()
        {
            long currentlyBlockedUntil = _blockedTill;
            if (SystemClock.UtcNow().Ticks >= currentlyBlockedUntil)
            {
                // It's time to permit a / another trial call in the half-open state ...
                // ... but to prevent race conditions/multiple calls, we have to ensure only _one_ thread wins the race to own this next call.
                return Interlocked.CompareExchange(ref _blockedTill, SystemClock.UtcNow().Ticks + _durationOfBreak.Ticks, currentlyBlockedUntil) == currentlyBlockedUntil;
            }
            return false;
        }

        private BrokenCircuitException GetBreakingException()
        {
            return _lastOutcome.Exception != null
                ? new BrokenCircuitException("The circuit is now open and is not allowing calls.", _lastOutcome.Exception)
                : new BrokenCircuitException<TResult>("The circuit is now open and is not allowing calls.", _lastOutcome.Result);
        }

        public void OnActionPreExecute()
        {
            switch (CircuitState)
            {
                case CircuitState.Closed:
                    break;
                case CircuitState.HalfOpen:
                    if (!PermitHalfOpenCircuitTest()) { throw GetBreakingException(); }
                    break;
                case CircuitState.Open:
                    throw GetBreakingException();
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

