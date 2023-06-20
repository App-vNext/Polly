using Polly.Fallback;
namespace Polly.Core.Tests.Fallback;

public class FallbackHandlerTests
{
    [Fact]
    public async Task GenerateAction_Generic_Ok()
    {
        var handler = FallbackHelper.CreateHandler(_ => true, () => "secondary".AsOutcome(), true);
        var context = ResilienceContext.Get();
        var outcome = await handler.GetFallbackOutcomeAsync(new OutcomeArguments<string, FallbackPredicateArguments>(context, new Outcome<string>("primary"), new FallbackPredicateArguments()))!;

        outcome.Result.Should().Be("secondary");
    }

    [Fact]
    public async Task GenerateAction_NonGeneric_Ok()
    {
        var handler = FallbackHelper.CreateHandler<object>(_ => true, () => ((object)"secondary").AsOutcome(), false);
        var context = ResilienceContext.Get();
        var outcome = await handler.GetFallbackOutcomeAsync(new OutcomeArguments<string, FallbackPredicateArguments>(context, new Outcome<string>("primary"), new FallbackPredicateArguments()))!;

        outcome.Result.Should().Be("secondary");
    }
}
