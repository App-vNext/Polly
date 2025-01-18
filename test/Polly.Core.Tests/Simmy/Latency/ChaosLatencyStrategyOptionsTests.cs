using Polly.Simmy;
using Polly.Simmy.Latency;

namespace Polly.Core.Tests.Simmy.Latency;

public class ChaosLatencyStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new ChaosLatencyStrategyOptions();
        sut.Randomizer.ShouldNotBeNull();
        sut.Enabled.ShouldBeTrue();
        sut.EnabledGenerator.ShouldBeNull();
        sut.InjectionRate.ShouldBe(ChaosStrategyConstants.DefaultInjectionRate);
        sut.InjectionRateGenerator.ShouldBeNull();
        sut.Latency.ShouldBe(ChaosLatencyConstants.DefaultLatency);
        sut.LatencyGenerator.ShouldBeNull();
        sut.OnLatencyInjected.ShouldBeNull();
    }
}
