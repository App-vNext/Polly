namespace Polly.Specs.Bulkhead;

internal readonly struct BulkheadScenario(
    int maxParallelization,
    int maxQueuingActions,
    int totalTestLoad,
    bool cancelQueuing,
    bool cancelExecuting,
    string scenario)
{
    private readonly int _maxParallelization = maxParallelization;
    private readonly int _maxQueuingActions = maxQueuingActions;
    private readonly int _totalTestLoad = totalTestLoad;
    private readonly string _scenario = scenario;
    private readonly bool _cancelQueuing = cancelQueuing;
    private readonly bool _cancelExecuting = cancelExecuting;

    public object[] ToTheoryData() =>
    [
        _maxParallelization,
        _maxQueuingActions,
        _totalTestLoad,
        _cancelQueuing,
        _cancelExecuting,
        _scenario
    ];
}
