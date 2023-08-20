using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public class OnLatencyArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnLatencyArguments(ResilienceContextPool.Shared.Get(), TimeSpan.FromSeconds(10));
        args.Context.Should().NotBeNull();
        args.Latency.Should().Be(TimeSpan.FromSeconds(10));
    }
}
