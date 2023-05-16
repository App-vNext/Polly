using Microsoft.Extensions.Logging;
using Polly.Extensions.Telemetry;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly;

/// <summary>
/// The telemetry extensions for the <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class TelemetryResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Enables telemetry for this builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="loggerFactory">The logger factory to be used for logging.</param>
    /// <returns>The builder instance with the telemetry enabled.</returns>
    /// <remarks>
    /// By enabling the telemetry the resilience strategy will log and meter all resilience events.
    /// Additionally, the telemetry strategy that logs and meters the executions is added to the beginning of the strategy pipeline.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="loggerFactory"/> is <see langword="null"/>.</exception>
    public static ResilienceStrategyBuilder EnableTelemetry(this ResilienceStrategyBuilder builder, ILoggerFactory loggerFactory)
    {
        Guard.NotNull(builder);
        Guard.NotNull(loggerFactory);

        return builder.EnableTelemetry(new TelemetryResilienceStrategyOptions { LoggerFactory = loggerFactory });
    }

    /// <summary>
    /// Enables telemetry for this builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The resilience telemetry options.</param>
    /// <returns>The builder instance with the telemetry enabled.</returns>
    /// <remarks>
    /// By enabling the telemetry the resilience strategy will log and meter all resilience events.
    /// Additionally, the telemetry strategy that logs and meters the executions is added to the beginning of the strategy pipeline.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    public static ResilienceStrategyBuilder EnableTelemetry(this ResilienceStrategyBuilder builder, TelemetryResilienceStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The resilience telemetry options are invalid.");

        builder.Properties.Set(
            TelemetryUtil.DiagnosticSourceKey,
            new ResilienceTelemetryDiagnosticSource(options));

        builder.OnCreatingStrategy = strategies =>
        {
            var telemetryStrategy = new TelemetryResilienceStrategy(
                builder.TimeProvider,
                builder.BuilderName,
                builder.Properties.GetValue(TelemetryUtil.StrategyKey, null!),
                options.LoggerFactory,
                options.Enrichers.ToList());

            strategies.Insert(0, telemetryStrategy);
        };

        return builder;
    }
}
