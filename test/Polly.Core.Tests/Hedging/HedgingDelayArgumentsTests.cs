using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class HedgingDelayArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new HedgingDelayArguments(ResilienceContextPool.Shared.Get(), 5);

        args.Context.Should().NotBeNull();
        args.Attempt.Should().Be(5);
    }
}
