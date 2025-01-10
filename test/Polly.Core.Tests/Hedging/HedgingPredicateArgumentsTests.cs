using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public static class HedgingPredicateArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();

        // Act
        var args = new HedgingPredicateArguments<int>(context, Outcome.FromResult(1));

        // Assert
        args.Context.Should().Be(context);
        args.Outcome.Result.Should().Be(1);
    }
}
