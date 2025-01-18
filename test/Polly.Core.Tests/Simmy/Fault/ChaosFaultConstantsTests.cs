using Polly.Simmy.Fault;

namespace Polly.Core.Tests.Simmy.Fault;

public class ChaosFaultConstantsTests
{
    [Fact]
    public void EnsureDefaults()
    {
        ChaosFaultConstants.DefaultName.ShouldBe("Chaos.Fault");
        ChaosFaultConstants.OnFaultInjectedEvent.ShouldBe("Chaos.OnFault");
    }
}
