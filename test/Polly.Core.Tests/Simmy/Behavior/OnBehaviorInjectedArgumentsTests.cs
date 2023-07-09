using Polly.Simmy.Behavior;

namespace Polly.Core.Tests.Simmy.Behavior;

public class OnBehaviorInjectedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnBehaviorInjectedArguments(ResilienceContext.Get());
        args.Context.Should().NotBeNull();
    }
}
