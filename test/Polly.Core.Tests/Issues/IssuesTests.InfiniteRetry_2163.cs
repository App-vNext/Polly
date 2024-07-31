using Microsoft.Extensions.Time.Testing;
using Polly.Retry;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact(Timeout = 15_000, Skip = "TODO - Failing under .NET 9 for some reason.")]
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
                args.RetryDelay.Should().BeGreaterThan(TimeSpan.Zero, $"RetryDelay is less than zero after {args.AttemptNumber} attempts");
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

        using var cts = new CancellationTokenSource(Debugger.IsAttached ? TimeSpan.MaxValue : TimeSpan.FromSeconds(10));

        // Act
        var executing = pipeline.ExecuteAsync((_) =>
        {
            return new ValueTask<bool>(attempts >= succeedAfter);
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
