﻿namespace Polly.Timeout;

/// <summary>
/// Exception thrown when a delegate executed through a <see cref="TimeoutPolicy"/> does not complete, before the configured timeout.
/// </summary>
public class TimeoutRejectedException : ExecutionRejectedException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutRejectedException" /> class.
    /// </summary>
    public TimeoutRejectedException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public TimeoutRejectedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TimeoutRejectedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
