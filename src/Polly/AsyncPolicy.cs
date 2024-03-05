#nullable enable
namespace Polly;

/// <summary>
/// Transient exception handling policies that can be applied to asynchronous delegates.
/// </summary>
public abstract partial class AsyncPolicy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPolicy"/> class.
    /// </summary>
    /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
    internal AsyncPolicy(ExceptionPredicates exceptionPredicates)
        : base(exceptionPredicates)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncPolicy"/> class.
    /// </summary>
    /// <param name="policyBuilder">A <see cref="PolicyBuilder"/> specifying which exceptions the policy should handle. </param>
    protected AsyncPolicy(PolicyBuilder? policyBuilder = null)
        : base(policyBuilder)
    {
    }
}
