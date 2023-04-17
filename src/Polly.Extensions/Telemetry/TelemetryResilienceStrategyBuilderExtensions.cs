using Polly.Extensions.Telemetry;
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
    /// <param name="options">The resilience telemetry options.</param>
    /// <returns>The builder instance with the telemetry enabled.</returns>
    public static ResilienceStrategyBuilder EnableTelemetry(this ResilienceStrategyBuilder builder, ResilienceStrategyTelemetryOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, "The resilience telemetry options are invalid.");

        builder.Properties.Set(
            Telemetry.TelemetryUtil.DiagnosticSourceKey,
            new ResilienceTelemetryDiagnosticSource(options));

        return builder;
    }
}
