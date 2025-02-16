namespace Polly.Specs.Caching;

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

        var actual = ttlStrategy.GetTtl(new Context("someOperationKey"), null);

        actual.Timespan.ShouldBe(TimeSpan.Zero);
        actual.SlidingExpiration.ShouldBeFalse();
    }

    [Fact]
    public void Should_return_timespan_reflecting_time_until_expiry()
    {
        DateTimeOffset today = DateTimeOffset.UtcNow.Date;
        DateTimeOffset tomorrow = today.AddDays(1);

        AbsoluteTtl ttlStrategy = new AbsoluteTtl(tomorrow);

        SystemClock.DateTimeOffsetUtcNow = () => today;
        var actual = ttlStrategy.GetTtl(new Context("someOperationKey"), null);

        actual.Timespan.ShouldBe(TimeSpan.FromDays(1));
        actual.SlidingExpiration.ShouldBeFalse();
    }

    [Fact]
    public void Should_return_zero_ttl_if_configured_to_expire_now()
    {
        SystemClock.DateTimeOffsetUtcNow = () => new(2025, 02, 16, 12, 34, 56, TimeSpan.Zero);

        AbsoluteTtl ttlStrategy = new AbsoluteTtl(SystemClock.DateTimeOffsetUtcNow());

        var actual = ttlStrategy.GetTtl(new Context("someOperationKey"), null);

        actual.Timespan.ShouldBe(TimeSpan.Zero);
        actual.SlidingExpiration.ShouldBeFalse();
    }

    public void Dispose() =>
        SystemClock.Reset();
}
