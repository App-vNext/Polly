using System.Threading.RateLimiting;
using NSubstitute;

namespace Polly.RateLimiting.Tests;

public class OnRateLimiterRejectedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnRateLimiterRejectedArguments(ResilienceContextPool.Shared.Get(), Substitute.For<RateLimitLease>());

        args.Context.Should().NotBeNull();
        args.Lease.Should().NotBeNull();
    }
}
