using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class HedgingDelayGeneratorArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new HedgingDelayGeneratorArguments(ResilienceContextPool.Shared.Get(), 5);

        args.Context.Should().NotBeNull();
        args.AttemptNumber.Should().Be(5);
    }
}
