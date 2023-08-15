using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class PipelineExecutedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new PipelineExecutedArguments(TimeSpan.MaxValue);
        args.Duration.Should().Be(TimeSpan.MaxValue);
    }
}
