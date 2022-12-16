namespace Polly.Specs.Bulkhead
{
    internal struct BulkheadScenario
    {
        readonly int _maxParallelization;
        readonly int _maxQueuingActions;
        readonly int _totalTestLoad;
        readonly string _scenario;
        readonly bool _cancelQueuing;
        readonly bool _cancelExecuting;

        public BulkheadScenario(int maxParallelization, int maxQueuingActions, int totalTestLoad, bool cancelQueuing, bool cancelExecuting, string scenario)
        {
             _maxParallelization = maxParallelization;
             _maxQueuingActions = maxQueuingActions;
             _totalTestLoad = totalTestLoad;
             _scenario = scenario;
             _cancelQueuing = cancelQueuing;
             _cancelExecuting = cancelExecuting;
        }

        public object[] ToTheoryData()
        {
            return new object[] {_maxParallelization, _maxQueuingActions, _totalTestLoad, _cancelQueuing, _cancelExecuting, _scenario };
        }
    }
}
