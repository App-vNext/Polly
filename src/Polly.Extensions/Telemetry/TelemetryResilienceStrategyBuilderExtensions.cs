using Microsoft.Extensions.Logging;
using Polly.Extensions.Telemetry;
using Polly.Extensions.Utils;
using Polly.Strategy;
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
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="loggerFactory">The logger factory to be used for logging.</param>
    /// <returns>The builder instance with the telemetry enabled.</returns>
    /// <remarks>
    /// By enabling telemetry, the resilience strategy will log and meter all resilience events.
    /// Additionally, the telemetry strategy that logs and meters the executions is added to the beginning of the strategy pipeline.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="loggerFactory"/> is <see langword="null"/>.</exception>
    public static TBuilder EnableTelemetry<TBuilder>(this TBuilder builder, ILoggerFactory loggerFactory)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(loggerFactory);

        return builder.EnableTelemetry(new TelemetryResilienceStrategyOptions { LoggerFactory = loggerFactory });
    }

    /// <summary>
    /// Enables telemetry for this builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The resilience telemetry options.</param>
    /// <returns>The builder instance with the telemetry enabled.</returns>
    /// <remarks>
    /// By enabling telemetry, the resilience strategy will log and meter all resilience events.
    /// Additionally, the telemetry strategy that logs and meters the executions is added to the beginning of the strategy pipeline.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    public static TBuilder EnableTelemetry<TBuilder>(this TBuilder builder, TelemetryResilienceStrategyOptions options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The resilience telemetry options are invalid.");

        builder.DiagnosticSource = new ResilienceTelemetryDiagnosticSource(options);

        builder.OnCreatingStrategy = strategies =>
        {
            var telemetryStrategy = new TelemetryResilienceStrategy(
                TimeProvider.System,
                builder.BuilderName,
                builder.Properties.GetValue(TelemetryUtil.StrategyKey, null!),
                options.LoggerFactory,
                options.Enrichers.ToList());

            strategies.Insert(0, telemetryStrategy);
        };

        return builder;
    }
}
