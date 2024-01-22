using Polly.Simmy;
using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new OutcomeStrategyOptions<int>();
        sut.Randomizer.Should().NotBeNull();
        sut.Enabled.Should().BeFalse();
        sut.EnabledGenerator.Should().BeNull();
        sut.InjectionRate.Should().Be(ChaosStrategyConstants.DefaultInjectionRate);
        sut.InjectionRateGenerator.Should().BeNull();
        sut.OnOutcomeInjected.Should().BeNull();
        sut.OutcomeGenerator.Should().BeNull();
    }
}
