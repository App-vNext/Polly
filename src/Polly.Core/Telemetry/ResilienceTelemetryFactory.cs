namespace Polly.Telemetry;

#pragma warning disable S1694 // An abstract class should have both abstract and concrete methods

/// <summary>
/// Factory used to created instances of <see cref="ResilienceTelemetry"/>.
/// </summary>
public abstract class ResilienceTelemetryFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="ResilienceTelemetry"/>.
    /// </summary>
    /// <param name="context">The context associated with the creation of <see cref="ResilienceTelemetry"/>.</param>
    /// <returns>An instance of <see cref="ResilienceTelemetry"/>.</returns>
    public abstract ResilienceTelemetry Create(ResilienceTelemetryFactoryContext context);
}
