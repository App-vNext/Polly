namespace Polly.Specs.Bulkhead;

internal struct BulkheadScenario
{
    private readonly int _maxParallelization;
    private readonly int _maxQueuingActions;
    private readonly int _totalTestLoad;
    private readonly string _scenario;
    private readonly bool _cancelQueuing;
    private readonly bool _cancelExecuting;

    public BulkheadScenario(int maxParallelization, int maxQueuingActions, int totalTestLoad, bool cancelQueuing, bool cancelExecuting, string scenario)
    {
        _maxParallelization = maxParallelization;
        _maxQueuingActions = maxQueuingActions;
        _totalTestLoad = totalTestLoad;
        _scenario = scenario;
        _cancelQueuing = cancelQueuing;
        _cancelExecuting = cancelExecuting;
    }

    public object[] ToTheoryData() =>
        new object[]
        {
            _maxParallelization,
            _maxQueuingActions,
            _totalTestLoad,
            _cancelQueuing,
            _cancelExecuting,
            _scenario
        };
}
