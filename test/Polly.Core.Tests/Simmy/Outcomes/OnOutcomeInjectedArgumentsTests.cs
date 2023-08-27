using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OnOutcomeInjectedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnOutcomeInjectedArguments<int>(ResilienceContextPool.Shared.Get(), new(200));
        args.Context.Should().NotBeNull();
    }
}
