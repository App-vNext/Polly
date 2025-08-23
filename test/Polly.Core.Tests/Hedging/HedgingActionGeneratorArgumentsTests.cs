using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public static class HedgingActionGeneratorArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var cancellationToken = TestCancellation.Token;
        var primaryContext = ResilienceContextPool.Shared.Get(cancellationToken);
        var actionContext = ResilienceContextPool.Shared.Get(cancellationToken);

        // Act
        var args = new HedgingActionGeneratorArguments<string>(primaryContext, actionContext, 5, _ => Outcome.FromResultAsValueTask("dummy"));

        // Assert
        args.PrimaryContext.ShouldBe(primaryContext);
        args.ActionContext.ShouldBe(actionContext);
        args.AttemptNumber.ShouldBe(5);
        args.Callback.ShouldNotBeNull();
    }
}
