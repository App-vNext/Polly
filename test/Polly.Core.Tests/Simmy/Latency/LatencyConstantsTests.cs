using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public class LatencyConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        LatencyConstants.DefaultName.Should().Be("Chaos.Latency");
        LatencyConstants.OnLatencyInjectedEvent.Should().Be("Chaos.OnLatency");
        LatencyConstants.DefaultLatency.Should().Be(TimeSpan.FromSeconds(30));
    }
}
