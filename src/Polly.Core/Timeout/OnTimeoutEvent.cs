namespace Polly.Timeout;

/// <summary>
/// An event that is raised when a timeout occurs.
/// </summary>
/// <remarks>This class supports registering multiple on-timeout callbacks.
/// The registered callbacks are executed one-by-one in the same order as they were registered.</remarks>
public sealed class OnTimeoutEvent : SimpleEvent<OnTimeoutArguments, OnTimeoutEvent>
{
}
