namespace Polly.CircuitBreaker;

internal abstract class CircuitStateController<TResult> : ICircuitController<TResult>
{
    protected readonly TimeSpan _durationOfBreak;
    protected readonly Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> _onBreak;
    protected readonly Action<Context> _onReset;
    protected readonly Action _onHalfOpen;
    protected readonly object _lock = new();

    protected long _blockedTill;
    protected CircuitState _circuitState;
    protected DelegateResult<TResult> _lastOutcome;

    protected CircuitStateController(
        TimeSpan durationOfBreak,
        Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> onBreak,
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

            using var _ = TimedLock.Lock(_lock);

#pragma warning disable CA1508 // Avoid dead conditional code. _circuitState is checked again in the lock
            if (_circuitState == CircuitState.Open && !IsInAutomatedBreak_NeedsLock)
            {
                _circuitState = CircuitState.HalfOpen;
                _onHalfOpen();
            }
#pragma warning restore CA1508 // Avoid dead conditional code. _circuitState is checked again in the lock

            return _circuitState;
        }
    }

    public Exception LastException
    {
        get
        {
            using var _ = TimedLock.Lock(_lock);
            return _lastOutcome?.Exception;
        }
    }

    public TResult LastHandledResult
    {
        get
        {
            using var _ = TimedLock.Lock(_lock);
            return _lastOutcome != null ? _lastOutcome.Result : default;
        }
    }

    protected bool IsInAutomatedBreak_NeedsLock => SystemClock.UtcNow().Ticks < _blockedTill;

    public void Isolate()
    {
        using var _ = TimedLock.Lock(_lock);
        _lastOutcome = new DelegateResult<TResult>(new IsolatedCircuitException("The circuit is manually held open and is not allowing calls."));
        BreakFor_NeedsLock(TimeSpan.MaxValue, Context.None());
        _circuitState = CircuitState.Isolated;
    }

    protected void Break_NeedsLock(Context context) =>
        BreakFor_NeedsLock(_durationOfBreak, context);

    private void BreakFor_NeedsLock(TimeSpan durationOfBreak, Context context)
    {
        bool willDurationTakeUsPastDateTimeMaxValue = durationOfBreak > DateTime.MaxValue - SystemClock.UtcNow();
        _blockedTill = willDurationTakeUsPastDateTimeMaxValue
            ? DateTime.MaxValue.Ticks
            : (SystemClock.UtcNow() + durationOfBreak).Ticks;

        var transitionedState = _circuitState;
        _circuitState = CircuitState.Open;

        _onBreak(_lastOutcome, transitionedState, durationOfBreak, context);
    }

    public void Reset() =>
        OnCircuitReset(Context.None());

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
        if (SystemClock.UtcNow().Ticks < currentlyBlockedUntil)
        {
            return false;
        }

        // It's time to permit a / another trial call in the half-open state ...
        // ... but to prevent race conditions/multiple calls, we have to ensure only _one_ thread wins the race to own this next call.
        return Interlocked.CompareExchange(ref _blockedTill, SystemClock.UtcNow().Ticks + _durationOfBreak.Ticks, currentlyBlockedUntil) == currentlyBlockedUntil;
    }

    private BrokenCircuitException GetBreakingException()
    {
        const string BrokenCircuitMessage = "The circuit is now open and is not allowing calls.";

        var lastOutcome = _lastOutcome;
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

