using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class ChaosOutcomeConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        ChaosOutcomeConstants.DefaultName.Should().Be("Chaos.Outcome");
        ChaosOutcomeConstants.OnOutcomeInjectedEvent.Should().Be("Chaos.OnOutcome");
    }
}
