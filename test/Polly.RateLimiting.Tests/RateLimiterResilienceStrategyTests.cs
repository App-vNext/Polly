using System.Threading.RateLimiting;
using NSubstitute;
using Polly.TestUtils;

namespace Polly.RateLimiting.Tests;

public class RateLimiterResilienceStrategyTests
{
    private readonly RateLimiter _limiter = Substitute.For<RateLimiter>();
    private readonly RateLimitLease _lease = Substitute.For<RateLimitLease>();
    private readonly FakeTelemetryListener _listener = new();
    private Func<OnRateLimiterRejectedArguments, ValueTask>? _event;

    [Fact]
    public void Ctor_Ok()
    {
        Create().Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_HappyPath()
    {
        using var cts = new CancellationTokenSource();

        SetupLimiter(cts.Token);

        _lease.IsAcquired.Returns(true);

        Create().Should().NotBeNull();

        var strategy = Create();

        strategy.Execute(_ => { }, cts.Token);

        await _limiter.ReceivedWithAnyArgs().AcquireAsync(default, default);
        _lease.Received().Dispose();
    }

    [InlineData(false, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [Theory]
    public async Task Execute_LeaseRejected(bool hasEvents, bool hasRetryAfter)
    {
        object? metadata = hasRetryAfter ? TimeSpan.FromSeconds(123) : null;

        using var cts = new CancellationTokenSource();
        var eventCalled = false;
        SetupLimiter(cts.Token);

        _lease.IsAcquired.Returns(false);
        _lease.TryGetMetadata("RETRY_AFTER", out Arg.Any<object?>())
              .Returns(x =>
              {
                  x[1] = hasRetryAfter ? metadata : null;
                  return hasRetryAfter;
              });

        if (hasEvents)
        {
            _event = args =>
            {
                args.Context.Should().NotBeNull();
                args.Lease.Should().Be(_lease);
                args.RetryAfter.Should().Be((TimeSpan?)metadata);
                eventCalled = true;
                return default;
            };
        }

        var strategy = Create();
        var context = ResilienceContextPool.Shared.Get(cts.Token);
        var outcome = await strategy.ExecuteOutcomeAsync((_, _) => Outcome.FromResultAsTask("dummy"), context, "state");

        outcome.Exception
            .Should()
            .BeOfType<RateLimiterRejectedException>().Subject
            .RetryAfter
            .Should().Be((TimeSpan?)metadata);

        outcome.Exception!.StackTrace.Should().Contain("Execute_LeaseRejected");

        eventCalled.Should().Be(hasEvents);

        await _limiter.ReceivedWithAnyArgs().AcquireAsync(default, default);
        _lease.Received().Dispose();

        _listener.GetArgs<OnRateLimiterRejectedArguments>().Should().HaveCount(1);
    }

    private void SetupLimiter(CancellationToken token)
    {
        var result = new ValueTask<RateLimitLease>(_lease);
        _limiter
            .GetType()
            .GetMethod("AcquireAsyncCore", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(_limiter, new object[] { 1, token })
            .Returns(result);
    }

    private ResiliencePipeline Create()
    {
        var builder = new ResiliencePipelineBuilder
        {
            TelemetryListener = _listener
        };

        return builder.AddRateLimiter(new RateLimiterStrategyOptions
        {
            RateLimiter = ResilienceRateLimiter.Create(_limiter),
            OnRejected = _event
        })
        .Build();
    }
}
