using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class TelemetryCompositeStrategyBuilderExtensionsTests
{
    private readonly CompositeStrategyBuilder _builder = new();

    [Fact]
    public void ConfigureTelemetry_EnsureDiagnosticSourceUpdated()
    {
        _builder.ConfigureTelemetry(NullLoggerFactory.Instance);
        _builder.DiagnosticSource.Should().BeOfType<ResilienceTelemetryDiagnosticSource>();
        _builder.AddStrategy(new TestResilienceStrategy()).Build().Should().NotBeOfType<TestResilienceStrategy>();
    }

    [Fact]
    public void ConfigureTelemetry_EnsureLogging()
    {
        using var factory = TestUtilities.CreateLoggerFactory(out var fakeLogger);

        _builder.ConfigureTelemetry(factory);
        _builder.AddStrategy(new TestResilienceStrategy()).Build().Execute(_ => { });

        fakeLogger.GetRecords().Should().NotBeEmpty();
        fakeLogger.GetRecords().Should().HaveCount(2);
    }

    [Fact]
    public void ConfigureTelemetry_InvalidOptions_Throws()
    {
        _builder
            .Invoking(b => b.ConfigureTelemetry(new TelemetryOptions
            {
                LoggerFactory = null!,
            })).Should()
            .Throw<ValidationException>()
            .WithMessage("""
            The 'TelemetryOptions' are invalid.

            Validation Errors:
            The LoggerFactory field is required.
            """);
    }
}
