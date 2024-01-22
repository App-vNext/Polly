using Polly.Simmy.Fault;

namespace Polly.Core.Tests.Simmy.Fault;

public class FaultConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        FaultConstants.DefaultName.Should().Be("Chaos.Fault");
        FaultConstants.OnFaultInjectedEvent.Should().Be("Chaos.OnFault");
    }
}
