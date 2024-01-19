using Polly.Simmy;
using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public class LatencyStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new LatencyStrategyOptions();
        sut.Randomizer.Should().NotBeNull();
        sut.Enabled.Should().BeFalse();
        sut.EnabledGenerator.Should().BeNull();
        sut.InjectionRate.Should().Be(MonkeyStrategyConstants.DefaultInjectionRate);
        sut.InjectionRateGenerator.Should().BeNull();
        sut.Latency.Should().Be(LatencyConstants.DefaultLatency);
        sut.LatencyGenerator.Should().BeNull();
        sut.OnLatencyInjected.Should().BeNull();
    }
}
