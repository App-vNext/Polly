using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class TelemetryEventArgumentsTests
{
    private readonly ResilienceTelemetrySource _source = new("builder", "instance", "strategy");

    [Fact]
    public void Get_Ok()
    {
        var context = ResilienceContextPool.Shared.Get();
        var args = TelemetryEventArguments.Get(_source, new ResilienceEvent(ResilienceEventSeverity.Warning, "ev"), context, Outcome.FromResult<object>("dummy"), "arg");

        args.Outcome!.Value.Result.Should().Be("dummy");
        args.Context.Should().Be(context);
        args.Event.EventName.Should().Be("ev");
        args.Event.Severity.Should().Be(ResilienceEventSeverity.Warning);
        args.Source.Should().Be(_source);
        args.Arguments.Should().BeEquivalentTo("arg");
        args.Context.Should().Be(context);
    }

    [Fact]
    public void Return_EnsurePropertiesCleared()
    {
        var context = ResilienceContextPool.Shared.Get();
        var args = TelemetryEventArguments.Get(_source, new ResilienceEvent(ResilienceEventSeverity.Warning, "ev"), context, Outcome.FromResult<object>("dummy"), "arg");

        TelemetryEventArguments.Return(args);

        TestUtilities.AssertWithTimeoutAsync(() =>
        {
            args.Outcome.Should().BeNull();
            args.Context.Should().BeNull();
            args.Event.EventName.Should().BeNullOrEmpty();
            args.Source.Should().BeNull();
            args.Arguments.Should().BeNull();
            args.Context.Should().BeNull();
        });
    }
}
