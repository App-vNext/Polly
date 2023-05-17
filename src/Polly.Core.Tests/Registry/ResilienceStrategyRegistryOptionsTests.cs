using Polly.Registry;

namespace Polly.Core.Tests.Registry;
public class ResilienceStrategyRegistryOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        ResilienceStrategyRegistryOptions<object> options = new();

        options.StrategyKeyFormatter.Should().NotBeNull();
        options.StrategyKeyFormatter(null!).Should().Be("");
        options.StrategyKeyFormatter("ABC").Should().Be("ABC");

        options.BuilderNameFormatter.Should().NotBeNull();
        options.BuilderNameFormatter(null!).Should().Be("");
        options.BuilderNameFormatter("ABC").Should().Be("ABC");
    }
}
