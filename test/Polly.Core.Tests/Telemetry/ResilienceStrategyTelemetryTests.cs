using Moq;
using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public class ResilienceStrategyTelemetryTests
{
    private readonly Mock<DiagnosticSource> _diagnosticSource = new(MockBehavior.Strict);

    public ResilienceStrategyTelemetryTests() => _sut = new(new ResilienceTelemetrySource(
        "builder",
        "instance",
        new ResilienceProperties(),
        "strategy-name"), _diagnosticSource.Object);

    private readonly ResilienceStrategyTelemetry _sut;

    [Fact]
    public void Report_NoOutcome_OK()
    {
        _diagnosticSource.Setup(o => o.IsEnabled("dummy-event")).Returns(true);
        _diagnosticSource
            .Setup(o => o.Write("dummy-event", It.Is<object>(obj => obj is TelemetryEventArguments)))
            .Callback<string, object>((_, obj) =>
            {
                obj.Should().BeOfType<TelemetryEventArguments>();
                var args = (TelemetryEventArguments)obj;

                args.Event.EventName.Should().Be("dummy-event");
                args.Event.Severity.Should().Be(ResilienceEventSeverity.Warning);
                args.Outcome.Should().BeNull();
                args.Source.StrategyName.Should().Be("strategy-name");
                args.Source.BuilderProperties.Should().NotBeNull();
                args.Arguments.Should().BeOfType<TestArguments>();
                args.Outcome.Should().BeNull();
                args.Context.Should().NotBeNull();
            });

        _sut.Report(new(ResilienceEventSeverity.Warning, "dummy-event"), ResilienceContextPool.Shared.Get(), new TestArguments());

        _diagnosticSource.VerifyAll();
    }

    [Fact]
    public void Report_NoOutcomeWhenNotSubscribed_None()
    {
        _diagnosticSource.Setup(o => o.IsEnabled("dummy-event")).Returns(false);

        _sut.Report(new(ResilienceEventSeverity.Warning, "dummy-event"), ResilienceContextPool.Shared.Get(), new TestArguments());

        _diagnosticSource.VerifyAll();
        _diagnosticSource.VerifyNoOtherCalls();
    }

    [Fact]
    public void ResilienceStrategyTelemetry_NoDiagnosticSource_Ok()
    {
        var source = new ResilienceTelemetrySource("builder", "instance", new ResilienceProperties(), "strategy-name");
        var sut = new ResilienceStrategyTelemetry(source, null);
        var context = ResilienceContextPool.Shared.Get();

        sut.Invoking(s => s.Report(new(ResilienceEventSeverity.Warning, "dummy"), context, new TestArguments())).Should().NotThrow();
        sut.Invoking(s => s.Report(new(ResilienceEventSeverity.Warning, "dummy"), new OutcomeArguments<int, TestArguments>(context, Outcome.FromResult(1), new TestArguments()))).Should().NotThrow();
    }

    [Fact]
    public void Report_Outcome_OK()
    {
        _diagnosticSource.Setup(o => o.IsEnabled("dummy-event")).Returns(true);
        _diagnosticSource
            .Setup(o => o.Write("dummy-event", It.Is<object>(obj => obj is TelemetryEventArguments)))
            .Callback<string, object>((_, obj) =>
            {
                obj.Should().BeOfType<TelemetryEventArguments>();
                var args = (TelemetryEventArguments)obj;

                args.Event.EventName.Should().Be("dummy-event");
                args.Event.Severity.Should().Be(ResilienceEventSeverity.Warning);
                args.Source.StrategyName.Should().Be("strategy-name");
                args.Source.BuilderProperties.Should().NotBeNull();
                args.Arguments.Should().BeOfType<TestArguments>();
                args.Outcome.Should().NotBeNull();
                args.Outcome!.Value.Result.Should().Be(99);
                args.Context.Should().NotBeNull();
            });

        var context = ResilienceContextPool.Shared.Get();
        _sut.Report(new(ResilienceEventSeverity.Warning, "dummy-event"), new OutcomeArguments<int, TestArguments>(context, Outcome.FromResult(99), new TestArguments()));

        _diagnosticSource.VerifyAll();
    }

    [Fact]
    public void Report_SeverityNone_Skipped()
    {
        _diagnosticSource.Setup(o => o.IsEnabled("dummy-event")).Returns(true);

        var context = ResilienceContextPool.Shared.Get();
        _sut.Report(new(ResilienceEventSeverity.None, "dummy-event"), new OutcomeArguments<int, TestArguments>(context, Outcome.FromResult(99), new TestArguments()));
        _sut.Report(new(ResilienceEventSeverity.None, "dummy-event"), ResilienceContextPool.Shared.Get(), new TestArguments());

        _diagnosticSource.VerifyAll();
        _diagnosticSource.VerifyNoOtherCalls();
    }

    [Fact]
    public void Report_OutcomeWhenNotSubscribed_None()
    {
        _diagnosticSource.Setup(o => o.IsEnabled("dummy-event")).Returns(false);
        var context = ResilienceContextPool.Shared.Get();
        _sut.Report(new(ResilienceEventSeverity.Warning, "dummy-event"), new OutcomeArguments<int, TestArguments>(context, Outcome.FromResult(10), new TestArguments()));

        _diagnosticSource.VerifyAll();
        _diagnosticSource.VerifyNoOtherCalls();
    }
}
