using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class HandleHedgingArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        this.Invoking(_ => new HandleHedgingArguments()).Should().NotThrow();
    }
}
