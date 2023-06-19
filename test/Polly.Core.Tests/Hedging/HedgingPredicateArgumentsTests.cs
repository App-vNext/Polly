using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class HedgingPredicateArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        this.Invoking(_ => new HedgingPredicateArguments()).Should().NotThrow();
    }
}
