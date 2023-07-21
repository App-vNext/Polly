using Polly.Hedging;
using Polly.Hedging.Utils;

namespace Polly.Core.Tests.Hedging;

public class HedgingHandlerTests
{
    [Fact]
    public async Task GenerateAction_Generic_Ok()
    {
        var handler = new HedgingHandler<string>(
            args => PredicateResult.True,
            args => () => Outcome.FromResultAsTask("ok"),
            true);

        var action = handler.GenerateAction(new HedgingActionGeneratorArguments<string>(
            ResilienceContextPool.Shared.Get(),
            ResilienceContextPool.Shared.Get(),
            0,
            _ => Outcome.FromResultAsTask("primary")))!;
        var res = await action();

        res.Result.Should().Be("ok");
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task GenerateAction_NonGeneric_Ok(bool nullAction)
    {
        var handler = new HedgingHandler<object>(
            args => PredicateResult.True,
            args =>
            {
                if (nullAction)
                {
                    return null;
                }

                return () => Outcome.FromResultAsTask((object)"ok");
            },
            false);

        var action = handler.GenerateAction(new HedgingActionGeneratorArguments<object>(
            ResilienceContextPool.Shared.Get(),
            ResilienceContextPool.Shared.Get(),
            0,
            _ => Outcome.FromResultAsTask((object)"primary")))!;
        if (nullAction)
        {
            action.Should().BeNull();
        }
        else
        {
            var res = await action();
            res.Result.Should().Be("ok");
        }
    }

    [Fact]
    public async Task GenerateAction_NonGeneric_FromCallback()
    {
        var handler = new HedgingHandler<object>(
            args => PredicateResult.True,
            args => () => args.Callback(args.ActionContext),
            false);

        var action = handler.GenerateAction(
            new HedgingActionGeneratorArguments<object>(
                ResilienceContextPool.Shared.Get(),
                ResilienceContextPool.Shared.Get(),
                0,
                _ => Outcome.FromResultAsTask((object)"callback")))!;
        var res = await action();
        res.Result.Should().Be("callback");
    }
}
