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
        _sut.Enabled.Should().BeTrue();
        new ResilienceStrategyTelemetry(_source, null).Enabled.Should().BeFalse();
    }

    [Fact]
    public void Report_NoOutcome_OK()
    {
        _sut.Report(new(ResilienceEventSeverity.Warning, "dummy-event"), ResilienceContextPool.Shared.Get(), new TestArguments());

        _args.Should().HaveCount(1);
        var args = _args.Single();
        args.Event.EventName.Should().Be("dummy-event");
        args.Event.Severity.Should().Be(ResilienceEventSeverity.Warning);
        args.Outcome.Should().BeNull();
        args.Source.StrategyName.Should().Be("strategy_name");
        args.Arguments.Should().BeOfType<TestArguments>();
        args.Outcome.Should().BeNull();
        args.Context.Should().NotBeNull();
    }

    [Fact]
    public void ResiliencePipelineTelemetry_NoDiagnosticSource_Ok()
    {
        var source = new ResilienceTelemetrySource("builder", "instance", "strategy_name");
        var sut = new ResilienceStrategyTelemetry(source, null);
        var context = ResilienceContextPool.Shared.Get();

        sut.Invoking(s => s.Report(new(ResilienceEventSeverity.Warning, "dummy"), context, new TestArguments())).Should().NotThrow();
        sut.Invoking(s => s.Report(new(ResilienceEventSeverity.Warning, "dummy"), context, Outcome.FromResult(1), new TestArguments())).Should().NotThrow();
    }

    [Fact]
    public void Report_Outcome_OK()
    {
        var context = ResilienceContextPool.Shared.Get();
        _sut.Report(new(ResilienceEventSeverity.Warning, "dummy-event"), context, Outcome.FromResult(99), new TestArguments());

        _args.Should().HaveCount(1);
        var args = _args.Single();
        args.Event.EventName.Should().Be("dummy-event");
        args.Event.Severity.Should().Be(ResilienceEventSeverity.Warning);
        args.Source.StrategyName.Should().Be("strategy_name");
        args.Arguments.Should().BeOfType<TestArguments>();
        args.Outcome.Should().NotBeNull();
        args.Outcome!.Value.Result.Should().Be(99);
        args.Context.Should().NotBeNull();
    }

    [Fact]
    public void Report_SeverityNone_Skipped()
    {
        var context = ResilienceContextPool.Shared.Get();
        _sut.Report(new(ResilienceEventSeverity.None, "dummy-event"), context, Outcome.FromResult(99), new TestArguments());
        _sut.Report(new(ResilienceEventSeverity.None, "dummy-event"), ResilienceContextPool.Shared.Get(), new TestArguments());

        _args.Should().BeEmpty();
    }

    [Fact]
    public void Report_NoListener_ShouldNotThrow()
    {
        var sut = new ResilienceStrategyTelemetry(_source, null);

        var context = ResilienceContextPool.Shared.Get();

        sut.Invoking(s => s.Report(new(ResilienceEventSeverity.None, "dummy-event"), context, Outcome.FromResult(99), new TestArguments()))
           .Should()
           .NotThrow();

        sut.Invoking(s => s.Report(new(ResilienceEventSeverity.None, "dummy-event"), ResilienceContextPool.Shared.Get(), new TestArguments()))
           .Should()
           .NotThrow();
    }

    [Fact]
    public void SetTelemetrySource_Ok()
    {
        var sut = new ResilienceStrategyTelemetry(_source, null);
        var exception = new TimeoutRejectedException();

        sut.SetTelemetrySource(exception);

        exception.TelemetrySource.Should().Be(_source);
    }

    [Fact]
    public void SetTelemetrySource_ShouldThrow()
    {
        ExecutionRejectedException? exception = null;

        _sut.Invoking(s => s.SetTelemetrySource(exception!))
            .Should()
            .Throw<ArgumentNullException>();
    }
}
