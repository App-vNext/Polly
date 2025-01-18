using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public static class OutcomeGeneratorArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();

        // Act
        var args = new OutcomeGeneratorArguments(context);

        // Assert
        args.Context.ShouldNotBeNull();
    }
}
