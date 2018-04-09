using System.Collections.Generic;
using System.Threading;

namespace Polly
{
    /// <summary>
    /// Implements elements common to both non-generic <see cref="Policy"/> and generic <see cref="Policy{TResult}"/>
    /// </summary>
    public abstract partial class PolicyBase
    {
        /// <summary>
        /// Predicates indicating which exceptions the policy should handle.
        /// </summary>
        protected internal IEnumerable<ExceptionPredicate> ExceptionPredicates { get; protected set; }

        /// <summary>
        /// Defines a CancellationToken to use, when none is supplied.
        /// </summary>
        protected static internal CancellationToken DefaultCancellationToken = CancellationToken.None;

        /// <summary>
        /// Defines a value to use for continueOnCaptureContext, when none is supplied.
        /// </summary>
        protected static internal bool DefaultContinueOnCapturedContext = false;
    }
}
