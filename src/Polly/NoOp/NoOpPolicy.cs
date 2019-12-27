using System;
using System.Diagnostics;
using System.Threading;

namespace Polly.NoOp
{
    /// <summary>
    /// A no-op policy that can be applied to synchronous delegate executions.  Code executed through the policy is executed as if no policy was applied.
    /// </summary>
    public class NoOpPolicy : Policy, ISyncNoOpPolicy
    {
        internal NoOpPolicy()
        {
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
            => NoOpEngine.Implementation(action, context, cancellationToken);
    }

    /// <summary>
    /// A no-op policy that can be applied to synchronous delegate executions returning a value of type <typeparamref name="TResult"/>.  Code executed through the policy is executed as if no policy was applied.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class NoOpPolicy<TResult> : Policy<TResult>, ISyncNoOpPolicy<TResult>
    {
        internal NoOpPolicy()
        {
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
            => NoOpEngine.Implementation(action, context, cancellationToken);
    }
}
