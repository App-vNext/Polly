using System.Threading.RateLimiting;
using Moq;

namespace Polly.RateLimiting.Tests;

public class OnRateLimiterRejectedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnRateLimiterRejectedArguments(ResilienceContextPool.Shared.Get(), Mock.Of<RateLimitLease>(), TimeSpan.FromSeconds(1));

        args.Context.Should().NotBeNull();
        args.Lease.Should().NotBeNull();
        args.RetryAfter.Should().Be(TimeSpan.FromSeconds(1));
    }
}
