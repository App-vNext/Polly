#nullable enable
namespace Polly.Bulkhead.Options;

/// <summary>
/// The <see cref="BulkheadPolicyOptions"/> defines the options used for the <see cref="BulkheadPolicy"/>.
/// </summary>
public class BulkheadPolicyOptions : BulkheadPolicyOptionsBase
{
    /// <summary>
    /// The callback object which handles the callback.
    /// </summary>
    /// <remarks>null means default</remarks>
    public BulkheadRejectionHandlerBase? BulkheadRejectedHandler { get; set; }

    /// <summary>
    /// Creates a new <see cref="BulkheadPolicyOptions"/> instance.
    /// </summary>
    /// <param name="maxParallelization">The maximum parallelization</param>
    public BulkheadPolicyOptions(MaxParallelizationValue maxParallelization) : base(maxParallelization) { }
}