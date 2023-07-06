using Polly.Registry;

namespace Polly.Core.Tests.Registry;
public class ResilienceStrategyRegistryOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        ResilienceStrategyRegistryOptions<object> options = new();

        options.InstanceNameFormatter.Should().BeNull();

        options.BuilderNameFormatter.Should().NotBeNull();
        options.BuilderNameFormatter(null!).Should().Be("");
        options.BuilderNameFormatter("ABC").Should().Be("ABC");
    }
}
