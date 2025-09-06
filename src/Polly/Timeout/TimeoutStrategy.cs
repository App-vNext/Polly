namespace Polly.Timeout;

/// <summary>
/// Defines how timeouts are enforced.
/// </summary>
public enum TimeoutStrategy
{
    /// <summary>
    /// An optimistic <see cref="TimeoutStrategy"/>. The <see cref="TimeoutPolicy"/> relies on a timing-out <see cref="CancellationToken"/> to cancel executed delegates by co-operative cancellation.
    /// </summary>
    Optimistic,

    /// <summary>
    /// A pessimistic <see cref="TimeoutStrategy"/>. The <see cref="TimeoutPolicy"/> will assume the delegates passed to be executed will not necessarily honor
    /// any timing-out <see cref="CancellationToken"/> but the policy will still guarantee timing out (and returning to the caller) by other means.
    /// </summary>
    Pessimistic
}
