using Polly.Strategy;

namespace Polly.Telemetry;

/// <summary>
/// A implementation of <see cref="ResilienceTelemetryFactory"/> that does nothing.
/// </summary>
public sealed class NullResilienceTelemetry : ResilienceTelemetry
{
    private NullResilienceTelemetry()
    {
    }

    /// <summary>
    /// Gets an instance of <see cref="NullResilienceTelemetry"/>.
    /// </summary>
    public static readonly NullResilienceTelemetry Instance = new();

    /// <inheritdoc/>
    public override void Report(string eventName, ResilienceContext context)
    {
    }

    /// <inheritdoc/>
    public override void Report<TResult>(string eventName, Outcome<TResult> outcome, ResilienceContext context)
    {
    }
}
