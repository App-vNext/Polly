using System;
using FluentAssertions;
using Polly.Caching;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Caching
{
    [Collection("SystemClockDependantCollection")]
    public class AbsoluteTtlSpecs : IDisposable
    {
        [Fact]
        public void Should_be_able_to_configure_for_near_future_time()
        {
            Action configure = () => new AbsoluteTtl(DateTime.Today.AddDays(1));

            configure.ShouldNotThrow();
        }

        [Fact]
        public void Should_be_able_to_configure_for_far_future()
        {
            Action configure = () => new AbsoluteTtl(DateTimeOffset.MaxValue);

            configure.ShouldNotThrow();
        }

        [Fact]
        public void Should_be_able_to_configure_for_past()
        {
            Action configure = () => new AbsoluteTtl(DateTimeOffset.MinValue);

            configure.ShouldNotThrow();
        }

        [Fact]
        public void Should_return_zero_ttl_if_configured_to_expire_in_past()
        {
            AbsoluteTtl ttlStrategy = new AbsoluteTtl(SystemClock.DateTimeOffsetUtcNow().Subtract(TimeSpan.FromTicks(1)));

            ttlStrategy.GetTtl(new Context("someExecutionKey")).Timespan.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void Should_return_timespan_reflecting_time_until_expiry()
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            AbsoluteTtl ttlStrategy = new AbsoluteTtl(tomorrow);

            SystemClock.DateTimeOffsetUtcNow = () => today;
            ttlStrategy.GetTtl(new Context("someExecutionKey")).Timespan.Should().Be(TimeSpan.FromDays(1));
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}
