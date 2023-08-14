using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class TelemetryEventArgumentsTests
{
    private readonly ResilienceTelemetrySource _source = new("builder", "instance", "strategy");

    [Fact]
    public void Ctor_Ok()
    {
        var context = ResilienceContextPool.Shared.Get();
        var args = new TelemetryEventArguments<string, string>(_source, new ResilienceEvent(ResilienceEventSeverity.Warning, "ev"), context, "arg", Outcome.FromResult<string>("dummy"));

        args.Outcome!.Value.Result.Should().Be("dummy");
        args.Context.Should().Be(context);
        args.Event.EventName.Should().Be("ev");
        args.Event.Severity.Should().Be(ResilienceEventSeverity.Warning);
        args.Source.Should().Be(_source);
        args.Arguments.Should().BeEquivalentTo("arg");
        args.Context.Should().Be(context);
    }
}
