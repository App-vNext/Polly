using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public static class OnLatencyInjectedArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();

        // Act
        var args = new OnLatencyInjectedArguments(context, TimeSpan.FromSeconds(10));

        // Assert
        args.Context.Should().Be(context);
        args.Latency.Should().Be(TimeSpan.FromSeconds(10));
    }
}
