namespace Polly.Specs.Caching;

public class SlidingTtlSpecs
{
    [Fact]
    public void Should_throw_when_timespan_is_less_than_zero()
    {
        Action configure = () => _ = new SlidingTtl(TimeSpan.FromMilliseconds(-1));

        Should.Throw<ArgumentOutOfRangeException>(configure).ParamName.ShouldBe("slidingTtl");
    }

    [Fact]
    public void Should_not_throw_when_timespan_is_zero()
    {
        Action configure = () => _ = new SlidingTtl(TimeSpan.Zero);

        Should.NotThrow(configure);
    }

    [Fact]
    public void Should_allow_timespan_max_value()
    {
        Action configure = () => _ = new SlidingTtl(TimeSpan.MaxValue);

        Should.NotThrow(configure);
    }

    [Fact]
    public void Should_return_configured_timespan()
    {
        TimeSpan ttl = TimeSpan.FromSeconds(30);

        SlidingTtl ttlStrategy = new SlidingTtl(ttl);

        Ttl retrieved = ttlStrategy.GetTtl(new Context("someOperationKey"), null);
        retrieved.Timespan.ShouldBe(ttl);
        retrieved.SlidingExpiration.ShouldBeTrue();
    }
}
