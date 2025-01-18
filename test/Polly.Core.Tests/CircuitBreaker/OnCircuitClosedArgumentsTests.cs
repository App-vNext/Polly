using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public static class OnCircuitClosedArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();

        // Act
        var args = new OnCircuitClosedArguments<int>(context, Outcome.FromResult(1), true);

        // Assert
        args.Context.ShouldBe(context);
        args.Outcome.Result.ShouldBe(1);
        args.IsManual.ShouldBeTrue();
    }
}
