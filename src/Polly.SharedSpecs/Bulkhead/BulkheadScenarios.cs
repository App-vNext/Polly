using System.Collections;
using System.Collections.Generic;

namespace Polly.Specs.Bulkhead
{
    /// <summary>
    /// A set of test scenarios used in all BulkheadPolicy tests.
    /// </summary>
    internal class BulkheadScenarios : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new BulkheadScenario(maxParallelization: 5, maxQueuingActions: 0, totalTestLoad: 3, cancelQueuing: false, cancelExecuting: false, scenario: "A bulkhead, with no queue, not even oversubscribed.").ToTheoryData();
            yield return new BulkheadScenario(maxParallelization: 20, maxQueuingActions: 0, totalTestLoad: 3, cancelQueuing: false, cancelExecuting: true, scenario: "A high capacity bulkhead, with no queue, not even oversubscribed; cancel some executing.").ToTheoryData();
            yield return new BulkheadScenario(maxParallelization: 3, maxQueuingActions: 0, totalTestLoad: 4, cancelQueuing: false, cancelExecuting: false, scenario: "A bulkhead, with no queue, oversubscribed.").ToTheoryData();
            yield return new BulkheadScenario(maxParallelization: 3, maxQueuingActions: 1, totalTestLoad: 5, cancelQueuing: false, cancelExecuting: false, scenario: "A bulkhead, with not enough queue to avoid rejections, oversubscribed.").ToTheoryData();
            yield return new BulkheadScenario(maxParallelization: 6, maxQueuingActions: 3, totalTestLoad: 9, cancelQueuing: true, cancelExecuting: true, scenario: "A bulkhead, with not enough queue to avoid rejections, oversubscribed; cancel some queuing, and some executing.").ToTheoryData();
            yield return new BulkheadScenario(5, 3, 8, cancelQueuing: false, cancelExecuting: false, scenario: "A bulkhead, with enough queue to avoid rejections, oversubscribed.").ToTheoryData();
            yield return new BulkheadScenario(maxParallelization: 6, maxQueuingActions: 3, totalTestLoad: 9, cancelQueuing: true, cancelExecuting: true, scenario: "A bulkhead, with enough queue to avoid rejections, oversubscribed; cancel some queuing, and some executing.").ToTheoryData();
            yield return new BulkheadScenario(maxParallelization: 1, maxQueuingActions: 6, totalTestLoad: 5, cancelQueuing: true, cancelExecuting: true, scenario: "A very tight capacity bulkhead, but which allows a huge queue; enough for all actions to be gradually processed; cancel some queuing, and some executing.").ToTheoryData();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
