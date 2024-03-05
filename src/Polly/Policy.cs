#nullable enable
namespace Polly;

/// <summary>
/// Transient exception handling policies that can be applied to synchronous delegates.
/// </summary>
public abstract partial class Policy : PolicyBase
{
    /// <summary>
    /// Constructs a new instance of a derived <see cref="Policy"/> type with the passed <paramref name="exceptionPredicates"/>.
    /// </summary>
    /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
    internal Policy(ExceptionPredicates exceptionPredicates)
        : base(exceptionPredicates)
    {
    }

    /// <summary>
    /// Constructs a new instance of a derived <see cref="Policy"/> type with the passed <paramref name="policyBuilder"/>.
    /// </summary>
    /// <param name="policyBuilder">A <see cref="PolicyBuilder"/> specifying which exceptions the policy should handle. </param>
    protected Policy(PolicyBuilder? policyBuilder = null)
        : base(policyBuilder)
    {
    }
}
