using System;
using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class TelemetryEventArgumentsTests
{
    private readonly ResilienceTelemetrySource _source = new("builder", new ResilienceProperties(), "strategy", "type");

    [Fact]
    public void Get_Ok()
    {
        var context = ResilienceContext.Get();
        var args = TelemetryEventArguments.Get(_source, "ev", context, Outcome.FromResult<object>("dummy"), "arg");

        args.Outcome!.Value.Result.Should().Be("dummy");
        args.Context.Should().Be(context);
        args.EventName.Should().Be("ev");
        args.Source.Should().Be(_source);
        args.Arguments.Should().BeEquivalentTo("arg");
        args.Context.Should().Be(context);
    }

    [Fact]
    public void Return_EnsurePropertiesCleared()
    {
        var context = ResilienceContext.Get();
        var args = TelemetryEventArguments.Get(_source, "ev", context, Outcome.FromResult<object>("dummy"), "arg");

        TelemetryEventArguments.Return(args);

        TestUtilities.AssertWithTimeoutAsync(() =>
        {
            args.Outcome.Should().BeNull();
            args.Context.Should().BeNull();
            args.EventName.Should().BeNull();
            args.Source.Should().BeNull();
            args.Arguments.Should().BeNull();
            args.Context.Should().BeNull();
        });
    }
}
