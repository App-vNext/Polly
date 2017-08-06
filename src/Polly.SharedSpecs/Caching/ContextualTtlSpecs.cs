using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Polly.Caching;
using Xunit;

namespace Polly.Specs.Caching
{
    public class ContextualTtlSpecs
    {
        [Fact]
        public void Should_return_zero_if_no_value_set_on_context()
        {
            new ContextualTtl().GetTtl(new Context("someExecutionKey")).Timespan.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void Should_return_zero_if_invalid_value_set_on_context()
        {
            Dictionary<string, object> contextData = new Dictionary<string, object>();
            contextData[ContextualTtl.TimeSpanKey] = new object();

            Context context = new Context(String.Empty, contextData);
            new ContextualTtl().GetTtl(context).Timespan.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void Should_return_value_set_on_context()
        {
            TimeSpan ttl = TimeSpan.FromSeconds(30);
            Dictionary<string, object> contextData = new Dictionary<string, object>();
            contextData[ContextualTtl.TimeSpanKey] = ttl;

            Context context = new Context(String.Empty, contextData);
            Ttl gotTtl = new ContextualTtl().GetTtl(context);
            gotTtl.Timespan.Should().Be(ttl);
            gotTtl.SlidingExpiration.Should().BeFalse();
        }

        [Fact]
        public void Should_return_negative_value_set_on_context()
        {
            TimeSpan ttl = TimeSpan.FromTicks(-1);
            Dictionary<string, object> contextData = new Dictionary<string, object>();
            contextData[ContextualTtl.TimeSpanKey] = ttl;

            Context context = new Context(String.Empty, contextData);
            Ttl gotTtl = new ContextualTtl().GetTtl(context);
            gotTtl.Timespan.Should().Be(ttl);
            gotTtl.SlidingExpiration.Should().BeFalse();
        }
    }
}
