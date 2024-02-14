namespace Polly.RateLimiting.Tests;

public class RateLimiterConstantsTests
{
    [Fact]
    public void Ctor_EnsureDefaults() =>
        RateLimiterConstants.OnRateLimiterRejectedEvent.Should().Be("OnRateLimiterRejected");
}
