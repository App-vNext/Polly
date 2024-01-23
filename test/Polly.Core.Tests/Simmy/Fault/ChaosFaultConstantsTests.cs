using Polly.Simmy.Fault;

namespace Polly.Core.Tests.Simmy.Fault;

public class ChaosFaultConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        ChaosFaultConstants.DefaultName.Should().Be("Chaos.Fault");
        ChaosFaultConstants.OnFaultInjectedEvent.Should().Be("Chaos.OnFault");
    }
}
