using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public static class OnLatencyInjectedArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);

        // Act
        var args = new OnLatencyInjectedArguments(context, TimeSpan.FromSeconds(10));

        // Assert
        args.Context.ShouldBe(context);
        args.Latency.ShouldBe(TimeSpan.FromSeconds(10));
    }
}
