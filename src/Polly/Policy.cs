#nullable enable
namespace Polly;

/// <summary>
/// Transient exception handling policies that can be applied to synchronous delegates.
/// </summary>
public abstract partial class Policy : PolicyBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Policy"/> class.
    /// </summary>
    /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
    internal Policy(ExceptionPredicates exceptionPredicates)
        : base(exceptionPredicates)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Policy"/> class.
    /// </summary>
    /// <param name="policyBuilder">A <see cref="PolicyBuilder"/> specifying which exceptions the policy should handle. </param>
    protected Policy(PolicyBuilder? policyBuilder = null)
        : base(policyBuilder)
    {
    }
}
