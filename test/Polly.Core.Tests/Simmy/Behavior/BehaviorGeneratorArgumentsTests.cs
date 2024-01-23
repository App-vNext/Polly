using Polly.Simmy.Behavior;

namespace Polly.Core.Tests.Simmy.Behavior;

public class BehaviorGeneratorArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new BehaviorGeneratorArguments(ResilienceContextPool.Shared.Get());
        args.Context.Should().NotBeNull();
    }
}
