using Polly.Simmy;
using Polly.Simmy.Fault;

namespace Polly.Core.Tests.Simmy.Fault;

public class FaultStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new FaultStrategyOptions();
        sut.Randomizer.Should().NotBeNull();
        sut.Enabled.Should().BeFalse();
        sut.EnabledGenerator.Should().BeNull();
        sut.InjectionRate.Should().Be(MonkeyStrategyConstants.DefaultInjectionRate);
        sut.InjectionRateGenerator.Should().BeNull();
        sut.Fault.Should().BeNull();
        sut.OnFaultInjected.Should().BeNull();
        sut.FaultGenerator.Should().BeNull();
    }
}
