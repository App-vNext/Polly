using Polly.Simmy.Fault;

namespace Polly.Core.Tests.Simmy.Fault;

public class FaultGeneratorArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new FaultGeneratorArguments(ResilienceContextPool.Shared.Get());
        args.Context.Should().NotBeNull();
    }
}
