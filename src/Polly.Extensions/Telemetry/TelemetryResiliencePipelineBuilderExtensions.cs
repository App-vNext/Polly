using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly;

/// <summary>
/// The telemetry extensions for the <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class TelemetryResiliencePipelineBuilderExtensions
{
    /// <summary>
    /// Enables telemetry for this builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="loggerFactory">The logger factory to be used for logging.</param>
    /// <returns>The builder instance with the telemetry enabled.</returns>
    /// <remarks>
    /// By enabling telemetry, the resilience pipeline will log and meter all resilience events.
    /// Additionally, the telemetry strategy that logs and meters the executions is added to the beginning of the composite strategy.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="loggerFactory"/> is <see langword="null"/>.</exception>
    public static TBuilder ConfigureTelemetry<TBuilder>(this TBuilder builder, ILoggerFactory loggerFactory)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(loggerFactory);

        return builder.ConfigureTelemetry(new TelemetryOptions { LoggerFactory = loggerFactory });
    }

    /// <summary>
    /// Enables telemetry for this builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The resilience telemetry options.</param>
    /// <returns>The builder instance with the telemetry enabled.</returns>
    /// <remarks>
    /// By enabling telemetry, the resilience pipeline will log and meter all resilience events.
    /// Additionally, the telemetry strategy that logs and meters the executions is added to the beginning of the composite strategy.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TelemetryOptions))]
    public static TBuilder ConfigureTelemetry<TBuilder>(this TBuilder builder, TelemetryOptions options)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(new(options, $"The '{nameof(TelemetryOptions)}' are invalid."));
        builder.TelemetryListener = new TelemetryListenerImpl(options);

        return builder;
    }
}
