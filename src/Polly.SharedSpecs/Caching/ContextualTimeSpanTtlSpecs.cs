using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Polly.Caching;
using Xunit;

namespace Polly.Specs.Caching
{
    public class ContextualTimeSpanTtlSpecs
    {
        [Fact]
        public void Should_return_zero_if_no_value_set_on_context()
        {
            new ContextualTimeSpanTtl().GetTtl(new Context("someExecutionKey")).Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void Should_return_zero_if_invalid_value_set_on_context()
        {
            Dictionary<string, object> contextData = new Dictionary<string, object>();
            contextData[ContextualTimeSpanTtl.Key] = new object();

            Context context = new Context(String.Empty, contextData);
            new ContextualTimeSpanTtl().GetTtl(context).Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void Should_return_value_set_on_context()
        {
            TimeSpan ttl = TimeSpan.FromSeconds(30);
            Dictionary<string, object> contextData = new Dictionary<string, object>();
            contextData[ContextualTimeSpanTtl.Key] = ttl;

            Context context = new Context(String.Empty, contextData);
            new ContextualTimeSpanTtl().GetTtl(context).Should().Be(ttl);
        }

        [Fact]
        public void Should_return_negative_value_set_on_context()
        {
            TimeSpan ttl = TimeSpan.FromTicks(-1);
            Dictionary<string, object> contextData = new Dictionary<string, object>();
            contextData[ContextualTimeSpanTtl.Key] = ttl;

            Context context = new Context(String.Empty, contextData);
            new ContextualTimeSpanTtl().GetTtl(context).Should().Be(ttl);
        }
    }
}
