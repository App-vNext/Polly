using Microsoft.Extensions.Time.Testing;
using Polly.Retry;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact(Timeout = 15_000)]
    public async Task InfiniteRetry_Delay_Does_Not_Overflow_2163()
    {
        // Arrange
        var options = new RetryStrategyOptions
        {
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(30),
            MaxRetryAttempts = int.MaxValue,
            UseJitter = true,
            OnRetry = (args) =>
            {
                args.RetryDelay.Should().BeGreaterThan(TimeSpan.Zero, $"RetryDelay is less than zero after {args.AttemptNumber} attempts");
                return default;
            },
        };

        var listener = new FakeTelemetryListener();
        var telemetry = TestUtilities.CreateResilienceTelemetry(listener);
        var timeProvider = new FakeTimeProvider();

        var strategy = new RetryResilienceStrategy<object>(options, timeProvider, telemetry);
        var pipeline = strategy.AsPipeline();

        int attempts = 0;
        int succeedAfter = 2049;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        var executing = pipeline.ExecuteAsync((_) =>
        {
            if (attempts++ < succeedAfter)
            {
                throw new InvalidOperationException("Simulated exception");
            }

            return new ValueTask<bool>(true);
        }, cts.Token);

        while (!executing.IsCompleted && !cts.IsCancellationRequested)
        {
            timeProvider.Advance(TimeSpan.FromSeconds(1));
        }

        // Assert
        cts.Token.ThrowIfCancellationRequested();

        var actual = await executing;

        actual.Should().BeTrue();
        attempts.Should().Be(succeedAfter);
    }
}
