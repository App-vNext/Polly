using NSubstitute;
using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public class ResilienceStrategyTelemetryTests
{
    private readonly DiagnosticSource _diagnosticSource = Substitute.For<DiagnosticSource>();

    public ResilienceStrategyTelemetryTests() => _sut = new(new ResilienceTelemetrySource(
        "builder",
        "instance",
        "strategy-name"), _diagnosticSource);

    private readonly ResilienceStrategyTelemetry _sut;

    [Fact]
    public void Report_NoOutcome_OK()
    {
        _diagnosticSource.IsEnabled("dummy-event").Returns(true);
        _diagnosticSource
            .When(o => o.Write("dummy-event", Arg.Is<object>(obj => obj is TelemetryEventArguments)))
            .Do((p) =>
            {
                p[1].Should().BeOfType<TelemetryEventArguments>();
                var args = (TelemetryEventArguments)p[1];

                args.Event.EventName.Should().Be("dummy-event");
                args.Event.Severity.Should().Be(ResilienceEventSeverity.Warning);
                args.Outcome.Should().BeNull();
                args.Source.StrategyName.Should().Be("strategy-name");
                args.Arguments.Should().BeOfType<TestArguments>();
                args.Outcome.Should().BeNull();
                args.Context.Should().NotBeNull();
            });

        _sut.Report(new(ResilienceEventSeverity.Warning, "dummy-event"), ResilienceContextPool.Shared.Get(), new TestArguments());

        _diagnosticSource.Received().Write("dummy-event", Arg.Is<object>(obj => obj is TelemetryEventArguments));
    }

    [Fact]
    public void Report_NoOutcomeWhenNotSubscribed_None()
    {
        _diagnosticSource.IsEnabled("dummy-event").Returns(false);

        _sut.Report(new(ResilienceEventSeverity.Warning, "dummy-event"), ResilienceContextPool.Shared.Get(), new TestArguments());

        _diagnosticSource.DidNotReceiveWithAnyArgs().Write(default!, default);
    }

    [Fact]
    public void ResilienceStrategyTelemetry_NoDiagnosticSource_Ok()
    {
        var source = new ResilienceTelemetrySource("builder", "instance", "strategy-name");
        var sut = new ResilienceStrategyTelemetry(source, null);
        var context = ResilienceContextPool.Shared.Get();

        sut.Invoking(s => s.Report(new(ResilienceEventSeverity.Warning, "dummy"), context, new TestArguments())).Should().NotThrow();
        sut.Invoking(s => s.Report(new(ResilienceEventSeverity.Warning, "dummy"), new OutcomeArguments<int, TestArguments>(context, Outcome.FromResult(1), new TestArguments()))).Should().NotThrow();
    }

    [Fact]
    public void Report_Outcome_OK()
    {
        _diagnosticSource.IsEnabled("dummy-event").Returns(true);
        _diagnosticSource
            .When(o => o.Write("dummy-event", Arg.Is<object>(obj => obj is TelemetryEventArguments)))
            .Do((obj) =>
            {
                obj[1].Should().BeOfType<TelemetryEventArguments>();
                var args = (TelemetryEventArguments)obj[1];

                args.Event.EventName.Should().Be("dummy-event");
                args.Event.Severity.Should().Be(ResilienceEventSeverity.Warning);
                args.Source.StrategyName.Should().Be("strategy-name");
                args.Arguments.Should().BeOfType<TestArguments>();
                args.Outcome.Should().NotBeNull();
                args.Outcome!.Value.Result.Should().Be(99);
                args.Context.Should().NotBeNull();
            });

        var context = ResilienceContextPool.Shared.Get();
        _sut.Report(new(ResilienceEventSeverity.Warning, "dummy-event"), new OutcomeArguments<int, TestArguments>(context, Outcome.FromResult(99), new TestArguments()));

        _diagnosticSource.Received().Write("dummy-event", Arg.Is<object>(obj => obj is TelemetryEventArguments));
    }

    [Fact]
    public void Report_SeverityNone_Skipped()
    {
        _diagnosticSource.IsEnabled("dummy-event").Returns(true);

        var context = ResilienceContextPool.Shared.Get();
        _sut.Report(new(ResilienceEventSeverity.None, "dummy-event"), new OutcomeArguments<int, TestArguments>(context, Outcome.FromResult(99), new TestArguments()));
        _sut.Report(new(ResilienceEventSeverity.None, "dummy-event"), ResilienceContextPool.Shared.Get(), new TestArguments());

        _diagnosticSource.DidNotReceiveWithAnyArgs().Write(default!, default);
    }

    [Fact]
    public void Report_OutcomeWhenNotSubscribed_None()
    {
        _diagnosticSource.IsEnabled("dummy-event").Returns(false);
        var context = ResilienceContextPool.Shared.Get();
        _sut.Report(new(ResilienceEventSeverity.Warning, "dummy-event"), new OutcomeArguments<int, TestArguments>(context, Outcome.FromResult(10), new TestArguments()));

        _diagnosticSource.DidNotReceiveWithAnyArgs().Write(default!, default);
    }
}
