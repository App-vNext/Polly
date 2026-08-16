using Microsoft.Extensions.Time.Testing;
using Polly.Retry;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact(Timeout = 30_000)]
    public async Task InfiniteRetry_Delay_Does_Not_Overflow_2163()
    {
        // Arrange
        int attempts = 0;
        int succeedAfter = 2049;

        var options = new RetryStrategyOptions<bool>
        {
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(30),
            MaxRetryAttempts = int.MaxValue,
            UseJitter = true,
            OnRetry = (args) =>
            {
                args.RetryDelay.ShouldBeGreaterThan(TimeSpan.Zero, $"RetryDelay is less than zero after {args.AttemptNumber} attempts");
                attempts++;
                return default;
            },
            ShouldHandle = (args) => new ValueTask<bool>(!args.Outcome.Result),
        };

        var listener = new FakeTelemetryListener();
        var telemetry = TestUtilities.CreateResilienceTelemetry(listener);
        var timeProvider = new FakeTimeProvider();

        var strategy = new RetryResilienceStrategy<bool>(options, timeProvider, telemetry);
        var pipeline = strategy.AsPipeline();

        using var timeout = new CancellationTokenSource(Debugger.IsAttached ? TimeSpan.MaxValue : TimeSpan.FromSeconds(20));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, TestContext.Current.CancellationToken);

        // Act
        var executing = pipeline.ExecuteAsync(
            (_) => new ValueTask<bool>(attempts >= succeedAfter),
            linked.Token);

        while (!executing.IsCompleted && !linked.IsCancellationRequested)
        {
            timeProvider.Advance(TimeSpan.FromSeconds(1));
        }

        // Assert
        linked.Token.ThrowIfCancellationRequested();

        var actual = await executing;

        actual.ShouldBeTrue();
        attempts.ShouldBe(succeedAfter);
    }
}
