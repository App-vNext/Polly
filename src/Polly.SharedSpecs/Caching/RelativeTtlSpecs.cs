using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Polly.Caching;
using Xunit;

namespace Polly.Specs.Caching
{
    public class RelativeTtllSpecs
    {
        [Fact]
        public void Should_throw_when_timespan_is_less_than_zero()
        {
            Action configure = () => new RelativeTtl(TimeSpan.FromMilliseconds(-1));

            configure.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("ttl");
        }

        [Fact]
        public void Should_not_throw_when_timespan_is_zero()
        {
            Action configure = () => new RelativeTtl(TimeSpan.Zero);

            configure.ShouldNotThrow();
        }

        [Fact]
        public void Should_allow_timespan_max_value()
        {
            Action configure = () => new RelativeTtl(TimeSpan.MaxValue);

            configure.ShouldNotThrow();
        }

        [Fact]
        public void Should_return_configured_timespan()
        {
            TimeSpan ttl = TimeSpan.FromSeconds(30);

            RelativeTtl ttlStrategy = new RelativeTtl(ttl);

            Ttl retrieved = ttlStrategy.GetTtl(new Context("someExecutionKey"));
            retrieved.Timespan.Should().BeCloseTo(ttl);
            retrieved.SlidingExpiration.Should().BeFalse();
        }
    }
}
