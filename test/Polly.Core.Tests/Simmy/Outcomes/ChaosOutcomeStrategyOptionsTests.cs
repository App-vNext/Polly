using Polly.Simmy;
using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class ChaosOutcomeStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new ChaosOutcomeStrategyOptions<int>();
        sut.Randomizer.ShouldNotBeNull();
        sut.Enabled.ShouldBeTrue();
        sut.EnabledGenerator.ShouldBeNull();
        sut.InjectionRate.ShouldBe(ChaosStrategyConstants.DefaultInjectionRate);
        sut.InjectionRateGenerator.ShouldBeNull();
        sut.OnOutcomeInjected.ShouldBeNull();
        sut.OutcomeGenerator.ShouldBeNull();
    }
}
