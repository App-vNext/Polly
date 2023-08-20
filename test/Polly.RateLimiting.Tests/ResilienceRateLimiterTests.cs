using System.Threading.RateLimiting;
using NSubstitute;

namespace Polly.RateLimiting.Tests;

public class ResilienceRateLimiterTests
{
    [Fact]
    public async Task Create_RateLimiter_Ok()
    {
        var lease = Substitute.For<RateLimitLease>();
        var leaseTask = new ValueTask<RateLimitLease>(lease);

        var rateLimiter = Substitute.For<RateLimiter>();
        rateLimiter
            .GetType()
            .GetMethod("AcquireAsyncCore", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(rateLimiter, new object[] { 1, default(CancellationToken) })
            .Returns(leaseTask);

        var limiter = ResilienceRateLimiter.Create(rateLimiter);

        (await limiter.AcquireAsync(ResilienceContextPool.Shared.Get())).Should().Be(lease);
        limiter.Limiter.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_PartitionedRateLimiter_Ok()
    {
        var context = ResilienceContextPool.Shared.Get();

        var lease = Substitute.For<RateLimitLease>();
        var leaseTask = new ValueTask<RateLimitLease>(lease);

        var rateLimiter = Substitute.For<PartitionedRateLimiter<ResilienceContext>>();
        rateLimiter
            .GetType()
            .GetMethod("AcquireAsyncCore", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(rateLimiter, new object[] { context, 1, default(CancellationToken) })
            .Returns(leaseTask);

        var limiter = ResilienceRateLimiter.Create(rateLimiter);

        (await limiter.AcquireAsync(context)).Should().Be(lease);
        limiter.PartitionedLimiter.Should().NotBeNull();
    }
}
