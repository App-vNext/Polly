using Polly.Simmy.Fault;

namespace Polly.Core.Tests.Simmy.Fault;

public class FaultConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        FaultConstants.OnFaultInjectedEvent.Should().Be("OnFaultInjected");
    }
}
