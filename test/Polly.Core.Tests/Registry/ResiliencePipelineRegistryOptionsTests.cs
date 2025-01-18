using Polly.Registry;

namespace Polly.Core.Tests.Registry;
public class ResiliencePipelineRegistryOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        ResiliencePipelineRegistryOptions<object> options = new();

        options.InstanceNameFormatter.ShouldBeNull();

        options.BuilderNameFormatter.ShouldNotBeNull();
        options.BuilderNameFormatter(null!).ShouldBe("");
        options.BuilderNameFormatter("ABC").ShouldBe("ABC");
    }
}
