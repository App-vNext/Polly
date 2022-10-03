#nullable enable
using System;

namespace Polly.Bulkhead.Options;

/// <summary>
/// The <see cref="BulkheadPolicyOptionsBase"/> provides the common options for the <see cref="BulkheadPolicyOptions"/>
/// and <see cref="AsyncBulkheadPolicyOptions"/>.
/// </summary>
public abstract class BulkheadPolicyOptionsBase
{
    /// <summary>
    /// The maximum number of parallelization.
    /// </summary>
    public MaxParallelizationValue MaxParallelization { get; }

    /// <summary>
    /// The maximum number of tasks waiting inside the execution queue.
    /// </summary>
    public QueuingActionValue MaxQueuingActions { get; set; } = QueuingActionValue.Default;

    /// <summary>
    /// Creates a new <see cref="BulkheadPolicyOptions"/>.
    /// </summary>
    /// <param name="maxParallelization">The maximum number of parallel running tasks allowed by the policy.</param>
    /// <exception cref="ArgumentOutOfRangeException">maxParallelization;Value must be greater than zero.</exception>
    protected BulkheadPolicyOptionsBase(MaxParallelizationValue maxParallelization)
    {
        MaxParallelization = maxParallelization;
    }
}