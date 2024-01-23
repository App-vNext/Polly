using Polly.Simmy;

public class ChaosStrategyConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        ChaosStrategyConstants.MinInjectionThreshold.Should().Be(0d);
        ChaosStrategyConstants.MaxInjectionThreshold.Should().Be(1d);
        ChaosStrategyConstants.DefaultInjectionRate.Should().Be(0.001d);
        ChaosStrategyConstants.DefaultEnabled.Should().BeTrue();
    }
}
