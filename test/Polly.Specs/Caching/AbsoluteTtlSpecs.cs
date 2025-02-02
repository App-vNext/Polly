﻿namespace Polly.Specs.Caching;

[Collection(Constants.SystemClockDependentTestCollection)]
public class AbsoluteTtlSpecs : IDisposable
{
    [Fact]
    public void Should_be_able_to_configure_for_near_future_time()
    {
        Action configure = () => _ = new AbsoluteTtl(DateTimeOffset.UtcNow.Date.AddDays(1));

        Should.NotThrow(configure);
    }

    [Fact]
    public void Should_be_able_to_configure_for_far_future()
    {
        Action configure = () => _ = new AbsoluteTtl(DateTimeOffset.MaxValue);

        Should.NotThrow(configure);
    }

    [Fact]
    public void Should_be_able_to_configure_for_past()
    {
        Action configure = () => _ = new AbsoluteTtl(DateTimeOffset.MinValue);

        Should.NotThrow(configure);
    }

    [Fact]
    public void Should_return_zero_ttl_if_configured_to_expire_in_past()
    {
        AbsoluteTtl ttlStrategy = new AbsoluteTtl(SystemClock.DateTimeOffsetUtcNow().Subtract(TimeSpan.FromTicks(1)));

        ttlStrategy.GetTtl(new Context("someOperationKey"), null).Timespan.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void Should_return_timespan_reflecting_time_until_expiry()
    {
        DateTimeOffset today = DateTimeOffset.UtcNow.Date;
        DateTimeOffset tomorrow = today.AddDays(1);

        AbsoluteTtl ttlStrategy = new AbsoluteTtl(tomorrow);

        SystemClock.DateTimeOffsetUtcNow = () => today;
        ttlStrategy.GetTtl(new Context("someOperationKey"), null).Timespan.ShouldBe(TimeSpan.FromDays(1));
    }

    public void Dispose() =>
        SystemClock.Reset();
}
