using System.Threading.Tasks;

namespace Polly.Bulkhead.Options;

/// <summary>
/// The <see cref="AsyncBulkheadRejectionHandlerBase"/> exposes the callback for the <see cref="AsyncBulkheadPolicy"/>.
/// </summary>
public abstract class AsyncBulkheadRejectionHandlerBase
{
    /// <summary>
    /// The callback function which is used when a bulkhead gets rejected.
    /// </summary>
    /// <param name="context">The object describing the context in which the bulkhead was rejected</param>
    public abstract Task OnBulkheadRejected(Context context);
}