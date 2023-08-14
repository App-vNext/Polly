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

        using var limiter = ResilienceRateLimiter.Create(rateLimiter);

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

        using var limiter = ResilienceRateLimiter.Create(rateLimiter);

        (await limiter.AcquireAsync(context)).Should().Be(lease);
        limiter.PartitionedLimiter.Should().NotBeNull();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task RateLimiter_Dispose_EnsureDisposed(bool isAsync)
    {
        using var concurrencyLimiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions { PermitLimit = 10, QueueLimit = 10 });
        var limiter = ResilienceRateLimiter.Create(concurrencyLimiter);

        if (isAsync)
        {
            await limiter.DisposeAsync();
        }
        else
        {
            limiter.Dispose();
        }

        await limiter.Invoking(l => l.AcquireAsync(ResilienceContextPool.Shared.Get()).AsTask()).Should().ThrowAsync<ObjectDisposedException>();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task PartitionedRateLimiter_Dispose_EnsureDisposed(bool isAsync)
    {
        using var partitioned = PartitionedRateLimiter.Create<ResilienceContext, string>(
            c => RateLimitPartition.GetConcurrencyLimiter("a",
            _ => new ConcurrencyLimiterOptions { PermitLimit = 10, QueueLimit = 10 }));
        var limiter = ResilienceRateLimiter.Create(partitioned);

        if (isAsync)
        {
            await limiter.DisposeAsync();
        }
        else
        {
            limiter.Dispose();
        }

        await limiter.Invoking(l => l.AcquireAsync(ResilienceContextPool.Shared.Get()).AsTask()).Should().ThrowAsync<ObjectDisposedException>();
    }
}
