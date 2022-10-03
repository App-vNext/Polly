namespace Polly.Bulkhead.Options;

/// <summary>
/// The <see cref="BulkheadRejectionHandlerBase"/> exposes the callback for the <see cref="BulkheadPolicy"/>.
/// </summary>
public abstract class BulkheadRejectionHandlerBase
{
    /// <summary>
    /// The callback function which is used when a bulkhead gets rejected.
    /// </summary>
    /// <param name="context">The object describing the context in which the bulkhead was rejected</param>
    public abstract void OnBulkheadRejected(Context context);
}