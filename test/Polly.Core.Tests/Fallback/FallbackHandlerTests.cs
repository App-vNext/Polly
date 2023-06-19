using Polly.Fallback;
namespace Polly.Core.Tests.Fallback;

public class FallbackHandlerTests
{
    [Fact]
    public void HandlesFallback_Generic_Ok()
    {
        var handler = FallbackHelper.CreateHandler<string>(_ => true, () => "secondary".AsOutcome(), true);

        handler.HandlesFallback<string>().Should().Be(true);
        handler.HandlesFallback<double>().Should().Be(false);
        handler.HandlesFallback<object>().Should().Be(false);
    }

    [Fact]
    public void HandlesFallback_NonGeneric_Ok()
    {
        var handler = FallbackHelper.CreateHandler(_ => true, () => "secondary".AsOutcome(), false);

        handler.HandlesFallback<string>().Should().Be(true);
        handler.HandlesFallback<double>().Should().Be(true);
        handler.HandlesFallback<object>().Should().Be(true);
    }

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
