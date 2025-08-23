using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public class TelemetryEventArgumentsTests
{
    private readonly ResilienceTelemetrySource _source = new("builder", "instance", "strategy");

    [Fact]
    public void Ctor_Ok()
    {
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);
        var args = new TelemetryEventArguments<string, string>(_source, new ResilienceEvent(ResilienceEventSeverity.Warning, "ev"), context, "arg", Outcome.FromResult("dummy"));

        args.Outcome!.Value.Result.ShouldBe("dummy");
        args.Context.ShouldBe(context);
        args.Event.EventName.ShouldBe("ev");
        args.Event.Severity.ShouldBe(ResilienceEventSeverity.Warning);
        args.Source.ShouldBe(_source);
        args.Arguments.ShouldBeEquivalentTo("arg");
        args.Context.ShouldBe(context);
    }
}
