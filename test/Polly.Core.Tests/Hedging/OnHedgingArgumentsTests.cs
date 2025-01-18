using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public static class OnHedgingArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var primaryContext = ResilienceContextPool.Shared.Get(cancellationToken);
        var actionContext = ResilienceContextPool.Shared.Get(cancellationToken);

        // Act
        var args = new OnHedgingArguments<int>(primaryContext, actionContext, 1);

        // Assert
        args.PrimaryContext.ShouldBe(primaryContext);
        args.ActionContext.ShouldBe(actionContext);
        args.AttemptNumber.ShouldBe(1);
    }
}
