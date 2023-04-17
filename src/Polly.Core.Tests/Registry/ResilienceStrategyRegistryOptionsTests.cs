using Polly.Registry;

namespace Polly.Core.Tests.Registry;
public class ResilienceStrategyRegistryOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        ResilienceStrategyRegistryOptions<object> options = new();

        options.KeyFormatter.Should().NotBeNull();
        options.KeyFormatter(null!).Should().Be("");
        options.KeyFormatter("ABC").Should().Be("ABC");
    }
}
