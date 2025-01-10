using Polly.Simmy.Behavior;

namespace Polly.Core.Tests.Simmy.Behavior;

public static class OnBehaviorInjectedArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();

        // Act
        var args = new OnBehaviorInjectedArguments(context);

        // Assert
        args.Context.Should().Be(context);
    }
}
