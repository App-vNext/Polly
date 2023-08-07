using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Polly.Extensions.Telemetry;
using Polly.Utils;

namespace Polly;

/// <summary>
/// The telemetry extensions for the <see cref="CompositeStrategyBuilder"/>.
/// </summary>
public static class TelemetryCompositeStrategyBuilderExtensions
{
    /// <summary>
    /// Enables telemetry for this builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="loggerFactory">The logger factory to be used for logging.</param>
    /// <returns>The builder instance with the telemetry enabled.</returns>
    /// <remarks>
    /// By enabling telemetry, the resilience strategy will log and meter all resilience events.
    /// Additionally, the telemetry strategy that logs and meters the executions is added to the beginning of the composite strategy.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="loggerFactory"/> is <see langword="null"/>.</exception>
    public static CompositeStrategyBuilder ConfigureTelemetry(this CompositeStrategyBuilder builder, ILoggerFactory loggerFactory)
    {
        Guard.NotNull(builder);
        Guard.NotNull(loggerFactory);

        return builder.ConfigureTelemetry(new TelemetryOptions { LoggerFactory = loggerFactory });
    }

    /// <summary>
    /// Enables telemetry for this builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The resilience telemetry options.</param>
    /// <returns>The builder instance with the telemetry enabled.</returns>
    /// <remarks>
    /// By enabling telemetry, the resilience strategy will log and meter all resilience events.
    /// Additionally, the telemetry strategy that logs and meters the executions is added to the beginning of the composite strategy.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TelemetryOptions))]
    public static CompositeStrategyBuilder ConfigureTelemetry(this CompositeStrategyBuilder builder, TelemetryOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        AddTelemetry(builder, options);

        return builder;
    }

    /// <summary>
    /// Enables telemetry for this builder.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="loggerFactory">The logger factory to be used for logging.</param>
    /// <returns>The builder instance with the telemetry enabled.</returns>
    /// <remarks>
    /// By enabling telemetry, the resilience strategy will log and meter all resilience events.
    /// Additionally, the telemetry strategy that logs and meters the executions is added to the beginning of the composite strategy.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="loggerFactory"/> is <see langword="null"/>.</exception>
    public static CompositeStrategyBuilder<TResult> ConfigureTelemetry<TResult>(this CompositeStrategyBuilder<TResult> builder, ILoggerFactory loggerFactory)
    {
        Guard.NotNull(builder);
        Guard.NotNull(loggerFactory);

        return builder.ConfigureTelemetry(new TelemetryOptions { LoggerFactory = loggerFactory });
    }

    /// <summary>
    /// Enables telemetry for this builder.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The resilience telemetry options.</param>
    /// <returns>The builder instance with the telemetry enabled.</returns>
    /// <remarks>
    /// By enabling telemetry, the resilience strategy will log and meter all resilience events.
    /// Additionally, the telemetry strategy that logs and meters the executions is added to the beginning of the composite strategy.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TelemetryOptions))]
    public static CompositeStrategyBuilder<TResult> ConfigureTelemetry<TResult>(this CompositeStrategyBuilder<TResult> builder, TelemetryOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        AddTelemetry(builder, options);

        return builder;
    }

    private static void AddTelemetry<TResult>(CompositeStrategyBuilderBase<TResult> builder, TelemetryOptions options)
    {
        builder.Validator(new(options, $"The '{nameof(TelemetryOptions)}' are invalid."));
        builder.DiagnosticSource = new ResilienceTelemetryDiagnosticSource(options);
        builder.OnCreatingStrategy = strategies =>
        {
            var telemetryStrategy = new TelemetryResilienceStrategy<TResult>(
                TimeProvider.System,
                builder.Name,
                builder.InstanceName,
                options.LoggerFactory,
                options.ResultFormatter,
                options.Enrichers.ToList());

            strategies.Insert(0, telemetryStrategy);
        };
    }
}
