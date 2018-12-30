namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can be applied to asynchronous delegates
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public abstract partial class AsyncPolicy<TResult>
    {
        internal ResultPredicates<TResult> ResultPredicates { get; }

        /// <summary>
        /// Constructs a new instance of a derived <see cref="AsyncPolicy{TResult}"/> type with the passed <paramref name="exceptionPredicates"/> and <paramref name="resultPredicates"/>. 
        /// </summary>
        /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
        /// <param name="resultPredicates">Predicates indicating which results the policy should handle. </param>
        protected AsyncPolicy(
            ExceptionPredicates exceptionPredicates,
            ResultPredicates<TResult> resultPredicates)
        {
            ExceptionPredicates = exceptionPredicates ?? ExceptionPredicates.None;
            ResultPredicates = resultPredicates ?? ResultPredicates<TResult>.None;
        }
    }
}
