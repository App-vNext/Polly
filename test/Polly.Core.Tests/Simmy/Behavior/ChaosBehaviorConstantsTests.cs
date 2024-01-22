using Polly.Simmy.Behavior;

namespace Polly.Core.Tests.Simmy.Behavior;

public class ChaosBehaviorConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        ChaosBehaviorConstants.DefaultName.Should().Be("Chaos.Behavior");
        ChaosBehaviorConstants.OnBehaviorInjectedEvent.Should().Be("Chaos.OnBehavior");
    }
}
