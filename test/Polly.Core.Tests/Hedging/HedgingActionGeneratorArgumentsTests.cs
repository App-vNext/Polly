using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class HedgingActionGeneratorArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new HedgingActionGeneratorArguments<string>(ResilienceContextPool.Shared.Get(), ResilienceContextPool.Shared.Get(), 5, _ => Outcome.FromResultAsTask("dummy"));

        args.PrimaryContext.Should().NotBeNull();
        args.ActionContext.Should().NotBeNull();
        args.Attempt.Should().Be(5);
        args.Callback.Should().NotBeNull();
    }
}
