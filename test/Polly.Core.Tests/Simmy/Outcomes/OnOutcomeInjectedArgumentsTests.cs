using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public static class OnOutcomeInjectedArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();
        var outcome = 200;

        // Act
        var args = new OnOutcomeInjectedArguments<int>(context, new(outcome));

        // Assert
        args.Context.ShouldBe(context);
        args.Outcome.Result.ShouldBe(outcome);
    }
}
