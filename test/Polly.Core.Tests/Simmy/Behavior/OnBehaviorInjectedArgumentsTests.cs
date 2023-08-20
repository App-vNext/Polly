using Polly.Simmy.Behavior;

namespace Polly.Core.Tests.Simmy.Behavior;

public class OnBehaviorInjectedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnBehaviorInjectedArguments(ResilienceContextPool.Shared.Get());
        args.Context.Should().NotBeNull();
    }
}
