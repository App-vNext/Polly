using Polly;
using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class BreakDurationGeneratorArgumentsTests
{
    [Fact]
    public void Constructor_Old_Ok()
    {
        double expectedFailureRate = 0.5;
        int failureCount = 10;
        var context = new ResilienceContext();
        var outcome = Outcome.FromResult(42);

        var args = new BreakDurationGeneratorArguments<int>(expectedFailureRate, failureCount, context, outcome);

        args.FailureRate.Should().Be(expectedFailureRate);
        args.FailureCount.Should().Be(failureCount);
        args.Context.Should().Be(context);
        args.Outcome.Should().Be(outcome);
    }

    [Fact]
    public void Constructor_Ok()
    {
        double expectedFailureRate = 0.5;
        int failureCount = 10;
        var context = new ResilienceContext();
        var outcome = Outcome.FromResult(42);

        var args = new BreakDurationGeneratorArguments<int>(expectedFailureRate, failureCount, context, 99, outcome);

        args.FailureRate.Should().Be(expectedFailureRate);
        args.FailureCount.Should().Be(failureCount);
        args.Context.Should().Be(context);
        args.HalfOpenAttempts.Should().Be(99);
        args.Outcome.Should().Be(outcome);
    }
}
