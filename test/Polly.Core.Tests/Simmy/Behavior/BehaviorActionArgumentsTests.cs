using Polly.Simmy.Behavior;

namespace Polly.Core.Tests.Simmy.Behavior;

public class BehaviorActionArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new BehaviorActionArguments(ResilienceContextPool.Shared.Get());
        args.Context.Should().NotBeNull();
    }
}
