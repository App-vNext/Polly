using Polly.Simmy.Behavior;

namespace Polly.Core.Tests.Simmy.Behavior;

public static class BehaviorGeneratorArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();

        // Act
        var args = new BehaviorGeneratorArguments(context);

        // Assert
        args.Context.Should().Be(context);
    }
}
