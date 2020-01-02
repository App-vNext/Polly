namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can be applied to synchronous delegates
    /// </summary>
    public abstract partial class PolicyV8 : PolicyBase
    {
        /// <summary>
        /// Constructs a new instance of a derived <see cref="PolicyV8"/> type with the passed <paramref name="exceptionPredicates"/>. 
        /// </summary>
        /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
        internal PolicyV8(ExceptionPredicates exceptionPredicates)
            : base(exceptionPredicates)
        {
        }

        /// <summary>
        /// Constructs a new instance of a derived <see cref="PolicyV8"/> type with the passed <paramref name="policyBuilder"/>. 
        /// </summary>
        /// <param name="policyBuilder">A <see cref="PolicyBuilder"/> specifying which exceptions the policy should handle. </param>
        protected PolicyV8(PolicyBuilder policyBuilder = null)
            : base(policyBuilder)
        {
        }
    }
}