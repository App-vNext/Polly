using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public static class HedgingActionGeneratorArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var primaryContext = ResilienceContextPool.Shared.Get(cancellationToken);
        var actionContext = ResilienceContextPool.Shared.Get(cancellationToken);

        // Act
        var args = new HedgingActionGeneratorArguments<string>(primaryContext, actionContext, 5, _ => Outcome.FromResultAsValueTask("dummy"));

        // Assert
        args.PrimaryContext.Should().Be(primaryContext);
        args.ActionContext.Should().Be(actionContext);
        args.AttemptNumber.Should().Be(5);
        args.Callback.Should().NotBeNull();
    }
}
