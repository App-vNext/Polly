using Polly.Simmy.Behavior;

namespace Polly.Core.Tests.Simmy.Behavior;

public class BehaviorConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        BehaviorConstants.DefaultName.Should().Be("Chaos.Behavior");
        BehaviorConstants.OnBehaviorInjectedEvent.Should().Be("Chaos.OnBehavior");
    }
}
