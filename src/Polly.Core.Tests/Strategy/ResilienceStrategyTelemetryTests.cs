using Moq;
using Polly.Strategy;
using Polly.Telemetry;

namespace Polly.Core.Tests.Strategy;

public class ResilienceStrategyTelemetryTests
{
    private readonly Mock<DiagnosticSource> _diagnosticSource = new(MockBehavior.Strict);

    public ResilienceStrategyTelemetryTests() => _sut = new(new ResilienceTelemetrySource("builder", new ResilienceProperties(), "strategy-name", "strategy-type"), _diagnosticSource.Object);

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

                args.EventName.Should().Be("dummy-event");
                args.Outcome.Should().BeNull();
                args.Source.StrategyName.Should().Be("strategy-name");
                args.Source.StrategyType.Should().Be("strategy-type");
                args.Source.BuilderProperties.Should().NotBeNull();
                args.Arguments.Should().BeOfType<TestArguments>();
                args.EventName.Should().Be("dummy-event");
                args.Outcome.Should().BeNull();
                args.Context.Should().NotBeNull();
            });

        _sut.Report("dummy-event", new TestArguments());

        _diagnosticSource.VerifyAll();
    }

    [Fact]
    public void Report_NoOutcomeWhenNotSubscribed_None()
    {
        _diagnosticSource.Setup(o => o.IsEnabled("dummy-event")).Returns(false);

        _sut.Report("dummy-event", new TestArguments());

        _diagnosticSource.VerifyAll();
        _diagnosticSource.VerifyNoOtherCalls();
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

                args.EventName.Should().Be("dummy-event");
                args.Source.StrategyName.Should().Be("strategy-name");
                args.Source.StrategyType.Should().Be("strategy-type");
                args.Source.BuilderProperties.Should().NotBeNull();
                args.Arguments.Should().BeOfType<TestArguments>();
                args.EventName.Should().Be("dummy-event");
                args.Outcome.Should().NotBeNull();
                args.Outcome!.Value.Result.Should().Be(99);
                args.Context.Should().NotBeNull();
            });

        _sut.Report("dummy-event", new Outcome<int>(99), new TestArguments());

        _diagnosticSource.VerifyAll();
    }

    [Fact]
    public void Report_OutcomeWhenNotSubscribed_None()
    {
        _diagnosticSource.Setup(o => o.IsEnabled("dummy-event")).Returns(false);

        _sut.Report("dummy-event", new Outcome<int>(10), new TestArguments());

        _diagnosticSource.VerifyAll();
        _diagnosticSource.VerifyNoOtherCalls();
    }
}
