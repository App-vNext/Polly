using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public class ChaosLatencyConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        ChaosLatencyConstants.DefaultName.ShouldBe("Chaos.Latency");
        ChaosLatencyConstants.OnLatencyInjectedEvent.ShouldBe("Chaos.OnLatency");
        ChaosLatencyConstants.DefaultLatency.ShouldBe(TimeSpan.FromSeconds(30));
    }
}
