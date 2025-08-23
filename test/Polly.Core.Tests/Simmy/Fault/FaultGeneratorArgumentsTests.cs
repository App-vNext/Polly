using Polly.Simmy.Fault;

namespace Polly.Core.Tests.Simmy.Fault;

public static class FaultGeneratorArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);

        // Act
        var args = new FaultGeneratorArguments(context);

        // Assert
        args.Context.ShouldBe(context);
    }
}
