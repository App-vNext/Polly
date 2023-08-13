using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeGeneratorArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OutcomeGeneratorArguments(ResilienceContextPool.Shared.Get());
        args.Context.Should().NotBeNull();
    }
}
