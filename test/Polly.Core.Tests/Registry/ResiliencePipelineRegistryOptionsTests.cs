using Polly.Registry;

namespace Polly.Core.Tests.Registry;
public class ResiliencePipelineRegistryOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        ResiliencePipelineRegistryOptions<object> options = new();

        options.InstanceNameFormatter.Should().BeNull();

        options.BuilderNameFormatter.Should().NotBeNull();
        options.BuilderNameFormatter(null!).Should().Be("");
        options.BuilderNameFormatter("ABC").Should().Be("ABC");
    }
}
