using System.Diagnostics;
using Moq;
using Polly.Telemetry;

namespace Polly.Core.Tests.Telemetry;

public class TelemetryUtilTests
{
    [Fact]
    public void Ctor_Ok()
    {
        TelemetryUtil.StrategyKey.Key.Should().Be("Polly.StrategyKey");
    }

    [Fact]
    public void CreateResilienceTelemetry_Ok()
    {
        var telemetry = TelemetryUtil.CreateTelemetry(null, "builder", new ResilienceProperties(), "strategy-name", "strategy-type");

        telemetry.TelemetrySource.BuilderName.Should().Be("builder");
        telemetry.TelemetrySource.StrategyName.Should().Be("strategy-name");
        telemetry.TelemetrySource.StrategyType.Should().Be("strategy-type");
        telemetry.DiagnosticSource.Should().BeNull();
    }

    [Fact]
    public void CreateResilienceTelemetry_DiagnosticSourceFromProperties_Ok()
    {
        var props = new ResilienceProperties();
        var source = Mock.Of<DiagnosticSource>();

        var telemetry = TelemetryUtil.CreateTelemetry(source, "builder", props, "strategy-name", "strategy-type");

        telemetry.DiagnosticSource.Should().Be(source);
    }
}
