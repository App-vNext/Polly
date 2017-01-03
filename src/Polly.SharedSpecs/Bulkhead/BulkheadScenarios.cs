using System.Collections;
using System.Collections.Generic;

namespace Polly.Specs
{
    /// <summary>
    /// A set of test scenarios used in all BulkheadPolicy tests.
    /// </summary>
    public class BulkheadScenarios : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[] {5, 0, 3, "A light capacity bulkhead, with no queue, not even overloaded.", false, false},
            new object[] {20, 0, 3, "A high capacity bulkhead, with no queue, not even overloaded; cancel some executing.", false, true},
            new object[] {5, 0, 8, "A light capacity bulkhead, with no queue, overloaded.", false, false},
            new object[] {20, 0, 30, "A high capacity bulkhead, with no queue, overloaded.", false, false},
            new object[] {5, 1, 8, "A light capacity bulkhead, with not enough queue to avoid rejections, overloaded.", false, false},
            new object[] {20, 10, 30, "A high capacity bulkhead, with not enough queue to avoid rejections, overloaded; cancel some queuing, and some executing.", true, true},
            new object[] {5, 3, 8, "A light capacity bulkhead, with enough queue to avoid rejections, overloaded.", false, false},
            new object[] {20, 10, 30, "A high capacity bulkhead, with enough queue to avoid rejections, overloaded; cancel some queuing, and some executing.", true, true},
            new object[] {5, 30, 25, "A very tight capacity bulkhead, but which allows a huge queue; enough for all actions to be gradually processed; cancel some queuing, and some executing.", true, true},
        };       

        public IEnumerator<object[]> GetEnumerator() { return _data.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    };

}
