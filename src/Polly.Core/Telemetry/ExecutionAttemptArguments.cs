﻿namespace Polly.Telemetry;

/// <summary>
/// Arguments that encapsulate the execution attempt for retries or hedging.
/// </summary>
public partial class ExecutionAttemptArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionAttemptArguments"/> class.
    /// </summary>
    /// <param name="attempt">The execution attempt.</param>
    /// <param name="executionTime">The execution time.</param>
    /// <param name="handled">Determines whether the attempt was handled by the strategy.</param>
    public ExecutionAttemptArguments(int attempt, TimeSpan executionTime, bool handled)
    {
        Attempt = attempt;
        ExecutionTime = executionTime;
        Handled = handled;
    }

    private ExecutionAttemptArguments()
    {
    }

    /// <summary>
    /// Gets the attempt number.
    /// </summary>
    public int Attempt { get; private set; }

    /// <summary>
    /// Gets the execution time of the attempt.
    /// </summary>
    public TimeSpan ExecutionTime { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the outcome was handled by retry or hedging strategy.
    /// </summary>
    public bool Handled { get; private set; }
}
