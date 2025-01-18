using Polly.Hedging;
using Polly.Hedging.Utils;

namespace Polly.Core.Tests.Hedging;

public static class HedgingHandlerTests
{
    [Fact]
    public static async Task GenerateAction_Generic_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();

        var handler = new HedgingHandler<string>(
            args => PredicateResult.True(),
            args => () => Outcome.FromResultAsValueTask("ok"),
            args => default);

        handler.OnHedging.ShouldNotBeNull();

        var action = handler.GenerateAction(new HedgingActionGeneratorArguments<string>(
            context,
            context,
            0,
            _ => Outcome.FromResultAsValueTask("primary")))!;

        // Act
        var res = await action();

        // Assert
        res.Result.ShouldBe("ok");
    }
}
