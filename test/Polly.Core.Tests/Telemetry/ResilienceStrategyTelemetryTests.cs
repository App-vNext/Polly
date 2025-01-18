using Polly.Telemetry;
using Polly.Timeout;

namespace Polly.Core.Tests.Telemetry;

public class ResilienceStrategyTelemetryTests
{
    private readonly List<TelemetryEventArguments<object, object>> _args = [];
    private readonly ResilienceTelemetrySource _source;
    private readonly ResilienceStrategyTelemetry _sut;

    public ResilienceStrategyTelemetryTests()
    {
        _source = new ResilienceTelemetrySource(
            "builder",
            "instance",
            "strategy_name");

        _sut = TestUtilities.CreateResilienceTelemetry(args => _args.Add(args));
    }

    [Fact]
    public void Enabled_Ok()
    {
        _sut.Enabled.ShouldBeTrue();
        new ResilienceStrategyTelemetry(_source, null).Enabled.ShouldBeFalse();
    }

    [Fact]
    public void Report_NoOutcome_OK()
    {
        var context = ResilienceContextPool.Shared.Get();

        _sut.Report(new(ResilienceEventSeverity.Warning, "dummy-event"), context, new TestArguments());

        _args.Count.ShouldBe(1);
        var args = _args.Single();
        args.Event.EventName.ShouldBe("dummy-event");
        args.Event.Severity.ShouldBe(ResilienceEventSeverity.Warning);
        args.Outcome.ShouldBeNull();
        args.Source.StrategyName.ShouldBe("strategy_name");
        args.Arguments.ShouldBeOfType<TestArguments>();
        args.Outcome.ShouldBeNull();
        args.Context.ShouldBe(context);
    }

    [Fact]
    public void ResiliencePipelineTelemetry_NoDiagnosticSource_Ok()
    {
        var source = new ResilienceTelemetrySource("builder", "instance", "strategy_name");
        var sut = new ResilienceStrategyTelemetry(source, null);
        var context = ResilienceContextPool.Shared.Get();

        Should.NotThrow(() => sut.Report(new(ResilienceEventSeverity.Warning, "dummy"), context, new TestArguments()));
        Should.NotThrow(() => sut.Report(new(ResilienceEventSeverity.Warning, "dummy"), context, Outcome.FromResult(1), new TestArguments()));
    }

    [Fact]
    public void Report_Outcome_OK()
    {
        var context = ResilienceContextPool.Shared.Get();
        _sut.Report(new(ResilienceEventSeverity.Warning, "dummy-event"), context, Outcome.FromResult(99), new TestArguments());

        _args.Count.ShouldBe(1);
        var args = _args.Single();
        args.Event.EventName.ShouldBe("dummy-event");
        args.Event.Severity.ShouldBe(ResilienceEventSeverity.Warning);
        args.Source.StrategyName.ShouldBe("strategy_name");
        args.Arguments.ShouldBeOfType<TestArguments>();
        args.Outcome.ShouldNotBeNull();
        args.Outcome!.Value.Result.ShouldBe(99);
        args.Context.ShouldNotBeNull();
    }

    [Fact]
    public void Report_SeverityNone_Skipped()
    {
        var context = ResilienceContextPool.Shared.Get();
        _sut.Report(new(ResilienceEventSeverity.None, "dummy-event"), context, Outcome.FromResult(99), new TestArguments());
        _sut.Report(new(ResilienceEventSeverity.None, "dummy-event"), context, new TestArguments());

        _args.ShouldBeEmpty();
    }

    [Fact]
    public void Report_NoListener_ShouldNotThrow()
    {
        var sut = new ResilienceStrategyTelemetry(_source, null);

        var context = ResilienceContextPool.Shared.Get();

        Should.NotThrow(() => sut.Report(new(ResilienceEventSeverity.None, "dummy-event"), context, Outcome.FromResult(99), new TestArguments()));

        Should.NotThrow(() => sut.Report(new(ResilienceEventSeverity.None, "dummy-event"), context, new TestArguments()));
    }

    [Fact]
    public void SetTelemetrySource_Ok()
    {
        var sut = new ResilienceStrategyTelemetry(_source, null);
        var exception = new TimeoutRejectedException();

        sut.SetTelemetrySource(exception);

        exception.TelemetrySource.ShouldBe(_source);
    }

    [Fact]
    public void SetTelemetrySource_ShouldThrow()
    {
        ExecutionRejectedException? exception = null;

        Should.Throw<ArgumentNullException>(() => _sut.SetTelemetrySource(exception!));
    }
}
