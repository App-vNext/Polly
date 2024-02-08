namespace Polly.Specs.Caching;

public class RelativeTtlSpecs
{
    [Fact]
    public void Should_throw_when_timespan_is_less_than_zero()
    {
        Action configure = () =>
        {
            _ = new RelativeTtl(TimeSpan.FromMilliseconds(-1));
        };

        configure.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("ttl");
    }

    [Fact]
    public void Should_not_throw_when_timespan_is_zero()
    {
        Action configure = () =>
        {
            _ = new RelativeTtl(TimeSpan.Zero);
        };

        configure.Should().NotThrow();
    }

    [Fact]
    public void Should_allow_timespan_max_value()
    {
        Action configure = () =>
        {
            _ = new RelativeTtl(TimeSpan.MaxValue);
        };

        configure.Should().NotThrow();
    }

    [Fact]
    public void Should_return_configured_timespan()
    {
        TimeSpan ttl = TimeSpan.FromSeconds(30);

        RelativeTtl ttlStrategy = new RelativeTtl(ttl);

        Ttl retrieved = ttlStrategy.GetTtl(new Context("someOperationKey"), null);
        retrieved.Timespan.Should().BeCloseTo(ttl, TimeSpan.Zero);
        retrieved.SlidingExpiration.Should().BeFalse();
    }

    [Fact]
    public void Should_return_configured_timespan_from_time_requested()
    {
        DateTimeOffset fixedTime = SystemClock.DateTimeOffsetUtcNow();
        TimeSpan ttl = TimeSpan.FromSeconds(30);
        TimeSpan delay = TimeSpan.FromSeconds(5);

        RelativeTtl ttlStrategy = new RelativeTtl(ttl);

        SystemClock.DateTimeOffsetUtcNow = () => fixedTime.Add(delay);

        Ttl retrieved = ttlStrategy.GetTtl(new Context("someOperationKey"), null);
        retrieved.Timespan.Should().BeCloseTo(ttl, TimeSpan.Zero);
        retrieved.SlidingExpiration.Should().BeFalse();
    }
}
