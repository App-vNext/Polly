using System;
using FluentAssertions;
using Polly.Caching;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Caching
{
    public class RelativeTtlSpecs
    {
        [Fact]
        public void Should_throw_when_timespan_is_less_than_zero()
        {
            Action configure = () => new RelativeTtl(TimeSpan.FromMilliseconds(-1));

            configure.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("ttl");
        }

        [Fact]
        public void Should_not_throw_when_timespan_is_zero()
        {
            Action configure = () => new RelativeTtl(TimeSpan.Zero);

            configure.Should().NotThrow();
        }

        [Fact]
        public void Should_allow_timespan_max_value()
        {
            Action configure = () => new RelativeTtl(TimeSpan.MaxValue);

            configure.Should().NotThrow();
        }

        [Fact]
        public void Should_return_configured_timespan()
        {
            var ttl = TimeSpan.FromSeconds(30);

            var ttlStrategy = new RelativeTtl(ttl);

            var retrieved = ttlStrategy.GetTtl(new Context("someOperationKey"), null);
            retrieved.Timespan.Should().BeCloseTo(ttl);
            retrieved.SlidingExpiration.Should().BeFalse();
        }

        [Fact]
        public void Should_return_configured_timespan_from_time_requested()
        {
            var fixedTime = SystemClock.DateTimeOffsetUtcNow();
            var ttl = TimeSpan.FromSeconds(30);
            var delay = TimeSpan.FromSeconds(5);

            var ttlStrategy = new RelativeTtl(ttl);

            SystemClock.DateTimeOffsetUtcNow = () => fixedTime.Add(delay);

            var retrieved = ttlStrategy.GetTtl(new Context("someOperationKey"), null);
            retrieved.Timespan.Should().BeCloseTo(ttl);
            retrieved.SlidingExpiration.Should().BeFalse();
        }
    }
}
