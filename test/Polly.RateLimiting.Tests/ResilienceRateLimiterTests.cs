using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace Polly.RateLimiting.Tests;

public class ResilienceRateLimiterTests
{
    [Fact]
    public async Task Create_RateLimiter_Ok()
    {
        var lease = Mock.Of<RateLimitLease>();
        var limiterMock = new Mock<RateLimiter>(MockBehavior.Strict);
        limiterMock.Protected().Setup<ValueTask<RateLimitLease>>("AcquireAsyncCore", 1, default(CancellationToken)).ReturnsAsync(lease);

        var limiter = ResilienceRateLimiter.Create(limiterMock.Object);

        (await limiter.AcquireAsync(ResilienceContextPool.Shared.Get())).Should().Be(lease);
        limiter.Limiter.Should().NotBeNull();
        limiterMock.VerifyAll();
    }

    [Fact]
    public async Task Create_PartitionedRateLimiter_Ok()
    {
        var context = ResilienceContextPool.Shared.Get();
        var lease = Mock.Of<RateLimitLease>();
        var limiterMock = new Mock<PartitionedRateLimiter<ResilienceContext>>(MockBehavior.Strict);
        limiterMock.Protected().Setup<ValueTask<RateLimitLease>>("AcquireAsyncCore", context, 1, default(CancellationToken)).ReturnsAsync(lease);

        var limiter = ResilienceRateLimiter.Create(limiterMock.Object);

        (await limiter.AcquireAsync(context)).Should().Be(lease);
        limiter.PartitionedLimiter.Should().NotBeNull();
        limiterMock.VerifyAll();
    }
}
