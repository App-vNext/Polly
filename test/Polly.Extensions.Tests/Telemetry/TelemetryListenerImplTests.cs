using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Polly.Telemetry;

namespace Polly.Extensions.Tests.Telemetry;

#pragma warning disable S103 // Lines should not be too long

[Collection(nameof(NonParallelizableCollection))]
public class TelemetryListenerImplTests : IDisposable
{
    private readonly FakeLogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly List<MeteringEvent> _events = new(1024);
    private Action<TelemetryEventArguments<object, object>>? _onEvent;

    public TelemetryListenerImplTests() => _loggerFactory = TestUtilities.CreateLoggerFactory(out _logger);

    public void Dispose() => _loggerFactory.Dispose();

    [Fact]
    public void Meter_Ok()
    {
        TelemetryListenerImpl.Meter.Name.Should().Be("Polly");
        TelemetryListenerImpl.Meter.Version.Should().Be("1.0");
        var source = new TelemetryListenerImpl(new TelemetryOptions());

        source.Counter.Description.Should().Be("Tracks the number of resilience events that occurred in resilience strategies.");
        source.AttemptDuration.Description.Should().Be("Tracks the duration of execution attempts.");
        source.AttemptDuration.Unit.Should().Be("ms");

        source.ExecutionDuration.Description.Should().Be("The execution duration of resilience pipelines.");
        source.ExecutionDuration.Unit.Should().Be("ms");
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void WriteEvent_LoggingWithOutcome_Ok(bool noOutcome)
    {
        var telemetry = Create();
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        ReportEvent(telemetry, noOutcome ? null : Outcome.FromResult<object>(response));

        var messages = _logger.GetRecords(new EventId(0, "ResilienceEvent")).ToList();
        messages.Should().HaveCount(1);

        if (noOutcome)
        {
            messages[0].Message.Should().Be("Resilience event occurred. EventName: 'my-event', Source: 'my-pipeline/my-instance/my-strategy', Operation Key: 'op-key', Result: ''");
        }
        else
        {
            messages[0].Message.Should().Be("Resilience event occurred. EventName: 'my-event', Source: 'my-pipeline/my-instance/my-strategy', Operation Key: 'op-key', Result: '200'");
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void WriteEvent_LoggingWithException_Ok(bool noOutcome)
    {
        var telemetry = Create();
        ReportEvent(telemetry, noOutcome ? null : Outcome.FromException<object>(new InvalidOperationException("Dummy message.")));

        var messages = _logger.GetRecords(new EventId(0, "ResilienceEvent")).ToList();

        messages.Should().HaveCount(1);

        if (!noOutcome)
        {
            messages[0].Exception.Should().NotBeNull();
        }

        if (noOutcome)
        {
            messages[0].Message.Should().Be("Resilience event occurred. EventName: 'my-event', Source: 'my-pipeline/my-instance/my-strategy', Operation Key: 'op-key', Result: ''");
        }
        else
        {
            messages[0].Message.Should().Be("Resilience event occurred. EventName: 'my-event', Source: 'my-pipeline/my-instance/my-strategy', Operation Key: 'op-key', Result: 'Dummy message.'");
        }
    }

    [Fact]
    public void WriteEvent_LoggingWithoutInstanceName_Ok()
    {
        var telemetry = Create();
        ReportEvent(telemetry, null, instanceName: null);

        var messages = _logger.GetRecords(new EventId(0, "ResilienceEvent")).ToList();

        messages[0].Message.Should().Be("Resilience event occurred. EventName: 'my-event', Source: 'my-pipeline/(null)/my-strategy', Operation Key: 'op-key', Result: ''");
    }

    [InlineData(ResilienceEventSeverity.Error, LogLevel.Error)]
    [InlineData(ResilienceEventSeverity.Warning, LogLevel.Warning)]
    [InlineData(ResilienceEventSeverity.Information, LogLevel.Information)]
    [InlineData(ResilienceEventSeverity.Debug, LogLevel.Debug)]
    [InlineData(ResilienceEventSeverity.Critical, LogLevel.Critical)]
    [InlineData(ResilienceEventSeverity.None, LogLevel.None)]
    [InlineData((ResilienceEventSeverity)99, LogLevel.None)]
    [Theory]
    public void WriteEvent_EnsureSeverityRespected(ResilienceEventSeverity severity, LogLevel logLevel)
    {
        using var metering = TestUtilities.EnablePollyMetering(_events);

        var telemetry = Create();
        ReportEvent(telemetry, null, severity: severity);

        var messages = _logger.GetRecords(new EventId(0, "ResilienceEvent")).ToList();

        messages[0].LogLevel.Should().Be(logLevel);

        var events = GetEvents("resilience.polly.strategy.events");
        events.Should().HaveCount(1);
        var ev = events[0]["event.severity"].Should().Be(severity.ToString());
    }

    [Fact]
    public void WriteExecutionAttempt_LoggingWithException_Ok()
    {
        var telemetry = Create();
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        ReportEvent(telemetry, Outcome.FromException<object>(new InvalidOperationException("Dummy message.")), arg: new ExecutionAttemptArguments(4, TimeSpan.FromMilliseconds(123), true));

        var messages = _logger.GetRecords(new EventId(3, "ExecutionAttempt")).ToList();
        messages.Should().HaveCount(1);

        messages[0].Message.Should().Be("Execution attempt. Source: 'my-pipeline/my-instance/my-strategy', Operation Key: 'op-key', Result: 'Dummy message.', Handled: 'True', Attempt: '4', Execution Time: '123'");
    }

    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [Theory]
    public void WriteExecutionAttempt_LoggingWithOutcome_Ok(bool noOutcome, bool handled)
    {
        var telemetry = Create();
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        ReportEvent(telemetry, noOutcome ? null : Outcome.FromResult<object>(response), arg: new ExecutionAttemptArguments(4, TimeSpan.FromMilliseconds(123), handled));

        var messages = _logger.GetRecords(new EventId(3, "ExecutionAttempt")).ToList();
        messages.Should().HaveCount(1);

        if (noOutcome)
        {
            string resultString = string.Empty;
            messages[0].Message.Should().Be($"Execution attempt. Source: 'my-pipeline/my-instance/my-strategy', Operation Key: 'op-key', Result: '{resultString}', Handled: '{handled}', Attempt: '4', Execution Time: '123'");
        }
        else
        {
            messages[0].Message.Should().Be($"Execution attempt. Source: 'my-pipeline/my-instance/my-strategy', Operation Key: 'op-key', Result: '200', Handled: '{handled}', Attempt: '4', Execution Time: '123'");
        }

        messages[0].LogLevel.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void WriteExecutionAttempt_NotEnabled_EnsureNotLogged()
    {
        var telemetry = Create();
        _logger.Enabled = false;

        ReportEvent(telemetry, null, arg: new ExecutionAttemptArguments(4, TimeSpan.FromMilliseconds(123), true));

        var messages = _logger.GetRecords(new EventId(3, "ExecutionAttempt")).ToList();
        messages.Should().HaveCount(0);
    }

    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [Theory]
    public void WriteEvent_MeteringWithoutEnrichers_Ok(bool noOutcome, bool exception)
    {
        using var metering = TestUtilities.EnablePollyMetering(_events);
        var telemetry = Create();
        Outcome<object>? outcome = noOutcome switch
        {
            false => null,
            true when exception => Outcome.FromException<object>(new InvalidOperationException("Dummy message.")),
            _ => Outcome.FromResult<object>(true)
        };
        ReportEvent(telemetry, outcome, context: ResilienceContextPool.Shared.Get("op-key").WithResultType<bool>());

        var events = GetEvents("resilience.polly.strategy.events");
        events.Should().HaveCount(1);
        var ev = events[0];

        if (noOutcome && exception)
        {
            ev.Count.Should().Be(7);
        }
        else
        {
            ev.Count.Should().Be(6);
        }

        ev["event.name"].Should().Be("my-event");
        ev["event.severity"].Should().Be("Warning");
        ev["pipeline.name"].Should().Be("my-pipeline");
        ev["pipeline.instance"].Should().Be("my-instance");
        ev["operation.key"].Should().Be("op-key");
        ev["strategy.name"].Should().Be("my-strategy");

        if (outcome?.Exception is not null)
        {
            ev["exception.type"].Should().Be("System.InvalidOperationException");
        }
        else
        {
            ev.Should().NotContainKey("exception.type");
        }
    }

    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [Theory]
    public void WriteExecutionAttemptEvent_Metering_Ok(bool noOutcome, bool exception)
    {
        using var metering = TestUtilities.EnablePollyMetering(_events);
        var telemetry = Create();
        var attemptArg = new ExecutionAttemptArguments(5, TimeSpan.FromSeconds(50), true);
        Outcome<object>? outcome = noOutcome switch
        {
            false => null,
            true when exception => Outcome.FromException<object>(new InvalidOperationException("Dummy message.")),
            _ => Outcome.FromResult<object>(true)
        };
        ReportEvent(telemetry, outcome, context: ResilienceContextPool.Shared.Get("op-key").WithResultType<bool>(), arg: attemptArg);

        var events = GetEvents("resilience.polly.strategy.attempt.duration");
        events.Should().HaveCount(1);
        var ev = events[0];

        if (noOutcome && exception)
        {
            ev.Count.Should().Be(9);
        }
        else
        {
            ev.Count.Should().Be(8);
        }

        ev["event.name"].Should().Be("my-event");
        ev["event.severity"].Should().Be("Warning");
        ev["pipeline.name"].Should().Be("my-pipeline");
        ev["pipeline.instance"].Should().Be("my-instance");
        ev["operation.key"].Should().Be("op-key");
        ev["pipeline.name"].Should().Be("my-pipeline");
        ev["attempt.number"].Should().Be(5);
        ev["attempt.handled"].Should().Be(true);

        if (outcome?.Exception is not null)
        {
            ev["exception.type"].Should().Be("System.InvalidOperationException");
        }
        else
        {
            ev.Should().NotContainKey("exception.type");
        }

        _events.Single(v => v.Name == "resilience.polly.strategy.attempt.duration").Measurement.Should().Be(50000);
    }

    [Fact]
    public void WriteExecutionAttemptEvent_ShouldBeSkipped()
    {
        using var metering = TestUtilities.EnablePollyMetering(_events, _ => false);

        var telemetry = Create();
        var attemptArg = new ExecutionAttemptArguments(5, TimeSpan.FromSeconds(50), true);
        ReportEvent(telemetry, Outcome.FromResult<object>(true), context: ResilienceContextPool.Shared.Get("op-key").WithResultType<bool>(), arg: attemptArg);

        var events = GetEvents("resilience.polly.strategy.attempt.duration");
        events.Should().HaveCount(0);
    }

    [InlineData(1)]
    [InlineData(100)]
    [Theory]
    public void WriteEvent_MeteringWithEnrichers_Ok(int count)
    {
        using var metering = TestUtilities.EnablePollyMetering(_events);

        const int DefaultDimensions = 6;

        var telemetry = Create(new[]
        {
            new CallbackEnricher(context =>
            {
                for (int i = 0; i < count; i++)
                {
                    context.Tags.Add(new KeyValuePair<string, object?>($"custom-{i}", $"custom-{i}-value"));
                }
            }),

            new CallbackEnricher(context =>
            {
                context.Tags.Add(new KeyValuePair<string, object?>("other", "other-value"));
            })
        });

        ReportEvent(telemetry, Outcome.FromResult<object>(true));

        var events = GetEvents("resilience.polly.strategy.events");
        var ev = events[0];
        ev.Count.Should().Be(DefaultDimensions + count + 1);
        ev["other"].Should().Be("other-value");

        for (int i = 0; i < count; i++)
        {
            ev[$"custom-{i}"].Should().Be($"custom-{i}-value");
        }
    }

    [Fact]
    public void WriteEvent_MeteringWithoutBuilderInstance_Ok()
    {
        using var metering = TestUtilities.EnablePollyMetering(_events);
        var telemetry = Create();
        ReportEvent(telemetry, null, instanceName: null);
        var events = GetEvents("resilience.polly.strategy.events")[0].Should().NotContainKey("pipeline.instance");
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void OnTelemetryEvent_Ok(bool hasCallback)
    {
        var called = false;

        if (hasCallback)
        {
            _onEvent = e => called = true;
        }

        var telemetry = Create();
        ReportEvent(telemetry, null, instanceName: null);

        called.Should().Be(hasCallback);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void PipelineExecution_Logged(bool exception)
    {
        var context = ResilienceContextPool.Shared.Get("op-key").WithResultType<int>();
        var telemetry = Create();
        var outcome = exception ? Outcome.FromException<object>(new InvalidOperationException("dummy message")) : Outcome.FromResult((object)10);
        var result = exception ? "dummy message" : "10";

        ReportEvent(telemetry, outcome: outcome, arg: default(PipelineExecutingArguments), context: context);
        ReportEvent(telemetry, outcome: outcome, arg: new PipelineExecutedArguments(TimeSpan.FromSeconds(10)), context: context);

        var messages = _logger.GetRecords(new EventId(1, "StrategyExecuting")).ToList();
        messages.Should().HaveCount(1);
        messages[0].Message.Should().Be("Resilience pipeline executing. Source: 'my-pipeline/my-instance', Operation Key: 'op-key'");
        messages = _logger.GetRecords(new EventId(2, "StrategyExecuted")).ToList();
        messages.Should().HaveCount(1);
        messages[0].Message.Should().Match($"Resilience pipeline executed. Source: 'my-pipeline/my-instance', Operation Key: 'op-key', Result: '{result}', Execution Time: 10000ms");
        messages[0].LogLevel.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void PipelineExecution_VoidResult_Ok()
    {
        var context = ResilienceContextPool.Shared.Get("op-key").WithVoidResultType();
        var telemetry = Create();
        ReportEvent(telemetry, outcome: null, arg: default(PipelineExecutingArguments), context: context);

        var messages = _logger.GetRecords(new EventId(1, "StrategyExecuting")).ToList();
        messages.Should().HaveCount(1);
        messages[0].Message.Should().Be("Resilience pipeline executing. Source: 'my-pipeline/my-instance', Operation Key: 'op-key'");
    }

    [Fact]
    public void PipelineExecution_NoOutcome_Logged()
    {
        var context = ResilienceContextPool.Shared.Get("op-key").WithResultType<int>();
        var telemetry = Create();

        ReportEvent(telemetry, outcome: null, arg: new PipelineExecutedArguments(TimeSpan.FromSeconds(10)), context: context);

        var messages = _logger.GetRecords(new EventId(2, "StrategyExecuted")).ToList();
        messages[0].Message.Should().Match($"Resilience pipeline executed. Source: 'my-pipeline/my-instance', Operation Key: 'op-key', Result: '', Execution Time: 10000ms");
    }

    [InlineData(false)]
    [InlineData(true)]
    [Theory]
    public void PipelineExecution_Metered(bool exception)
    {
        using var metering = TestUtilities.EnablePollyMetering(_events);

        var context = ResilienceContextPool.Shared.Get("op-key").WithResultType<int>();
        var outcome = exception ? Outcome.FromException<object>(new InvalidOperationException("dummy message")) : Outcome.FromResult((object)10);
        var result = exception ? "dummy message" : "10";

        var telemetry = Create(new[]
        {
            new CallbackEnricher(context =>
            {
                if (exception)
                {
                    context.TelemetryEvent.Outcome!.Value.Exception.Should().BeOfType<InvalidOperationException>();
                }

                context.Tags.Add(new("custom-tag", "custom-tag-value"));
            })
        });

        ReportEvent(telemetry, outcome: outcome, arg: new PipelineExecutedArguments(TimeSpan.FromSeconds(10)), context: context);

        var ev = _events.Single(v => v.Name == "resilience.polly.pipeline.duration").Tags;

        ev.Count.Should().Be(exception ? 8 : 7);
        ev["pipeline.instance"].Should().Be("my-instance");
        ev["operation.key"].Should().Be("op-key");
        ev["pipeline.name"].Should().Be("my-pipeline");
        ev["event.name"].Should().Be("my-event");
        ev["event.severity"].Should().Be("Warning");
        ev["pipeline.name"].Should().Be("my-pipeline");
        ev["custom-tag"].Should().Be("custom-tag-value");

        if (exception)
        {
            ev["exception.type"].Should().Be("System.InvalidOperationException");
        }
        else
        {
            ev.Should().NotContainKey("exception.type");
        }
    }

    [Fact]
    public void PipelineExecuted_ShouldBeSkipped()
    {
        using var metering = TestUtilities.EnablePollyMetering(_events, _ => false);

        var telemetry = Create();
        var attemptArg = new PipelineExecutedArguments(TimeSpan.FromSeconds(50));
        ReportEvent(telemetry, Outcome.FromResult<object>(true), context: ResilienceContextPool.Shared.Get("op-key").WithResultType<bool>(), arg: attemptArg);

        var events = GetEvents("resilience.polly.pipeline.duration");
        events.Should().HaveCount(0);
    }

    private List<Dictionary<string, object?>> GetEvents(string eventName) => _events.Where(e => e.Name == eventName).Select(v => v.Tags).ToList();

    private TelemetryListenerImpl Create(IEnumerable<MeteringEnricher>? enrichers = null)
    {
        var options = new TelemetryOptions
        {
            LoggerFactory = _loggerFactory,
        };

        if (_onEvent is not null)
        {
            options.TelemetryListeners.Add(new FakeTelemetryListener(_onEvent));
        }

        if (enrichers != null)
        {
            foreach (var enricher in enrichers)
            {
                options.MeteringEnrichers.Add(enricher);
            }
        }

        return new(options);
    }

    private static void ReportEvent(
        TelemetryListenerImpl telemetry,
        Outcome<object>? outcome,
        string? instanceName = "my-instance",
        ResilienceContext? context = null,
        TestArguments? arg = null,
        ResilienceEventSeverity severity = ResilienceEventSeverity.Warning) => ReportEvent<TestArguments>(telemetry, outcome, instanceName, context, arg!, severity);

    private static void ReportEvent<TArgs>(
        TelemetryListenerImpl telemetry,
        Outcome<object>? outcome,
        string? instanceName = "my-instance",
        ResilienceContext? context = null,
        TArgs arg = default!,
        ResilienceEventSeverity severity = ResilienceEventSeverity.Warning)
    {
        telemetry.Write(
            new TelemetryEventArguments<object, TArgs>(
                new ResilienceTelemetrySource("my-pipeline", instanceName, "my-strategy"),
                new ResilienceEvent(severity, "my-event"),
                context ?? ResilienceContextPool.Shared.Get("op-key"),
                arg!,
                outcome));
    }

    private class CallbackEnricher : MeteringEnricher
    {
        private readonly Action<EnrichmentContext<object, object>> _callback;

        public CallbackEnricher(Action<EnrichmentContext<object, object>> callback) => _callback = callback;

        public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
        {
            _callback(new EnrichmentContext<object, object>(context.TelemetryEvent.AsObjectArguments(), context.Tags));
        }
    }
}
