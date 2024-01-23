using Polly.Simmy;
using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public class ChaosLatencyStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new ChaosLatencyStrategyOptions();
        sut.Randomizer.Should().NotBeNull();
        sut.Enabled.Should().BeTrue();
        sut.EnabledGenerator.Should().BeNull();
        sut.InjectionRate.Should().Be(ChaosStrategyConstants.DefaultInjectionRate);
        sut.InjectionRateGenerator.Should().BeNull();
        sut.Latency.Should().Be(ChaosLatencyConstants.DefaultLatency);
        sut.LatencyGenerator.Should().BeNull();
        sut.OnLatencyInjected.Should().BeNull();
    }
}
