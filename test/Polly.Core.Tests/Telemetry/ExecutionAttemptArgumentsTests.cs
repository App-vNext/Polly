using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public class ExecutionAttemptArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new ExecutionAttemptArguments(99, TimeSpan.MaxValue, true);
        args.AttemptNumber.ShouldBe(99);
        args.Duration.ShouldBe(TimeSpan.MaxValue);
        args.Handled.ShouldBeTrue();
    }
}
