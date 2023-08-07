using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        OutcomeConstants.OnFaultInjectedEvent.Should().Be("OnFaultInjectedEvent");
        OutcomeConstants.OnOutcomeInjectedEvent.Should().Be("OnOutcomeInjected");
    }
}
