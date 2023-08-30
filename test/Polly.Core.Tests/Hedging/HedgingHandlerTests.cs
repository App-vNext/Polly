using Polly.Hedging;
using Polly.Hedging.Utils;

namespace Polly.Core.Tests.Hedging;

public class HedgingHandlerTests
{
    [Fact]
    public async Task GenerateAction_Generic_Ok()
    {
        var handler = new HedgingHandler<string>(
            args => PredicateResult.True(),
            args => () => Outcome.FromResultAsValueTask("ok"));

        var action = handler.GenerateAction(new HedgingActionGeneratorArguments<string>(
            ResilienceContextPool.Shared.Get(),
            ResilienceContextPool.Shared.Get(),
            0,
            _ => Outcome.FromResultAsValueTask("primary")))!;
        var res = await action();

        res.Result.Should().Be("ok");
    }
}
