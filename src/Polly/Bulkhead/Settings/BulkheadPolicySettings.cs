namespace Polly.Bulkhead.Settings;

/// <summary>
/// The <see cref="BulkheadPolicySettings"/> defines the settings used for the <see cref="BulkheadPolicy"/>.
/// </summary>
public class BulkheadPolicySettings
{
    /// <summary>
    /// The maximum number of parallelization.
    /// </summary>
    /// <remarks>null means default</remarks>
    public int MaxParallelization { get; set; }
    
    /// <summary>
    /// The maximum number of actions inside the queue.
    /// </summary>
    /// <remarks>null means default</remarks>
    public int? MaxQueuingActions { get; set; }
    
    /// <summary>
    /// The callback object which handles the callback.
    /// </summary>
    /// <remarks>null means default</remarks>
    public IBulkheadRejectedCallback OnBulkheadRejectedCallback { get; set; }

    public BulkheadPolicySettings(int maxParallelization)
    {
        MaxParallelization = maxParallelization;
    }
}