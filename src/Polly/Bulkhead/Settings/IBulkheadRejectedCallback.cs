namespace Polly.Bulkhead.Settings;

/// <summary>
/// The <see cref="IBulkheadRejectedCallback"/> exposes the callback for the <see cref="BulkheadPolicy"/>.
/// </summary>
public interface IBulkheadRejectedCallback
{
    /// <summary>
    /// The callback function which is used when a bulkhead gets rejected.
    /// </summary>
    /// <param name="context">The object describing the context in which the bulkhead was rejected</param>
    void OnBulkheadRejected(Context context);
}