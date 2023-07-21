namespace Polly.RateLimiting.Tests;

public class RateLimiterStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new RateLimiterStrategyOptions();

        options.RateLimiter.Should().BeNull();
        options.OnRejected.Should().BeNull();
        options.DefaultRateLimiterOptions.PermitLimit.Should().Be(1000);
        options.DefaultRateLimiterOptions.QueueLimit.Should().Be(0);
    }
}
