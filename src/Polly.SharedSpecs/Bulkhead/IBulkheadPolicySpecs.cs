using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Polly.Bulkhead;
using Xunit;

namespace Polly.Specs.Bulkhead
{
    public class IBulkheadPolicySpecs
    {
        [Fact]
        public void Should_be_able_to_use_BulkheadAvailableCount_via_interface()
        {
            IBulkheadPolicy bulkhead = Policy.Bulkhead(20, 10);

            bulkhead.BulkheadAvailableCount.Should().Be(20);
        }

        [Fact]
        public void Should_be_able_to_use_QueueAvailableCount_via_interface()
        {
            IBulkheadPolicy bulkhead = Policy.Bulkhead(20, 10);

            bulkhead.QueueAvailableCount.Should().Be(10);
        }
    }
}
