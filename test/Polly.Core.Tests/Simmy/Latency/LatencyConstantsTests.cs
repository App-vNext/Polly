using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public class LatencyConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        LatencyConstants.OnLatencyEvent.Should().Be("OnLatency");
        LatencyConstants.DefaultLatency.Should().Be(TimeSpan.FromSeconds(30));
    }
}
