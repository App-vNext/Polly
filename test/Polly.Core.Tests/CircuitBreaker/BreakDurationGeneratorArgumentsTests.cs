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

        var args = new BreakDurationGeneratorArguments(expectedFailureRate, failureCount, context);

        args.FailureRate.ShouldBe(expectedFailureRate);
        args.FailureCount.ShouldBe(failureCount);
        args.Context.ShouldBe(context);
    }

    [Fact]
    public void Constructor_Ok()
    {
        double expectedFailureRate = 0.5;
        int failureCount = 10;
        var context = new ResilienceContext();

        var args = new BreakDurationGeneratorArguments(expectedFailureRate, failureCount, context, 99);

        args.FailureRate.ShouldBe(expectedFailureRate);
        args.FailureCount.ShouldBe(failureCount);
        args.Context.ShouldBe(context);
        args.HalfOpenAttempts.ShouldBe(99);
    }
}
