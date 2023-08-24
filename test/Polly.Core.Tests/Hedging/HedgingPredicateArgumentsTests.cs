using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class HedgingPredicateArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new HedgingPredicateArguments<int>(ResilienceContextPool.Shared.Get(), Outcome.FromResult(1));

        args.Context.Should().NotBeNull();
        args.Outcome.Result.Should().Be(1);
    }
}
