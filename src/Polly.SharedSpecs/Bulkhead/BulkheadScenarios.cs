using System.Collections;
using System.Collections.Generic;

namespace Polly.Specs.Bulkhead
{
    /// <summary>
    /// A set of test scenarios used in all BulkheadPolicy tests.
    /// </summary>
    public class BulkheadScenarios : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] {5, 0, 3, "A bulkhead, with no queue, not even oversubscribed.", false, false};
            yield return new object[] {20, 0, 3, "A high capacity bulkhead, with no queue, not even oversubscribed; cancel some executing.", false, true};
            yield return new object[] {3, 0, 4, "A bulkhead, with no queue, oversubscribed.", false, false};
            yield return new object[] {3, 1, 5, "A bulkhead, with not enough queue to avoid rejections, oversubscribed.", false, false};
            yield return new object[] {6, 3, 9, "A bulkhead, with not enough queue to avoid rejections, oversubscribed; cancel some queuing, and some executing.", true, true};
            yield return new object[] {5, 3, 8, "A bulkhead, with enough queue to avoid rejections, oversubscribed.", false, false};
            yield return new object[] {6, 3, 9, "A bulkhead, with enough queue to avoid rejections, oversubscribed; cancel some queuing, and some executing.", true, true};
            yield return new object[] {1, 6, 5, "A very tight capacity bulkhead, but which allows a huge queue; enough for all actions to be gradually processed; cancel some queuing, and some executing.", true, true};
        }       

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    };

}
