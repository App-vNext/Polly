using System;
using System.Collections.Generic;
using FluentAssertions;
using Polly.Caching;
using Xunit;

namespace Polly.Specs.Caching;

public class ContextualTtlSpecs
{
    [Fact]
    public void Should_return_zero_if_no_value_set_on_context()
    {
        new ContextualTtl().GetTtl(new("someOperationKey"), null).Timespan.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Should_return_zero_if_invalid_value_set_on_context()
    {
        var contextData = new Dictionary<string, object>();
        contextData[ContextualTtl.TimeSpanKey] = new();

        var context = new Context(String.Empty, contextData);
        new ContextualTtl().GetTtl(context, null).Timespan.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Should_return_value_set_on_context()
    {
        var ttl = TimeSpan.FromSeconds(30);
        var contextData = new Dictionary<string, object>();
        contextData[ContextualTtl.TimeSpanKey] = ttl;

        var context = new Context(String.Empty, contextData);
        var gotTtl = new ContextualTtl().GetTtl(context, null);
        gotTtl.Timespan.Should().Be(ttl);
        gotTtl.SlidingExpiration.Should().BeFalse();
    }

    [Fact]
    public void Should_return_negative_value_set_on_context()
    {
        var ttl = TimeSpan.FromTicks(-1);
        var contextData = new Dictionary<string, object>();
        contextData[ContextualTtl.TimeSpanKey] = ttl;

        var context = new Context(String.Empty, contextData);
        var gotTtl = new ContextualTtl().GetTtl(context, null);
        gotTtl.Timespan.Should().Be(ttl);
        gotTtl.SlidingExpiration.Should().BeFalse();
    }
}