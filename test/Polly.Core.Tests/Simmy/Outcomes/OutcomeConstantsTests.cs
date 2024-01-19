using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        OutcomeConstants.DefaultName.Should().Be("Chaos.Outcome");
        OutcomeConstants.OnOutcomeInjectedEvent.Should().Be("Chaos.OnOutcome");
    }
}
