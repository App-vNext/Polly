using System.Net.Http;
using Microsoft.Extensions.Logging;
using Polly.Extensions.Telemetry;
using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

#pragma warning disable S103 // Lines should not be too long

[Collection("NonParallelizableTests")]
public class ResilienceTelemetryDiagnosticSourceTests : IDisposable
{
    private readonly FakeLogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly List<MeteringEvent> _events = new();
    private readonly IDisposable _metering;

    public ResilienceTelemetryDiagnosticSourceTests()
    {
        _metering = TestUtilities.EnablePollyMetering(_events);
        _loggerFactory = TestUtilities.CreateLoggerFactory(out _logger);
    }

    public void Dispose()
    {
        _metering.Dispose();
        _loggerFactory.Dispose();
    }

    [Fact]
    public void Meter_Ok()
    {
        ResilienceTelemetryDiagnosticSource.Meter.Name.Should().Be("Polly");
        ResilienceTelemetryDiagnosticSource.Meter.Version.Should().Be("1.0");
        var source = new ResilienceTelemetryDiagnosticSource(new TelemetryOptions());

        source.Counter.Description.Should().Be("Tracks the number of resilience events that occurred in resilience strategies.");
        source.AttemptDuration.Description.Should().Be("Tracks the duration of execution attempts.");
        source.AttemptDuration.Unit.Should().Be("ms");
    }

    [Fact]
    public void IsEnabled_true()
    {
        var source = Create();

        source.IsEnabled("dummy").Should().BeTrue();
    }

    [Fact]
    public void Write_InvalidType_Nothing()
    {
        var source = Create();

        source.Write("dummy", new object());

        _logger.GetRecords().Should().BeEmpty();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void WriteEvent_LoggingWithOutcome_Ok(bool noOutcome)
    {
        var telemetry = Create();
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        ReportEvent(telemetry, noOutcome ? null : new Outcome<object>(response));

        var messages = _logger.GetRecords(new EventId(0, "ResilienceEvent")).ToList();
        messages.Should().HaveCount(1);

        if (noOutcome)
        {
            messages[0].Message.Should().Be("Resilience event occurred. EventName: 'my-event', Builder Name: 'my-builder', Strategy Name: 'my-strategy', Strategy Type: 'my-strategy-type', Strategy Key: 'my-strategy-key', Result: '(null)'");
        }
        else
        {
            messages[0].Message.Should().Be("Resilience event occurred. EventName: 'my-event', Builder Name: 'my-builder', Strategy Name: 'my-strategy', Strategy Type: 'my-strategy-type', Strategy Key: 'my-strategy-key', Result: '200'");
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void WriteEvent_LoggingWithException_Ok(bool noOutcome)
    {
        var telemetry = Create();
        ReportEvent(telemetry, noOutcome ? null : new Outcome<object>(new InvalidOperationException("Dummy message.")));

        var messages = _logger.GetRecords(new EventId(0, "ResilienceEvent")).ToList();

        messages.Should().HaveCount(1);

        if (!noOutcome)
        {
            messages[0].Exception.Should().NotBeNull();
        }

        if (noOutcome)
        {
            messages[0].Message.Should().Be("Resilience event occurred. EventName: 'my-event', Builder Name: 'my-builder', Strategy Name: 'my-strategy', Strategy Type: 'my-strategy-type', Strategy Key: 'my-strategy-key', Result: '(null)'");
        }
        else
        {
            messages[0].Message.Should().Be("Resilience event occurred. EventName: 'my-event', Builder Name: 'my-builder', Strategy Name: 'my-strategy', Strategy Type: 'my-strategy-type', Strategy Key: 'my-strategy-key', Result: 'Dummy message.'");
        }
    }

    [Fact]
    public void WriteEvent_LoggingWithoutStrategyKey_Ok()
    {
        var telemetry = Create();
        ReportEvent(telemetry, null, strategyKey: null);

        var messages = _logger.GetRecords(new EventId(0, "ResilienceEvent")).ToList();

        messages[0].Message.Should().Be("Resilience event occurred. EventName: 'my-event', Builder Name: 'my-builder', Strategy Name: 'my-strategy', Strategy Type: 'my-strategy-type', Strategy Key: '(null)', Result: '(null)'");
    }

    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [Theory]
    public void WriteEvent_MeteringWithoutEnrichers_Ok(bool noOutcome, bool exception)
    {
        var telemetry = Create();
        Outcome<object>? outcome = noOutcome switch
        {
            false => null,
            true when exception => new Outcome<object>(new InvalidOperationException("Dummy message.")),
            _ => new Outcome<object>(true)
        };
        ReportEvent(telemetry, outcome, context: ResilienceContext.Get().WithResultType<bool>());

        var events = GetEvents("resilience-events");
        events.Should().HaveCount(1);
        var ev = events[0];

        ev.Count.Should().Be(7);
        ev["event-name"].Should().Be("my-event");
        ev["strategy-type"].Should().Be("my-strategy-type");
        ev["strategy-name"].Should().Be("my-strategy");
        ev["strategy-key"].Should().Be("my-strategy-key");
        ev["builder-name"].Should().Be("my-builder");
        ev["result-type"].Should().Be("Boolean");

        if (outcome?.Exception is not null)
        {
            ev["exception-name"].Should().Be("System.InvalidOperationException");
        }
        else
        {
            ev["exception-name"].Should().Be(null);
        }
    }

    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [Theory]
    public void WriteExecutionAttemptEvent_Metering_Ok(bool noOutcome, bool exception)
    {
        var telemetry = Create();
        var attemptArg = new ExecutionAttemptArguments(5, TimeSpan.FromSeconds(50), true);
        Outcome<object>? outcome = noOutcome switch
        {
            false => null,
            true when exception => new Outcome<object>(new InvalidOperationException("Dummy message.")),
            _ => new Outcome<object>(true)
        };
        ReportEvent(telemetry, outcome, context: ResilienceContext.Get().WithResultType<bool>(), arg: attemptArg);

        var events = GetEvents("execution-attempt-duration");
        events.Should().HaveCount(1);
        var ev = events[0];

        ev.Count.Should().Be(9);
        ev["event-name"].Should().Be("my-event");
        ev["strategy-type"].Should().Be("my-strategy-type");
        ev["strategy-name"].Should().Be("my-strategy");
        ev["strategy-key"].Should().Be("my-strategy-key");
        ev["builder-name"].Should().Be("my-builder");
        ev["result-type"].Should().Be("Boolean");
        ev["attempt-number"].Should().Be(5);
        ev["attempt-handled"].Should().Be(true);

        if (outcome?.Exception is not null)
        {
            ev["exception-name"].Should().Be("System.InvalidOperationException");
        }
        else
        {
            ev["exception-name"].Should().Be(null);
        }

        _events.Single(v => v.Name == "execution-attempt-duration").Measurement.Should().Be(50000);
    }

    [InlineData(1)]
    [InlineData(100)]
    [Theory]
    public void WriteEvent_MeteringWithEnrichers_Ok(int count)
    {
        const int DefaultDimensions = 7;
        var telemetry = Create(enrichers =>
        {
            enrichers.Add(context =>
            {
                for (int i = 0; i < count; i++)
                {
                    context.Tags.Add(new KeyValuePair<string, object?>($"custom-{i}", $"custom-{i}-value"));
                }
            });

            enrichers.Add(context =>
            {
                context.Tags.Add(new KeyValuePair<string, object?>("other", "other-value"));
            });
        });

        ReportEvent(telemetry, new Outcome<object>(true));

        var events = GetEvents("resilience-events");
        var ev = events[0];
        ev.Count.Should().Be(DefaultDimensions + count + 1);
        ev["other"].Should().Be("other-value");

        for (int i = 0; i < count; i++)
        {
            ev[$"custom-{i}"].Should().Be($"custom-{i}-value");
        }
    }

    [Fact]
    public void WriteEvent_MeteringWithoutStrategyKey_Ok()
    {
        var telemetry = Create();
        ReportEvent(telemetry, null, strategyKey: null);
        var events = GetEvents("resilience-events")[0]["strategy-key"].Should().BeNull();
    }

    private List<Dictionary<string, object?>> GetEvents(string eventName) => _events.Where(e => e.Name == eventName).Select(v => v.Tags).ToList();

    private ResilienceTelemetryDiagnosticSource Create(Action<ICollection<Action<EnrichmentContext>>>? configureEnrichers = null)
    {
        var options = new TelemetryOptions
        {
            LoggerFactory = _loggerFactory
        };

        configureEnrichers?.Invoke(options.Enrichers);

        return new(options);
    }

    private static void ReportEvent(
        ResilienceTelemetryDiagnosticSource telemetry,
        Outcome<object>? outcome,
        string? strategyKey = "my-strategy-key",
        ResilienceContext? context = null,
        object? arg = null)
    {
        context ??= ResilienceContext.Get();
        var props = new ResilienceProperties();
        if (!string.IsNullOrEmpty(strategyKey))
        {
            props.Set(new ResiliencePropertyKey<string?>("Polly.StrategyKey"), strategyKey);
        }

        telemetry.ReportEvent(
            "my-event",
            "my-builder",
            props,
            "my-strategy",
            "my-strategy-type",
            context,
            outcome,
            arg ?? new TestArguments());
    }
}
