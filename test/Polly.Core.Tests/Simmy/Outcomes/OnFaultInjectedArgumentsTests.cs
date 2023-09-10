using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OnFaultInjectedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnFaultInjectedArguments(ResilienceContextPool.Shared.Get(), new InvalidCastException());
        args.Context.Should().NotBeNull();
        args.Fault.Should().NotBeNull();
    }
}
