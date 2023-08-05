using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class ExecutionAttemptArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new ExecutionAttemptArguments(99, TimeSpan.MaxValue, true);
        Assert.NotNull(args);
        args.AttemptNumber.Should().Be(99);
        args.ExecutionTime.Should().Be(TimeSpan.MaxValue);
        args.Handled.Should().BeTrue();
    }

    [Fact]
    public void Get_Ok()
    {
        var args = ExecutionAttemptArguments.Get(99, TimeSpan.MaxValue, true);
        Assert.NotNull(args);
        args.AttemptNumber.Should().Be(99);
        args.ExecutionTime.Should().Be(TimeSpan.MaxValue);
        args.Handled.Should().BeTrue();
    }

    [Fact]
    public void Return_EnsurePropertiesCleared()
    {
        var args = ExecutionAttemptArguments.Get(99, TimeSpan.MaxValue, true);

        ExecutionAttemptArguments.Return(args);

        args.AttemptNumber.Should().Be(0);
        args.ExecutionTime.Should().Be(TimeSpan.Zero);
        args.Handled.Should().BeFalse();
    }
}
