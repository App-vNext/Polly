using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public static class OnCircuitOpenedArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);

        // Act
        var args = new OnCircuitOpenedArguments<int>(context, Outcome.FromResult(1), TimeSpan.FromSeconds(2), true);

        // Assert
        args.Context.ShouldBe(context);
        args.Outcome.Result.ShouldBe(1);
        args.BreakDuration.ShouldBe(TimeSpan.FromSeconds(2));
        args.IsManual.ShouldBeTrue();
    }
}
