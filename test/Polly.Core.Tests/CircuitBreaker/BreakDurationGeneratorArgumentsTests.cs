using Polly;
using Polly.CircuitBreaker;
namespace Polly.Core.Tests.CircuitBreaker;
public class BreakDurationGeneratorArgumentsTests
{
    [Fact]
    public void Constructor_ShouldSetFailureRate()
    {
        double expectedFailureRate = 0.5;
        int failureCount = 10;
        var context = new ResilienceContext();

        var args = new BreakDurationGeneratorArguments(expectedFailureRate, failureCount, context);

        args.FailureRate.Should().Be(expectedFailureRate);
    }

    [Fact]
    public void Constructor_ShouldSetFailureCount()
    {
        double failureRate = 0.5;
        int expectedFailureCount = 10;
        var context = new ResilienceContext();

        var args = new BreakDurationGeneratorArguments(failureRate, expectedFailureCount, context);

        args.FailureCount.Should().Be(expectedFailureCount);
    }

    [Fact]
    public void Constructor_ShouldSetContext()
    {
        double failureRate = 0.5;
        int failureCount = 10;
        var expectedContext = new ResilienceContext();

        var args = new BreakDurationGeneratorArguments(failureRate, failureCount, expectedContext);

        args.Context.Should().Be(expectedContext);
    }
}
