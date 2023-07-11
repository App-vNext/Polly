namespace Polly.Testing;

/// <summary>
/// Describes the pipeline of resilience strategy.
/// </summary>
/// <param name="Strategies">The strategies the pipeline is composed of.</param>
/// <param name="HasTelemetry">Gets a value indicating whether the pipeline has telemetry enabled.</param>
/// <param name="IsReloadable">Gets a value indicating whether the resilience strategy is reloadable.</param>
public record class InnerStrategiesDescriptor(IReadOnlyList<ResilienceStrategyDescriptor> Strategies, bool HasTelemetry, bool IsReloadable);
