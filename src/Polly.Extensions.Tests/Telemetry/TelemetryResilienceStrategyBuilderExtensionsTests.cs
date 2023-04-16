using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Polly.Extensions.Telemetry;
using Polly.Extensions.Tests.Helpers;
using Polly.Strategy;

namespace Polly.Extensions.Tests.Telemetry;

#pragma warning disable S103 // Lines should not be too long

public class TelemetryResilienceStrategyBuilderExtensionsTests : IDisposable
{
    private readonly ResilienceStrategyBuilder _builder = new();
    private readonly FakeLogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly MeterListener _meterListener;
    private readonly List<Dictionary<string, object?>> _events = new();

    public TelemetryResilienceStrategyBuilderExtensionsTests()
    {
        _builder.BuilderName = "dummy-builder";
        _builder.Properties.Set(new ResiliencePropertyKey<string>("StrategyKey"), "strategy-key");
        _loggerFactory = TestUtils.CreateLoggerFactory(out _logger);
        _meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "Polly")
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            }
        };
        _meterListener.SetMeasurementEventCallback<int>(OnMeasurementRecorded);
        _meterListener.Start();

        void OnMeasurementRecorded<T>(Instrument instrument, T measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
        {
            instrument.Name.Should().Be("resilience-events");
            _events.Add(tags.ToArray().ToDictionary(v => v.Key, v => v.Value));
        }
    }

    public void Dispose()
    {
        _loggerFactory.Dispose();
        _meterListener.Dispose();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void EnableTelemetry_LoggingWithOutcome_Ok(bool noOutcome)
    {
        var strategy = CreateStrategyWithTelemetry(noOutcome);

        strategy.Execute(_ => true);

        _logger.Messages.Should().HaveCount(1);
        _logger.Events.Should().HaveCount(1);

        _logger.Events[0].Name.Should().Be("ResilienceEvent");
        _logger.Events[0].Id.Should().Be(1);

        if (noOutcome)
        {
            _logger.Messages[0].Should().Be("Resilience event occurred. EventName: 'no-outcome', Builder Name: 'dummy-builder', Strategy Name: 'strategy-name', Strategy Type: 'strategy-type', Strategy Key: 'strategy-key', Outcome: 'null'");
        }
        else
        {
            _logger.Messages[0].Should().Be("Resilience event occurred. EventName: 'outcome', Builder Name: 'dummy-builder', Strategy Name: 'strategy-name', Strategy Type: 'strategy-type', Strategy Key: 'strategy-key', Outcome: 'True'");
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void EnableTelemetry_LoggingWithException_Ok(bool noOutcome)
    {
        var strategy = CreateStrategyWithTelemetry(noOutcome);

        try
        {
            strategy.Execute(_ => throw new InvalidOperationException("Dummy message."));
        }
        catch (InvalidOperationException)
        {
            // ok
        }

        _logger.Messages.Should().HaveCount(1);
        _logger.Exceptions.Should().HaveCount(1);

        if (noOutcome)
        {
            _logger.Messages[0].Should().Be("Resilience event occurred. EventName: 'no-outcome', Builder Name: 'dummy-builder', Strategy Name: 'strategy-name', Strategy Type: 'strategy-type', Strategy Key: 'strategy-key', Outcome: 'null'");
        }
        else
        {
            _logger.Messages[0].Should().Be("Resilience event occurred. EventName: 'outcome', Builder Name: 'dummy-builder', Strategy Name: 'strategy-name', Strategy Type: 'strategy-type', Strategy Key: 'strategy-key', Outcome: 'Dummy message.'");
        }
    }

    [Fact]
    public void EnableTelemetry_LoggingWithoutStrategyKey_Ok()
    {
        ((IDictionary<string, object?>)_builder.Properties).Remove("StrategyKey");
        var strategy = CreateStrategyWithTelemetry(true);

        try
        {
            strategy.Execute(_ => throw new InvalidOperationException("Dummy message."));
        }
        catch (InvalidOperationException)
        {
            // ok
        }

        _logger.Messages[0].Should().Be("Resilience event occurred. EventName: 'no-outcome', Builder Name: 'dummy-builder', Strategy Name: 'strategy-name', Strategy Type: 'strategy-type', Strategy Key: 'null', Outcome: 'null'");
    }

    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [Theory]
    public void EnableTelemetry_MeteringWithoutEnrichers_Ok(bool noOutcome, bool exception)
    {
        var strategy = CreateStrategyWithTelemetry(noOutcome);
        var exceptionReported = exception && !noOutcome;

        try
        {
            if (noOutcome)
            {
                strategy.Execute(_ => true);
            }
            else
            {
                strategy.Execute(_ =>
                {
                    if (exception)
                    {
                        throw new InvalidOperationException();
                    }
                });
            }
        }
        catch (InvalidOperationException)
        {
            // ok
        }

        _events.Should().HaveCount(1);

        var ev = _events[0];

        ev.Count.Should().Be(7);
        ev["event-name"].Should().Be(noOutcome ? "no-outcome" : "outcome");
        ev["strategy-type"].Should().Be("strategy-type");
        ev["strategy-name"].Should().Be("strategy-name");
        ev["strategy-type"].Should().Be("strategy-type");
        ev["strategy-key"].Should().Be("strategy-key");
        ev["builder-name"].Should().Be("dummy-builder");
        ev["result-type"].Should().Be(noOutcome ? "Boolean" : "void");

        if (exceptionReported)
        {
            ev["exception-name"].Should().Be("System.InvalidOperationException");
        }
        else
        {
            ev["exception-name"].Should().Be(null);
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void EnableTelemetry_MeteringWithEnrichers_Ok(bool noOutcome)
    {
        var strategy = CreateStrategyWithTelemetry(noOutcome, options =>
        {
            options.Enrichers.Add(context =>
            {
                if (noOutcome)
                {
                    context.Outcome.Should().BeNull();
                }
                else
                {
                    context.Outcome!.Value.Result.Should().Be(true);
                }

                context.ResilienceContext.Should().NotBeNull();
                context.ResilienceArguments.Should().BeOfType<TestArguments>();
                context.Tags.Add(new KeyValuePair<string, object?>("custom-1", "custom-1-value"));
            });

            options.Enrichers.Add(context =>
            {
                context.Tags.Add(new KeyValuePair<string, object?>("custom-2", "custom-2-value"));
            });
        });

        strategy.Execute(_ => true);
        strategy.Execute(_ => true);

        var ev = _events[0];
        ev.Count.Should().Be(9);

        ev["custom-1"].Should().Be("custom-1-value");
        ev["custom-2"].Should().Be("custom-2-value");

        ev = _events[1];
        ev.Count.Should().Be(9);

        ev["custom-1"].Should().Be("custom-1-value");
        ev["custom-2"].Should().Be("custom-2-value");
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void EnableTelemetry_MeteringWithoutStrategyKey_Ok(bool noOutcome)
    {
        ((IDictionary<string, object?>)_builder.Properties).Remove("StrategyKey");
        var strategy = CreateStrategyWithTelemetry(noOutcome);

        strategy.Execute(_ => true);

        _events[0]["strategy-key"].Should().BeNull();
    }

    [Fact]
    public void EnableTelemetry_InvalidOptions_Throws()
    {
        _builder
            .Invoking(b => b.EnableTelemetry(new ResilienceStrategyTelemetryOptions
            {
                LoggerFactory = null!,
                OutcomeFormatter = null!
            })).Should()
            .Throw<ValidationException>()
            .WithMessage("""
            The resilience telemetry options are invalid.

            Validation Errors:
            The LoggerFactory field is required.
            The OutcomeFormatter field is required.
            """);
    }

    private ResilienceStrategy CreateStrategyWithTelemetry(bool noOutcome, Action<ResilienceStrategyTelemetryOptions>? configure = null)
    {
        return _builder.AddStrategyAndEnableTelemetry(noOutcome, options =>
        {
            options.LoggerFactory = _loggerFactory;
            configure?.Invoke(options);

        });
    }
}
