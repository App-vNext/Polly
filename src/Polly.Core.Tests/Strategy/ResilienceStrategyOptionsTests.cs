using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class ResilienceStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new ResilienceStrategyOptions();

        options.StrategyType.Should().Be("");
        options.StrategyName.Should().Be("");
    }
}
