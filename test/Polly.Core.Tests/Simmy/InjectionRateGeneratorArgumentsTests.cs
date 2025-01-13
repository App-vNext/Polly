using Polly.Simmy;

namespace Polly.Core.Tests.Simmy.Outcomes;

public static class InjectionRateGeneratorArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();

        // Act
        var args = new InjectionRateGeneratorArguments(context);

        // Assert
        args.Context.Should().Be(context);
    }
}
