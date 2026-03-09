using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

public class TelemetryResiliencePipelineBuilderExtensionsTests
{
    private readonly ResiliencePipelineBuilder _builder = new();

    [Fact]
    public void ConfigureTelemetry_EnsureDiagnosticSourceUpdated()
    {
        _builder.ConfigureTelemetry(NullLoggerFactory.Instance);
        _builder.TelemetryListener.ShouldBeOfType<TelemetryListenerImpl>();
        _builder.AddStrategy(new TestResilienceStrategy()).Build().ShouldNotBeOfType<TestResilienceStrategy>();
    }

    [Fact]
    public void ConfigureTelemetry_EnsureLogging()
    {
        using var factory = TestUtilities.CreateLoggerFactory(out var fakeLogger);

        _builder.ConfigureTelemetry(factory);
        _builder.AddStrategy(new TestResilienceStrategy()).Build().Execute(_ => { }, TestCancellation.Token);

        fakeLogger.GetRecords().ShouldNotBeEmpty();
        fakeLogger.GetRecords().Count().ShouldBe(2);
    }

    [Fact]
    public void ConfigureTelemetry_EnsureTracing()
    {
        var options = new TelemetryOptions();

        _builder.ConfigureTelemetry(options);
        var pipeline = _builder.AddStrategy(new TestResilienceStrategy()).Build();

        using var listener = new ActivityListener
        {
            Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ShouldListenTo = (source) => source.Name == "Polly",
        };

        ActivitySource.AddActivityListener(listener);

        Should.NotThrow(() => pipeline.Execute(
            _ =>
            {
                var activity = Activity.Current;

                activity.ShouldNotBeNull();
                activity.Source.ShouldBe(options.ActivitySource);
            },
            TestCancellation.Token));
    }

    [Fact]
    public void ConfigureTelemetry_EnsureTracing_CustomActivitySource()
    {
        var sourceName = Guid.NewGuid().ToString();
        var options = new TelemetryOptions
        {
            ActivitySource = new ActivitySource(sourceName),
        };

        _builder.ConfigureTelemetry(options);
        var pipeline = _builder.AddStrategy(new TestResilienceStrategy()).Build();

        using var listener = new ActivityListener
        {
            Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ShouldListenTo = (source) => source.Name == sourceName,
        };

        ActivitySource.AddActivityListener(listener);

        Should.NotThrow(() => pipeline.Execute(
            _ =>
            {
                var activity = Activity.Current;

                activity.ShouldNotBeNull();
                activity.Source.ShouldBe(options.ActivitySource);
            },
            TestCancellation.Token));
    }

    [Fact]
    public void ConfigureTelemetry_NullArguments_Throws()
    {
        ResiliencePipelineBuilder builder = null!;

        using var loggerFactory = TestUtilities.CreateLoggerFactory(out var fakeLogger);
        var options = new TelemetryOptions();

        Assert.Throws<ArgumentNullException>("builder", () => builder.ConfigureTelemetry(loggerFactory));
        Assert.Throws<ArgumentNullException>("builder", () => builder.ConfigureTelemetry(options));

        ILoggerFactory nulLoggerFactory = null!;
        TelemetryOptions nulOptions = null!;

        Assert.Throws<ArgumentNullException>("loggerFactory", () => _builder.ConfigureTelemetry(nulLoggerFactory));
        Assert.Throws<ArgumentNullException>("options", () => _builder.ConfigureTelemetry(nulOptions));
    }

    [Fact]
    public void ConfigureTelemetry_InvalidOptions_Throws()
    {
        var exception = Should.Throw<ValidationException>(() =>
            _builder.ConfigureTelemetry(new TelemetryOptions
            {
                LoggerFactory = null!,
            }));
        exception.Message.Trim().ShouldBe("""
            The 'TelemetryOptions' are invalid.
            Validation Errors:
            The LoggerFactory field is required.
            """,
            StringCompareShould.IgnoreLineEndings);
    }
}
