namespace Polly.Telemetry;

/// <summary>
/// Arguments that encapsulate the execution attempt for retries or hedging.
/// </summary>
public sealed partial class ExecutionAttemptArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionAttemptArguments"/> class.
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

    private ExecutionAttemptArguments()
    {
    }

    /// <summary>
    /// Gets the attempt number.
    /// </summary>
    public int AttemptNumber { get; private set; }

    /// <summary>
    /// Gets the execution duration of the attempt.
    /// </summary>
    public TimeSpan Duration { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the outcome was handled by retry or hedging strategy.
    /// </summary>
    public bool Handled { get; private set; }
}
