using Polly.Simmy;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class EnabledGeneratorArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new EnabledGeneratorArguments(ResilienceContextPool.Shared.Get());
        args.Context.Should().NotBeNull();
    }
}
