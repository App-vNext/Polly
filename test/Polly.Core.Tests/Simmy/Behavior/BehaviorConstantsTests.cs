using Polly.Simmy.Behavior;

namespace Polly.Core.Tests.Simmy.Behavior;

public class BehaviorConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        BehaviorConstants.OnBehaviorInjectedEvent.Should().Be("OnBehaviorInjected");
        BehaviorConstants.StrategyType.Should().Be("Behavior");
    }
}
