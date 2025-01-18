using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class ChaosOutcomeConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        ChaosOutcomeConstants.DefaultName.ShouldBe("Chaos.Outcome");
        ChaosOutcomeConstants.OnOutcomeInjectedEvent.ShouldBe("Chaos.OnOutcome");
    }
}
