#nullable enable
namespace Polly.Bulkhead.Options;

/// <summary>
/// The <see cref="AsyncBulkheadPolicyOptions"/> defines the options used for the <see cref="AsyncBulkheadPolicy"/>.
/// </summary>
public class AsyncBulkheadPolicyOptions : BulkheadPolicyOptionsBase
{
    /// <summary>
    /// The callback object which handles the callback.
    /// </summary>
    /// <remarks>null means default</remarks>
    public AsyncBulkheadRejectionHandlerBase? BulkheadRejectionHandler { get; set; }

    /// <summary>
    /// Creates a new <see cref="BulkheadPolicyOptions"/> instance.
    /// </summary>
    /// <param name="maxParallelization">The maximum parallelization</param>
    public AsyncBulkheadPolicyOptions(MaxParallelizationValue maxParallelization) : base(maxParallelization) { }
}