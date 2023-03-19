namespace Polly.Telemetry;

/// <summary>
/// Factory that returns <see cref="NullResilienceTelemetry"/> instances.
/// </summary>
public sealed class NullResilienceTelemetryFactory : ResilienceTelemetryFactory
{
    /// <summary>
    /// Gets the singleton instance of the factory.
    /// </summary>
    public static readonly NullResilienceTelemetryFactory Instance = new();

    private NullResilienceTelemetryFactory()
    {
    }

    /// <inheritdoc/>
    public override ResilienceTelemetry Create(ResilienceTelemetryFactoryContext context) => NullResilienceTelemetry.Instance;
}
