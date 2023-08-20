using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public class LatencyGeneratorArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new LatencyGeneratorArguments(ResilienceContextPool.Shared.Get());
        args.Context.Should().NotBeNull();
    }
}
