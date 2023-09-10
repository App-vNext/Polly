using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class FaultGeneratorArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new FaultGeneratorArguments(ResilienceContextPool.Shared.Get());
        args.Context.Should().NotBeNull();
    }
}
