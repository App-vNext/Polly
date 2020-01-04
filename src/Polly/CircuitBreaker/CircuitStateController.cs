using Polly.Utilities;
using System;
using System.Threading;

namespace Polly.CircuitBreaker
{
    internal abstract class CircuitStateController<TResult> : ICircuitController<TResult>
    {
        protected readonly Func<int, TimeSpan> _factoryForNextBreakDuration;
        protected long _blockedUntil;
        protected TimeSpan _currentBlockDuration;
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
                return SystemClock.UtcNow().Ticks < _blockedUntil;
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
            if (nextBreakDuration < TimeSpan.Zero)
            {
                throw new InvalidOperationException("Dynamically calculated Break Durations must always be non-negative.");
            }

            BreakFor_NeedsLock(nextBreakDuration, context);
        }

        private void BreakFor_NeedsLock(TimeSpan durationOfBreak, Context context)
        {
            var (breakEnd, breakLength) = ExtensionUpToMaxValue(SystemClock.UtcNow(), durationOfBreak);

            _currentBlockDuration = breakLength;
            _blockedUntil = breakEnd.Ticks;

            var transitionedState = _circuitState;
            _circuitState = CircuitState.Open;

            _onBreak(_lastOutcome, transitionedState, durationOfBreak, context);
        }

        /// <summary>
        /// Given a startDate <b>in Ticks</b>and a proposedExtension (and optionally an explicit end limit), <b>either</b> return
        /// the given extension and the resultant endDate, <b>or</b> (if the proposed extension would take us
        /// past the limit) return DateTime.MaxValue, and the appropriate extension to get to that limit.
        /// </summary>
        /// <param name="startPointTicks">The Ticks of the DateTime to which the proposedExtension will be added.</param>
        /// <param name="proposedExtension">The TimeSpan to add to the startDate</param>
        /// <param name="maxDateTime">Max DateTime value that the extension is allowed to take us up to. DateTime.MaxValue, by default.</param>
        /// <returns>Tuple of (DateTime, TimeSpan), represent the allowed endpoint and the extension to get us there.</returns>
        private ValueTuple<DateTime, TimeSpan> ExtensionUpToMaxValue(long startPointTicks, TimeSpan proposedExtension, DateTime? maxDateTime = null)
        {
            var startDate = new DateTime(startPointTicks);
            return ExtensionUpToMaxValue(startDate, proposedExtension, maxDateTime);
        }

        /// <summary>
        /// Given a startDate and a proposedExtension (and optionally an explicit end limit), <b>either</b> return
        /// the given extension and the resultant endDate, <b>or</b> (if the proposed extension would take us
        /// past the limit) return DateTime.MaxValue, and the appropriate extension to get to that limit.
        /// </summary>
        /// <param name="startDate">The DateTime to which the proposedExtension will be added.</param>
        /// <param name="proposedExtension">The TimeSpan to add to the startDate</param>
        /// <param name="maxDateTime">Max DateTime value that the extension is allowed to take us up to. DateTime.MaxValue, by default.</param>
        /// <returns>Tuple of (DateTime, TimeSpan), represent the allowed endpoint and the extension to get us there.</returns>
        private ValueTuple<DateTime, TimeSpan> ExtensionUpToMaxValue(DateTime startDate, TimeSpan proposedExtension, DateTime? maxDateTime = null)
        {
            maxDateTime = maxDateTime ?? DateTime.MaxValue;
            var maxPossibleExtension = (maxDateTime.Value - startDate);
            var extensionWouldTakeUsPastDateTimeMaxValue = proposedExtension > maxPossibleExtension;

            if (extensionWouldTakeUsPastDateTimeMaxValue)
            {
                return (maxDateTime.Value, maxPossibleExtension);
            }
            else
            {
                return (startDate + proposedExtension, proposedExtension);
            }
        }

        public void Reset() => OnCircuitReset(Context.None());

        protected void ResetInternal_NeedsLock(Context context)
        {
            _blockedUntil = DateTime.MinValue.Ticks;
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
            // ReSharper disable InconsistentNaming
            long blockedUntil_AtStartOfPermitHalfOpenCircuitTest = _blockedUntil;
            if (SystemClock.UtcNow().Ticks < blockedUntil_AtStartOfPermitHalfOpenCircuitTest)
            {
                return false;
            }

            // It's time to permit a / another trial call in the half-open state ...
            // ... but to prevent race conditions/multiple calls, we have to ensure only _one_ thread wins the race to own this next call.
            // If "we" are the thread that wins that race, then we also want to update the BlockedTill date, so that no other thread / call tries
            // to execute before this Test has reached *it's* conclusion (or is deemed to have been lost, if it has failed to return within _currentBlockDuration)
            //
            // So combine doing those two things into an Interlocked.CompareExchange call.
            // Attempt to update _blockedUntil. Then examine the return value to determine whether someone else had beaten us to the punch.
            // If not, then that tells us that we won the race, and we can permit the Circuit Test.
            // If someone had already modified _blockedTil, then we lost the race and thus aren't going to be doing the Test.
            //
            // Note that, in general, we want to extend the blocked period by an amount based on the number of failures in halfOpen state (tracked elsewhere, last time a failure was received).  
            // The logic of halfOpen state is that it permits one fresh trial call per expiry of each extension-of-broken-state.
            // This particular extension of the Open state is occurring due to the preceding Open period expiring, or a halfOpen period expiring with no call result received.
            // i.e. it's not an extension triggered by any additional failures received.
            // Consequently, the period to extend by is unchanged from the last period, so we can reuse _currentBlockDuration (calculated last time we saw an active failure).
            //
            // Lastly note that the conceptual edge case of _blockedUntil being equal to DateTime.MaxValue.Ticks, is not a concern, since in that case the
            // if-check above will not have passed, so we won't have gotten here in the first place.
            var (proposedNewBlockDeadline, _) = ExtensionUpToMaxValue(blockedUntil_AtStartOfPermitHalfOpenCircuitTest, _currentBlockDuration);
            var blockedUntil_AtAtomicStartOfExchangeCall = Interlocked.CompareExchange(ref _blockedUntil, proposedNewBlockDeadline.Ticks, blockedUntil_AtStartOfPermitHalfOpenCircuitTest);
            var blockedUntil_HadNotChangedPriorToExchangeCall = blockedUntil_AtAtomicStartOfExchangeCall == blockedUntil_AtStartOfPermitHalfOpenCircuitTest;

            return blockedUntil_HadNotChangedPriorToExchangeCall;
            // ReSharper restore InconsistentNaming
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

