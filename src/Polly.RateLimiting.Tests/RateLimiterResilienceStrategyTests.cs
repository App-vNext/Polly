using System.Diagnostics;
using System.Threading.RateLimiting;
using Moq;
using Moq.Protected;
using Polly.Strategy;

namespace Polly.RateLimiting.Tests;

public class RateLimiterResilienceStrategyTests
{
    private readonly Mock<RateLimiter> _limiter = new(MockBehavior.Strict);
    private readonly Mock<RateLimitLease> _lease = new(MockBehavior.Strict);
    private readonly Mock<DiagnosticSource> _diagnosticSource = new();
    private Func<OnRateLimiterRejectedArguments, ValueTask>? _event;

    [Fact]
    public void Ctor_Ok()
    {
        Create().Should().NotBeNull();
    }

    [Fact]
    public void Execute_HappyPath()
    {
        using var cts = new CancellationTokenSource();

        SetupLimiter(cts.Token);

        _lease.Setup(v => v.IsAcquired).Returns(true);
        _lease.Protected().Setup("Dispose", exactParameterMatch: true, true);

        Create().Should().NotBeNull();

        var strategy = Create();

        strategy.Execute(_ => { }, cts.Token);

        _limiter.VerifyAll();
        _lease.VerifyAll();
    }

    [InlineData(false, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [Theory]
    public void Execute_LeaseRejected(bool hasEvents, bool hasRetryAfter)
    {
        _diagnosticSource.Setup(v => v.IsEnabled("OnRateLimiterRejected")).Returns(true);
        _diagnosticSource.Setup(v => v.Write("OnRateLimiterRejected", It.Is<object>(obj => obj != null)));

        object? metadata = hasRetryAfter ? TimeSpan.FromSeconds(123) : null;

        using var cts = new CancellationTokenSource();
        var eventCalled = false;
        SetupLimiter(cts.Token);

        _lease.Setup(v => v.IsAcquired).Returns(false);
        _lease.Protected().Setup("Dispose", exactParameterMatch: true, true);
        _lease.Setup(v => v.TryGetMetadata("RETRY_AFTER", out metadata)).Returns(hasRetryAfter);

        if (hasEvents)
        {
            _event = args =>
            {
                args.Context.Should().NotBeNull();
                args.Lease.Should().Be(_lease.Object);
                args.RetryAfter.Should().Be((TimeSpan?)metadata);
                eventCalled = true;
                return default;
            };
        }

        var strategy = Create();

        var assertion = strategy
            .Invoking(s => s.Execute(_ => { }, cts.Token))
            .Should()
            .Throw<RateLimiterRejectedException>()
            .And
            .RetryAfter.Should().Be((TimeSpan?)metadata);

        _limiter.VerifyAll();
        _lease.VerifyAll();
        eventCalled.Should().Be(hasEvents);

        _diagnosticSource.VerifyAll();
    }

    private void SetupLimiter(CancellationToken token) => _limiter
                .Protected()
                .Setup<ValueTask<RateLimitLease>>("AcquireAsyncCore", 1, token)
                .Returns(new ValueTask<RateLimitLease>(_lease.Object));

    private RateLimiterResilienceStrategy Create()
    {
        var builder = new ResilienceStrategyBuilder();
        builder.Properties.Set(new ResiliencePropertyKey<DiagnosticSource>("DiagnosticSource"), _diagnosticSource.Object);

        return (RateLimiterResilienceStrategy)builder
            .AddRateLimiter(new RateLimiterStrategyOptions
            {
                RateLimiter = _limiter.Object,
                OnRejected = _event
            })
            .Build();
    }
}
