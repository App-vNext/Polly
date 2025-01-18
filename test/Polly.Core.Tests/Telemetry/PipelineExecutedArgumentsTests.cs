using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public class PipelineExecutedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new PipelineExecutedArguments(TimeSpan.MaxValue);
        args.Duration.ShouldBe(TimeSpan.MaxValue);
    }
}
