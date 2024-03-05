#nullable enable
namespace Polly;

/// <summary>
/// Transient fault handling policies that can be applied to delegates returning results of type <typeparamref name="TResult"/>.
/// </summary>
public abstract partial class Policy<TResult> : PolicyBase<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Policy{TResult}"/> class.
    /// </summary>
    /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle.</param>
    /// <param name="resultPredicates">Predicates indicating which results the policy should handle.</param>
    internal Policy(ExceptionPredicates exceptionPredicates, ResultPredicates<TResult> resultPredicates)
    : base(exceptionPredicates, resultPredicates)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Policy{TResult}"/> class.
    /// </summary>
    /// <param name="policyBuilder">A <see cref="PolicyBuilder{TResult}"/> indicating which exceptions and results the policy should handle.</param>
    protected Policy(PolicyBuilder<TResult>? policyBuilder = null)
        : base(policyBuilder)
    {
    }
}
