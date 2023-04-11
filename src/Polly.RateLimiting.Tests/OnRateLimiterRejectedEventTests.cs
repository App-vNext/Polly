namespace Polly.RateLimiting.Tests;

public class OnRateLimiterRejectedEventTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var ev = new OnRateLimiterRejectedEvent();

        ev.Should().NotBeNull();
    }
}
