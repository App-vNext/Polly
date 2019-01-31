namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can be applied to synchronous delegates
    /// </summary>
    public abstract partial class Policy : PolicyBase
    {
        /// <summary>
        /// Constructs a new instance of a derived <see cref="Policy"/> type with the passed <paramref name="exceptionPredicates"/>. 
        /// </summary>
        /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
        protected Policy(ExceptionPredicates exceptionPredicates)
            : base(exceptionPredicates)
        {
        }
    }
}