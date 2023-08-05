using Polly.Hedging;

namespace Polly.Core.Tests.Hedging;

public class HedgingPredicateArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        this.Invoking(_ => default(HedgingPredicateArguments)).Should().NotThrow();
    }
}
