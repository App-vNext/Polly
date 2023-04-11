using Polly.Strategy;

namespace Polly.Telemetry;

#pragma warning disable S1694 // An abstract class should have both abstract and concrete methods

/// <summary>
/// Resilience telemetry is used by individual resilience strategies to report some important events.
/// </summary>
/// <remarks>
/// For example, the timeout strategy reports "OnTimeout" event when the timeout is reached or "OnRetry" for retry strategy.
/// </remarks>
public abstract class ResilienceTelemetry
{
    /// <summary>
    /// Reports an event that occurred in the resilience strategy.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    /// <param name="context">The context associated with the event.</param>
    public abstract void Report(string eventName, ResilienceContext context);

    /// <summary>
    /// Reports an event that occurred in the resilience strategy.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="eventName">The event name.</param>
    /// <param name="outcome">The outcome associated with the event.</param>
    /// <param name="context">The context associated with the event.</param>
    public abstract void Report<TResult>(string eventName, Outcome<TResult> outcome, ResilienceContext context);
}
