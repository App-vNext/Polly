#nullable enable
namespace Polly;

/// <summary>
/// Transient exception handling policies that can be applied to asynchronous delegates.
/// </summary>
/// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
public abstract partial class AsyncPolicy<TResult> : PolicyBase<TResult>
{
    /// <summary>
    /// Constructs a new instance of a derived <see cref="AsyncPolicy{TResult}"/> type with the passed <paramref name="exceptionPredicates"/> and <paramref name="resultPredicates"/>.
    /// </summary>
    /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
    /// <param name="resultPredicates">Predicates indicating which results the policy should handle. </param>
    internal AsyncPolicy(
        ExceptionPredicates exceptionPredicates,
        ResultPredicates<TResult> resultPredicates)
        : base(exceptionPredicates, resultPredicates)
    {
    }

    /// <summary>
    /// Constructs a new instance of a derived <see cref="AsyncPolicy{TResult}"/> type with the passed <paramref name="policyBuilder"/>.
    /// </summary>
    /// <param name="policyBuilder">A <see cref="PolicyBuilder{TResult}"/> indicating which exceptions and results the policy should handle.</param>
    protected AsyncPolicy(PolicyBuilder<TResult>? policyBuilder = null)
        : base(policyBuilder)
    {
    }
}
