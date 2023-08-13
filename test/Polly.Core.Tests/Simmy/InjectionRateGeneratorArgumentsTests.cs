using Polly.Simmy;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class InjectionRateGeneratorArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new InjectionRateGeneratorArguments(ResilienceContextPool.Shared.Get());
        args.Context.Should().NotBeNull();
    }
}
