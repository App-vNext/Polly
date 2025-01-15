using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public static class LatencyGeneratorArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get(TestContext.Current.CancellationToken);

        // Act
        var args = new LatencyGeneratorArguments(context);

        // Assert
        args.Context.Should().NotBeNull();
    }
}
