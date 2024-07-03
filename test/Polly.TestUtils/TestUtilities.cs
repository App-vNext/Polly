using System;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Polly.Telemetry;

namespace Polly.TestUtils;

#pragma warning disable CA1031 // Do not catch general exception types

public static class TestUtilities
{
    public static Task AssertWithTimeoutAsync(Func<Task> assertion) => AssertWithTimeoutAsync(assertion, TimeSpan.FromSeconds(60));

    public static Task AssertWithTimeoutAsync(Action assertion) => AssertWithTimeoutAsync(
        () =>
        {
            assertion();
            return Task.CompletedTask;
        },
        TimeSpan.FromSeconds(60));

    public static Task AssertWithTimeoutAsync(Func<Task> assertion, TimeSpan timeout)
    {
        if (assertion is null)
        {
            throw new ArgumentNullException(nameof(assertion));
        }

        return AssertWithTimeoutInternalAsync(assertion, timeout);
    }

    private static async Task AssertWithTimeoutInternalAsync(Func<Task> assertion, TimeSpan timeout)
    {
        var watch = Stopwatch.StartNew();

        while (true)
        {
            try
            {
                await assertion().ConfigureAwait(false);
                return;
            }
            catch (Exception) when (watch.Elapsed < timeout)
            {
                await Task.Delay(5).ConfigureAwait(false);
            }
        }
    }

    public static ResilienceStrategyTelemetry CreateResilienceTelemetry(TelemetryListener listener, ResilienceTelemetrySource? source = null)
        => new(source ?? new ResilienceTelemetrySource("dummy-builder", "dummy-instance", "strategy_name"), listener);

    public static ResilienceStrategyTelemetry CreateResilienceTelemetry(Action<TelemetryEventArguments<object, object>> callback, ResilienceTelemetrySource? source = null)
        => CreateResilienceTelemetry(new FakeTelemetryListener(callback), source);

    public static ILoggerFactory CreateLoggerFactory(out FakeLogger logger)
    {
        logger = new FakeLogger();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger("Polly").Returns(logger);

        return loggerFactory;
    }

    public static IDisposable EnablePollyMetering(ICollection<MeteringEvent> events, Predicate<Instrument>? shouldListen = null)
    {
        var stateStr = Guid.NewGuid().ToString();
        var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "Polly")
                {
                    if (shouldListen is not null && !shouldListen(instrument))
                    {
                        return;
                    }

                    listener.EnableMeasurementEvents(instrument, stateStr);
                }
            }
        };
        meterListener.SetMeasurementEventCallback<int>(OnMeasurementRecorded);
        meterListener.SetMeasurementEventCallback<double>(OnMeasurementRecorded);
        meterListener.Start();

        void OnMeasurementRecorded<T>(Instrument instrument, T measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? stateObj)
        {
            if (stateObj is string str && str == stateStr)
            {
                events.Add(new MeteringEvent(measurement!, instrument.Name, tags.ToArray().ToDictionary(v => v.Key, v => v.Value)));
            }
        }

        return meterListener;
    }

    public static ResilienceContext WithResultType<T>(this ResilienceContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.Initialize<T>(true);
        return context;
    }

    public static ResilienceContext WithVoidResultType(this ResilienceContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.Initialize<VoidResult>(true);
        return context;
    }

    public static TelemetryEventArguments<object, object> AsObjectArguments<T, TArgs>(this TelemetryEventArguments<T, TArgs> args)
    {
        Outcome<object>? outcome = args.Outcome switch
        {
            null => null,
            { Exception: { } ex } => Outcome.FromException<object>(ex),
            _ => Outcome.FromResult<object>(args.Outcome!.Value.Result),
        };

        return new TelemetryEventArguments<object, object>(
                args.Source,
                args.Event,
                args.Context,
                args.Arguments!,
                outcome);
    }
}
