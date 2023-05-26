namespace Polly.CircuitBreaker;

internal sealed class ConsecutiveFailuresCircuitBehavior : CircuitBehavior
{
    private readonly int _failureThreshold;
    private int _consecutiveFailures;

    public ConsecutiveFailuresCircuitBehavior(int failureThreshold) => _failureThreshold = failureThreshold;

    public override void OnActionSuccess(CircuitState currentState)
    {
        if (currentState == CircuitState.Closed)
        {
            _consecutiveFailures = 0;
        }
    }

    public override void OnActionFailure(CircuitState currentState, out bool shouldBreak)
    {
        shouldBreak = false;

        if (currentState == CircuitState.Closed)
        {
            _consecutiveFailures += 1;
            if (_consecutiveFailures >= _failureThreshold)
            {
                shouldBreak = true;
            }
        }
    }

    public override void OnCircuitClosed()
    {
        _consecutiveFailures = 0;
    }
}

