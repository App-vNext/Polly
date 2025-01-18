using Polly.Simmy;

namespace Polly.Core.Tests;

public class ChaosStrategyConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        ChaosStrategyConstants.MinInjectionThreshold.ShouldBe(0d);
        ChaosStrategyConstants.MaxInjectionThreshold.ShouldBe(1d);
        ChaosStrategyConstants.DefaultInjectionRate.ShouldBe(0.001d);
        ChaosStrategyConstants.DefaultEnabled.ShouldBeTrue();
    }
}
