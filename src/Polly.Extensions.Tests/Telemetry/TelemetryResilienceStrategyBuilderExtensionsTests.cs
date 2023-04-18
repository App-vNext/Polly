using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.Extensions.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class TelemetryResilienceStrategyBuilderExtensionsTests
{
    private readonly ResilienceStrategyBuilder _builder = new();

    [Fact]
    public void EnableTelemetry_EnsureDiagnosticSourceUpdated()
    {
        _builder.EnableTelemetry(NullLoggerFactory.Instance);
        _builder.Properties.GetValue(new ResiliencePropertyKey<DiagnosticSource?>("DiagnosticSource"), null).Should().BeOfType<ResilienceTelemetryDiagnosticSource>();
        _builder.AddStrategy(new TestResilienceStrategy()).Build().Should().NotBeOfType<TestResilienceStrategy>();
    }

    [Fact]
    public void EnableTelemetry_EnsureLogging()
    {
        using var factory = TestUtilities.CreateLoggerFactory(out var fakeLogger);
        _builder.EnableTelemetry(factory);
        _builder.AddStrategy(new TestResilienceStrategy()).Build().Execute(_ => { });

        fakeLogger.GetRecords().Should().NotBeEmpty();
        fakeLogger.GetRecords().Should().HaveCount(2);

    }

    [Fact]
    public void EnableTelemetry_InvalidOptions_Throws()
    {
        _builder
            .Invoking(b => b.EnableTelemetry(new TelemetryResilienceStrategyOptions
            {
                LoggerFactory = null!,
            })).Should()
            .Throw<ValidationException>()
            .WithMessage("""
            The resilience telemetry options are invalid.

            Validation Errors:
            The LoggerFactory field is required.
            """);
    }
}
