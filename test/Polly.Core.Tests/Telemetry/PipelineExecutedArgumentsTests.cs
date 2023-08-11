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

    [Fact]
    public void Get_Ok()
    {
        var args = PipelineExecutedArguments.Get(TimeSpan.MaxValue);
        Assert.NotNull(args);
        args.Duration.Should().Be(TimeSpan.MaxValue);
    }

    [Fact]
    public void Return_EnsurePropertiesCleared()
    {
        var args = PipelineExecutedArguments.Get(TimeSpan.MaxValue);

        PipelineExecutedArguments.Return(args);

        args.Duration.Should().Be(TimeSpan.Zero);
    }
}
