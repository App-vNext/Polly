using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        OutcomeConstants.OnOutcomeInjectedEvent.Should().Be("OnOutcomeInjected");
    }
}
