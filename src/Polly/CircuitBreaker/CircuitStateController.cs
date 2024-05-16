namespace Polly.CircuitBreaker;

internal abstract class CircuitStateController<TResult> : ICircuitController<TResult>
{
    protected readonly TimeSpan DurationOfBreak;
    protected readonly Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> OnBreak;
    protected readonly Action<Context> OnReset;
    protected readonly Action OnHalfOpen;
    protected readonly object Lock = new();

    protected long BlockedTill;
    protected CircuitState InternalCircuitState;
    protected DelegateResult<TResult> LastOutcome;

    protected CircuitStateController(
        TimeSpan durationOfBreak,
        Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> onBreak,
        Action<Context> onReset,
        Action onHalfOpen)
    {
        DurationOfBreak = durationOfBreak;
        OnBreak = onBreak;
        OnReset = onReset;
        OnHalfOpen = onHalfOpen;

        InternalCircuitState = CircuitState.Closed;
        Reset();
    }

    public CircuitState CircuitState
    {
        get
        {
            if (InternalCircuitState != CircuitState.Open)
            {
                return InternalCircuitState;
            }

            using var _ = TimedLock.Lock(Lock);

#pragma warning disable CA1508 // Avoid dead conditional code. _circuitState is checked again in the lock
            if (InternalCircuitState == CircuitState.Open && !IsInAutomatedBreak_NeedsLock)
            {
                InternalCircuitState = CircuitState.HalfOpen;
                OnHalfOpen();
            }
#pragma warning restore CA1508 // Avoid dead conditional code. _circuitState is checked again in the lock

            return InternalCircuitState;
        }
    }

    public Exception LastException
    {
        get
        {
            using var _ = TimedLock.Lock(Lock);
            return LastOutcome?.Exception;
        }
    }

    public TResult LastHandledResult
    {
        get
        {
            using var _ = TimedLock.Lock(Lock);
            return LastOutcome != null ? LastOutcome.Result : default;
        }
    }

    protected bool IsInAutomatedBreak_NeedsLock => SystemClock.UtcNow().Ticks < BlockedTill;

    public void Isolate()
    {
        using var _ = TimedLock.Lock(Lock);
        LastOutcome = new DelegateResult<TResult>(new IsolatedCircuitException("The circuit is manually held open and is not allowing calls."));
        BreakFor_NeedsLock(TimeSpan.MaxValue, Context.None());
        InternalCircuitState = CircuitState.Isolated;
    }

    protected void Break_NeedsLock(Context context) =>
        BreakFor_NeedsLock(DurationOfBreak, context);

    private void BreakFor_NeedsLock(TimeSpan durationOfBreak, Context context)
    {
        bool willDurationTakeUsPastDateTimeMaxValue = durationOfBreak > DateTime.MaxValue - SystemClock.UtcNow();
        BlockedTill = willDurationTakeUsPastDateTimeMaxValue
            ? DateTime.MaxValue.Ticks
            : (SystemClock.UtcNow() + durationOfBreak).Ticks;

        var transitionedState = InternalCircuitState;
        InternalCircuitState = CircuitState.Open;

        OnBreak(LastOutcome, transitionedState, durationOfBreak, context);
    }

    public void Reset() =>
        OnCircuitReset(Context.None());

    protected void ResetInternal_NeedsLock(Context context)
    {
        BlockedTill = DateTime.MinValue.Ticks;
        LastOutcome = null;

        CircuitState priorState = InternalCircuitState;
        InternalCircuitState = CircuitState.Closed;
        if (priorState != CircuitState.Closed)
        {
            OnReset(context);
        }
    }

    protected bool PermitHalfOpenCircuitTest()
    {
        long currentlyBlockedUntil = BlockedTill;
        if (SystemClock.UtcNow().Ticks < currentlyBlockedUntil)
        {
            return false;
        }

        // It's time to permit a / another trial call in the half-open state ...
        // ... but to prevent race conditions/multiple calls, we have to ensure only _one_ thread wins the race to own this next call.
        return Interlocked.CompareExchange(ref BlockedTill, SystemClock.UtcNow().Ticks + DurationOfBreak.Ticks, currentlyBlockedUntil) == currentlyBlockedUntil;
    }

    private BrokenCircuitException GetBreakingException()
    {
        const string BrokenCircuitMessage = "The circuit is now open and is not allowing calls.";

        var lastOutcome = LastOutcome;
        if (lastOutcome == null)
        {
            return new BrokenCircuitException(BrokenCircuitMessage);
        }

        if (lastOutcome.Exception != null)
        {
            return new BrokenCircuitException(BrokenCircuitMessage, lastOutcome.Exception);
        }

        return new BrokenCircuitException<TResult>(BrokenCircuitMessage, lastOutcome.Result);
    }

    public void OnActionPreExecute()
    {
        switch (CircuitState)
        {
            case CircuitState.Closed:
                break;
            case CircuitState.HalfOpen:
                if (!PermitHalfOpenCircuitTest())
                {
                    throw GetBreakingException();
                }

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

