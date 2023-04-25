using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class HandleHedgingArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new HandleHedgingArguments(ResilienceContext.Get());

        args.Context.Should().NotBeNull();
    }
}
