using Polly.Telemetry;

namespace Polly;

/// <summary>
/// The context used for building an individual resilience strategy.
/// </summary>
public sealed class StrategyBuilderContext
{
    internal StrategyBuilderContext(ResilienceStrategyTelemetry telemetry, TimeProvider timeProvider)
    {
        TimeProvider = timeProvider;
        Telemetry = telemetry;
    }

    /// <summary>
    /// Gets the resilience telemetry used to report important events.
    /// </summary>
    public ResilienceStrategyTelemetry Telemetry { get; }

    /// <summary>
    /// Gets the <see cref="TimeProvider"/> used by this strategy.
    /// </summary>
    internal TimeProvider TimeProvider { get; }
}
