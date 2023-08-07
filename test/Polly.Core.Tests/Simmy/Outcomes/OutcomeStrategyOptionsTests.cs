using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new OutcomeStrategyOptions<int>();
        sut.Randomizer.Should().NotBeNull();
        sut.Enabled.Should().BeNull();
        sut.EnabledGenerator.Should().BeNull();
        sut.InjectionRate.Should().BeNull();
        sut.InjectionRateGenerator.Should().BeNull();
        sut.Outcome.Should().Be(default(Outcome<int>));
        sut.OnOutcomeInjected.Should().BeNull();
        sut.OutcomeGenerator.Should().BeNull();
    }
}
