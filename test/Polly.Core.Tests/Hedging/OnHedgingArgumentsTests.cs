using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class OnHedgingArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnHedgingArguments<int>(ResilienceContextPool.Shared.Get(), Outcome.FromResult(1), 1, true, TimeSpan.FromSeconds(1));

        args.Context.Should().NotBeNull();
        args.Outcome.Result.Should().Be(1);
        args.AttemptNumber.Should().Be(1);
        args.HasOutcome.Should().BeTrue();
        args.Duration.Should().Be(TimeSpan.FromSeconds(1));
    }
}
