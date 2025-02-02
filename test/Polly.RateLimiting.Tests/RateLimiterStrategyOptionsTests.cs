namespace Polly.RateLimiting.Tests;

public class RateLimiterStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new RateLimiterStrategyOptions();

        options.RateLimiter.ShouldBeNull();
        options.OnRejected.ShouldBeNull();
        options.DefaultRateLimiterOptions.PermitLimit.ShouldBe(1000);
        options.DefaultRateLimiterOptions.QueueLimit.ShouldBe(0);
        options.Name.ShouldBe("RateLimiter");
    }
}
