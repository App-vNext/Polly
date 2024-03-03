#nullable enable
namespace Polly;

/// <summary>
/// Transient fault handling policies that can be applied to delegates returning results of type <typeparamref name="TResult"/>.
/// </summary>
public abstract partial class Policy<TResult> : PolicyBase<TResult>
{
    /// <summary>
    /// Constructs a new instance of a derived <see cref="Policy{TResult}"/> type with the passed <paramref name="exceptionPredicates"/> and <paramref name="resultPredicates"/>.
    /// </summary>
    /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle.</param>
    /// <param name="resultPredicates">Predicates indicating which results the policy should handle.</param>
    internal Policy(ExceptionPredicates exceptionPredicates, ResultPredicates<TResult> resultPredicates)
    : base(exceptionPredicates, resultPredicates)
    {
    }

    /// <summary>
    /// Constructs a new instance of a derived <see cref="Policy{TResult}"/> type with the passed <paramref name="policyBuilder"/>.
    /// </summary>
    /// <param name="policyBuilder">A <see cref="PolicyBuilder{TResult}"/> indicating which exceptions and results the policy should handle.</param>
    protected Policy(PolicyBuilder<TResult>? policyBuilder = null)
        : base(policyBuilder)
    {
    }
}
