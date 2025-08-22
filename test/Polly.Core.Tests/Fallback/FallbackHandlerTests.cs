using Polly.Fallback;
namespace Polly.Core.Tests.Fallback;

public class FallbackHandlerTests
{
    [Fact]
    public async Task GenerateAction_Generic_Ok()
    {
        var handler = FallbackHelper.CreateHandler(_ => true, () => Outcome.FromResult("secondary"));
        var context = ResilienceContextPool.Shared.Get();
        var outcome = await handler.ActionGenerator(new FallbackActionArguments<string>(context, Outcome.FromResult("primary")))!;

        outcome.Result.ShouldBe("secondary");
    }
}
