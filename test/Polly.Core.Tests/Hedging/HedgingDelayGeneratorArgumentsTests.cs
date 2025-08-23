using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public static class HedgingDelayGeneratorArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);

        // Act
        var args = new HedgingDelayGeneratorArguments(context, 5);

        // Assert
        args.Context.ShouldBe(context);
        args.AttemptNumber.ShouldBe(5);
    }
}
