using System.Threading.RateLimiting;
using NSubstitute;

namespace Polly.RateLimiting.Tests;

public static class OnRateLimiterRejectedArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        var args = new OnRateLimiterRejectedArguments(
            ResilienceContextPool.Shared.Get(CancellationToken.None),
            Substitute.For<RateLimitLease>());

        args.Context.Should().NotBeNull();
        args.Lease.Should().NotBeNull();
    }
}
