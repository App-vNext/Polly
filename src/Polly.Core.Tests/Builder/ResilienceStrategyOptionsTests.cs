using Polly.Builder;

namespace Polly.Core.Tests.Builder;

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
