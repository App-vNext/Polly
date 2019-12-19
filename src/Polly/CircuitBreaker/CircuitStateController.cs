using System;
using System.Threading;
using Polly.Utilities;

namespace Polly.CircuitBreaker
{
    internal abstract class CircuitStateController<TResult> : ICircuitController<TResult>
    {
        protected readonly Func<int, TimeSpan> _factoryForNextBreakDuration;
        protected long _blockedTill;
        protected CircuitState _circuitState;
        protected DelegateResult<TResult> _lastOutcome;

        protected readonly Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> _onBreak;
        protected readonly Action<Context> _onReset;
        protected readonly Action _onHalfOpen;

        protected readonly object _lock = new object();

        protected CircuitStateController(
            Func<int, TimeSpan> factoryForNextBreakDuration,
            Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> onBreak, 
            Action<Context> onReset, 
            Action onHalfOpen)
        {
            _factoryForNextBreakDuration = factoryForNextBreakDuration;
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
                        ? _lastOutcome.Result : default;
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
                BreakFor_NeedsLock(TimeSpan.MaxValue, Context.None());
                _circuitState = CircuitState.Isolated;
            }
        }

        protected void Break_NeedsLock(int consecutiveHalfOpenFailures, Context context)
        {
            var nextBreakDuration = _factoryForNextBreakDuration(consecutiveHalfOpenFailures);
            BreakFor_NeedsLock(nextBreakDuration, context);
        }

        private void BreakFor_NeedsLock(TimeSpan durationOfBreak, Context context)
        {
            if (durationOfBreak < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(durationOfBreak), "Value must be greater than zero.");

            bool willDurationTakeUsPastDateTimeMaxValue = durationOfBreak > DateTime.MaxValue - SystemClock.UtcNow();
            _blockedTill = willDurationTakeUsPastDateTimeMaxValue
                ? DateTime.MaxValue.Ticks
                : (SystemClock.UtcNow() + durationOfBreak).Ticks;

            var transitionedState = _circuitState;
            _circuitState = CircuitState.Open;

            _onBreak(_lastOutcome, transitionedState, durationOfBreak, context);
        }

        public void Reset() => OnCircuitReset(Context.None());

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
            long initialBlockedTill = _blockedTill;
            if (SystemClock.UtcNow().Ticks < initialBlockedTill)
            {
                return false;
            }

            // It's time to permit a / another trial call in the half-open state ...
            // ... but to prevent race conditions/multiple calls, we have to ensure only _one_ thread wins the race to own this next call.
            // If "we" are the thread that wins that races, then we also want to update the BlockedTill date, so that no other thread / call tries to execute before this Test has reached *it's* conclusion.
            //
            // So combine doing those two things into an Interlocked.CompareExchange call.
            // Attempt to update _blockedTill. Then examine the return value to determine whether someone else had beaten us to the punch.
            // If not, then that tells us that we won the race, and we can permit the Circuit Test.
            // If someone had already modified _blockedTil, then we lost the race and thus aren't going to be doing the Test.
            //
            // Note that if the Circuit Test is a failure, it will re-configure the Break limit, using the updated number of Test failures.
            // Which means that it doesn't really matter what delay we put in here as long as no other attempts get made before this Test completes.
            // This is helpful, since we don't have convenient access to the number of Test Failures here, and thus can't calculate the *correct* next delay.
            //
            // Lastly note that the conceptual edge case of _blockedTil being equal to DateTime.MaxValue.Ticks, is not a concern, since in that case the
            // if-check above will not have passed, so we won't have gotten here in the first place.

            var blockedTil_AtAtomicStartOfExchangeCall = Interlocked.CompareExchange(ref _blockedTill, DateTime.MaxValue.Ticks, initialBlockedTill);
            var blockedTil_HadNotChangedPriorToExchangeCall = blockedTil_AtAtomicStartOfExchangeCall == initialBlockedTill;

            return blockedTil_HadNotChangedPriorToExchangeCall;
        }

        private BrokenCircuitException GetBreakingException()
            => _lastOutcome.Exception != null
                ? new BrokenCircuitException("The circuit is now open and is not allowing calls.", _lastOutcome.Exception)
                : new BrokenCircuitException<TResult>("The circuit is now open and is not allowing calls.", _lastOutcome.Result);

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

