using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class OnHedgingArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnHedgingArguments<int>(ResilienceContextPool.Shared.Get(), ResilienceContextPool.Shared.Get(), 1);

        args.PrimaryContext.Should().NotBeNull();
        args.ActionContext.Should().NotBeNull();
        args.AttemptNumber.Should().Be(1);
    }
}
