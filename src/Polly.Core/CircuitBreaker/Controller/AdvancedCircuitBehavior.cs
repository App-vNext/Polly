namespace Polly.CircuitBreaker;

internal sealed class AdvancedCircuitBehavior : CircuitBehavior
{
    public override void OnActionSuccess(CircuitState currentState)
    {
    }

    public override void OnActionFailure(CircuitState currentState, out bool shouldBreak)
    {
        shouldBreak = false;
    }

    public override void OnCircuitClosed()
    {
    }
}

