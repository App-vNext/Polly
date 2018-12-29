namespace Polly
{
    /// <summary>
    /// Transient fault handling policies that can be applied to delegates returning results of type <typeparamref name="TResult"/>
    /// </summary>
    public abstract partial class Policy<TResult> : PolicyBase
    {
        internal ResultPredicates<TResult> ResultPredicates { get; }

        /// <summary>
        /// Constructs a new instance of a derived <see cref="Policy{TResult}"/> type with the passed <paramref name="exceptionPredicates"/> and <paramref name="resultPredicates"/>.
        /// </summary>
        /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle.</param>
        /// <param name="resultPredicates">Predicates indicating which results the policy should handle.</param>
        protected Policy(ExceptionPredicates exceptionPredicates, ResultPredicates<TResult> resultPredicates)
        {
            ExceptionPredicates = exceptionPredicates ?? ExceptionPredicates.None;
            ResultPredicates = resultPredicates ?? ResultPredicates<TResult>.None;
        }
    }

}
