namespace Polly.Core.Tests;

public class NullResiliencePipelineTests
{
    [Fact]
    public void Instance_ShouldNotBeNull()
    {
        NullResiliencePipeline.Instance.Should().NotBeNull();
        NullResiliencePipeline<string>.Instance.Should().NotBeNull();

    }

    [Fact]
    public void Execute_Ok()
    {
        bool executed = false;
        NullResiliencePipeline.Instance.Execute(_ => executed = true);
        executed.Should().BeTrue();

        NullResiliencePipeline<string>.Instance.Execute(_ => "res").Should().Be("res");
    }
}
