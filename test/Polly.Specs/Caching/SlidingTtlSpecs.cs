namespace Polly.Specs.Caching;

public class SlidingTtlSpecs
{
    [Fact]
    public void Should_throw_when_timespan_is_less_than_zero()
    {
        Action configure = () =>
        {
            SlidingTtl ttl = new SlidingTtl(TimeSpan.FromMilliseconds(-1));
        };

        configure.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("slidingTtl");
    }

    [Fact]
    public void Should_not_throw_when_timespan_is_zero()
    {
        Action configure = () =>
        {
            SlidingTtl ttl = new SlidingTtl(TimeSpan.Zero);
        };

        configure.Should().NotThrow();
    }

    [Fact]
    public void Should_allow_timespan_max_value()
    {
        Action configure = () =>
        {
            SlidingTtl ttl = new SlidingTtl(TimeSpan.MaxValue);
        };

        configure.Should().NotThrow();
    }

    [Fact]
    public void Should_return_configured_timespan()
    {
        TimeSpan ttl = TimeSpan.FromSeconds(30);

        SlidingTtl ttlStrategy = new SlidingTtl(ttl);

        Ttl retrieved = ttlStrategy.GetTtl(new Context("someOperationKey"), null);
        retrieved.Timespan.Should().Be(ttl);
        retrieved.SlidingExpiration.Should().BeTrue();
    }
}
