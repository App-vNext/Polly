using FluentAssertions;
using Polly.Hedging;
using Polly.Hedging.Utils;
using Polly.Utils;

namespace Polly.Core.Tests.Hedging;

public class HedgingHandlerTests
{
    [Fact]
    public async Task GenerateAction_Generic_Ok()
    {
        var handler = new HedgingHandler<string>(
            PredicateInvoker<HandleHedgingArguments>.Create<string>(args => PredicateResult.True, true)!,
            args => () => "ok".AsOutcomeAsync(),
            true);

        var action = handler.GenerateAction(new HedgingActionGeneratorArguments<string>(ResilienceContext.Get(), ResilienceContext.Get(), 0, _ => "primary".AsOutcomeAsync()))!;
        var res = await action();

        res.Result.Should().Be("ok");
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task GenerateAction_NonGeneric_Ok(bool nullAction)
    {
        var handler = new HedgingHandler<object>(
            PredicateInvoker<HandleHedgingArguments>.Create<object>(args => PredicateResult.True, false)!,
            args =>
            {
                if (nullAction)
                {
                    return null;
                }

                return () => ((object)"ok").AsOutcomeAsync();
            },
            false);

        var action = handler.GenerateAction(new HedgingActionGeneratorArguments<string>(ResilienceContext.Get(), ResilienceContext.Get(), 0, _ => "primary".AsOutcomeAsync()))!;
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
            PredicateInvoker<HandleHedgingArguments>.Create<object>(args => PredicateResult.True, false)!,
            args => () => args.Callback(args.ActionContext),
            false);

        var action = handler.GenerateAction(new HedgingActionGeneratorArguments<string>(ResilienceContext.Get(), ResilienceContext.Get(), 0, _ => "callback".AsOutcomeAsync()))!;
        var res = await action();
        res.Result.Should().Be("callback");
    }
}
