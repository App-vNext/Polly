#nullable enable
namespace Polly;

/// <summary>
/// Transient exception handling policies that can be applied to asynchronous delegates.
/// </summary>
public abstract partial class AsyncPolicy
{
    /// <summary>
    /// Constructs a new instance of a derived <see cref="AsyncPolicy"/> type with the passed <paramref name="exceptionPredicates"/>.
    /// </summary>
    /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
    internal AsyncPolicy(ExceptionPredicates exceptionPredicates)
        : base(exceptionPredicates)
    {
    }

    /// <summary>
    /// Constructs a new instance of a derived <see cref="AsyncPolicy"/> type with the passed <paramref name="policyBuilder"/>.
    /// </summary>
    /// <param name="policyBuilder">A <see cref="PolicyBuilder"/> specifying which exceptions the policy should handle. </param>
    protected AsyncPolicy(PolicyBuilder? policyBuilder = null)
        : base(policyBuilder)
    {
    }
}
