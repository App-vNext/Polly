using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public class OnLatencyInjectedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnLatencyInjectedArguments(ResilienceContextPool.Shared.Get(), TimeSpan.FromSeconds(10));
        args.Context.Should().NotBeNull();
        args.Latency.Should().Be(TimeSpan.FromSeconds(10));
    }
}
