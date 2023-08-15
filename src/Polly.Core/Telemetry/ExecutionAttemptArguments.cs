namespace Polly.Telemetry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments that encapsulate the execution attempt for retries or hedging.
/// </summary>
/// <remarks>
/// Always use constructor when creating this struct, otherwise we do not guarantee the binary compatibility.
/// </remarks>
public readonly struct ExecutionAttemptArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionAttemptArguments"/> struct.
    /// </summary>
    /// <param name="attemptNumber">The execution attempt number.</param>
    /// <param name="duration">The execution duration.</param>
    /// <param name="handled">Determines whether the attempt was handled by the strategy.</param>
    public ExecutionAttemptArguments(int attemptNumber, TimeSpan duration, bool handled)
    {
        AttemptNumber = attemptNumber;
        Duration = duration;
        Handled = handled;
    }

    /// <summary>
    /// Gets the attempt number.
    /// </summary>
    public int AttemptNumber { get; }

    /// <summary>
    /// Gets the execution duration of the attempt.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets a value indicating whether the outcome was handled by retry or hedging strategy.
    /// </summary>
    public bool Handled { get; }
}
