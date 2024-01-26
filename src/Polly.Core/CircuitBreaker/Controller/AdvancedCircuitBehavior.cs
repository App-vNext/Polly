using Polly.CircuitBreaker.Health;

namespace Polly.CircuitBreaker;

internal sealed class AdvancedCircuitBehavior : CircuitBehavior
{
    private readonly HealthMetrics _metrics;
    private readonly double _failureRatio;
    private readonly int _minimumThroughput;

    public AdvancedCircuitBehavior(double failureRatio, int minimumThroughput, HealthMetrics metrics)
    {
        _metrics = metrics;
        _failureRatio = failureRatio;
        _minimumThroughput = minimumThroughput;
    }

    public override void OnActionSuccess(CircuitState currentState) => _metrics.IncrementSuccess();

    public override void OnActionFailure(CircuitState currentState, out bool shouldBreak)
    {
        switch (currentState)
        {
            case CircuitState.Closed:
                _metrics.IncrementFailure();
                var info = _metrics.GetHealthInfo();
                shouldBreak = info.Throughput >= _minimumThroughput && info.FailureRate >= _failureRatio;
                break;

            case CircuitState.Open:
            case CircuitState.Isolated:
                // A failure call result may arrive when the circuit is open, if it was placed before the circuit broke.
                // We take no action beyond tracking the metric
                // We do not want to duplicate-signal onBreak
                // We do not want to extend time for which the circuit is broken.
                // We do not want to mask the fact that the call executed (as replacing its result with a Broken/IsolatedCircuitException would do).
                _metrics.IncrementFailure();
                shouldBreak = false;
                break;
            default:
                shouldBreak = false;
                break;
        }
    }

    public override void OnCircuitClosed() => _metrics.Reset();

    public override HealthInfo GetHealthInfo() => _metrics.GetHealthInfo();
}

