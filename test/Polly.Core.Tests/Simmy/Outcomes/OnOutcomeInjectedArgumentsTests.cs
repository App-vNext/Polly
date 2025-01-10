using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public static class OnOutcomeInjectedArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();

        // Act
        var args = new OnOutcomeInjectedArguments<int>(context, new(200));

        // Assert
        args.Context.Should().Be(context);
        args.Outcome.Should().NotBeNull();
    }
}
