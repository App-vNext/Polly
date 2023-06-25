using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.Extensions.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class TelemetryResilienceStrategyBuilderExtensionsTests
{
    private readonly ResilienceStrategyBuilder _builder = new();
    private readonly ResilienceStrategyBuilder<string> _genericBuilder = new();

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void ConfigureTelemetry_EnsureDiagnosticSourceUpdated(bool generic)
    {
        if (generic)
        {
            _genericBuilder.ConfigureTelemetry(NullLoggerFactory.Instance);
            _genericBuilder.DiagnosticSource.Should().BeOfType<ResilienceTelemetryDiagnosticSource>();
        }
        else
        {
            _builder.ConfigureTelemetry(NullLoggerFactory.Instance);
            _builder.DiagnosticSource.Should().BeOfType<ResilienceTelemetryDiagnosticSource>();
            _builder.AddStrategy(new TestResilienceStrategy()).Build().Should().NotBeOfType<TestResilienceStrategy>();
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void ConfigureTelemetry_EnsureLogging(bool generic)
    {
        using var factory = TestUtilities.CreateLoggerFactory(out var fakeLogger);

        if (generic)
        {
            _genericBuilder.ConfigureTelemetry(factory);
            _genericBuilder.AddStrategy(new TestResilienceStrategy()).Build().Execute(_ => string.Empty);
        }
        else
        {
            _builder.ConfigureTelemetry(factory);
            _builder.AddStrategy(new TestResilienceStrategy()).Build().Execute(_ => { });
        }

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
            The resilience telemetry options are invalid.

            Validation Errors:
            The LoggerFactory field is required.
            """);

        _genericBuilder
            .Invoking(b => b.ConfigureTelemetry(new TelemetryOptions
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
