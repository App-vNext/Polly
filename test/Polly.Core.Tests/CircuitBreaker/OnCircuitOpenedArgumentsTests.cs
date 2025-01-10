using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public static class OnCircuitOpenedArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();

        // Act
        var args = new OnCircuitOpenedArguments<int>(context, Outcome.FromResult(1), TimeSpan.FromSeconds(2), true);

        // Assert
        args.Context.Should().Be(context);
        args.Outcome.Result.Should().Be(1);
        args.BreakDuration.Should().Be(TimeSpan.FromSeconds(2));
        args.IsManual.Should().BeTrue();
    }
}
