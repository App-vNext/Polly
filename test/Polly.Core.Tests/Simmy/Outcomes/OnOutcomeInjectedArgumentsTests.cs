using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OnOutcomeInjectedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnOutcomeInjectedArguments(ResilienceContextPool.Shared.Get());
        args.Context.Should().NotBeNull();
    }
}
