#nullable enable

namespace Polly.Bulkhead;

/// <summary>
/// Exception thrown when a bulkhead's semaphore and queue are full.
/// </summary>
public class BulkheadRejectedException : ExecutionRejectedException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BulkheadRejectedException" /> class.
    /// </summary>
    public BulkheadRejectedException() : this("The bulkhead semaphore and queue are full and execution was rejected.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkheadRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public BulkheadRejectedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkheadRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BulkheadRejectedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
