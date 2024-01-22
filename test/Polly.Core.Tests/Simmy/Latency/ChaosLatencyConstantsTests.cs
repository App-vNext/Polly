using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public class ChaosLatencyConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        ChaosLatencyConstants.DefaultName.Should().Be("Chaos.Latency");
        ChaosLatencyConstants.OnLatencyInjectedEvent.Should().Be("Chaos.OnLatency");
        ChaosLatencyConstants.DefaultLatency.Should().Be(TimeSpan.FromSeconds(30));
    }
}
