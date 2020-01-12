using System.Diagnostics;
using System.Threading;

namespace Polly.NoOp
{
    /// <summary>
    /// A no-op policy that can be applied to synchronous executions.  Code executed through the policy is executed as if no policy was applied.
    /// </summary>
    public class NoOpPolicy : Policy, ISyncNoOpPolicy
    {
        internal NoOpPolicy()
        {
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult SyncGenericImplementation<TExecutable, TResult>(in TExecutable action, Context context, CancellationToken cancellationToken)
        {
            return NoOpEngine.Implementation<TExecutable, TResult>(action, context, cancellationToken);
        }
    }

    /// <summary>
    /// A no-op policy that can be applied to synchronous executions returning a value of type <typeparamref name="TResult"/>.  Code executed through the policy is executed as if no policy was applied.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class NoOpPolicy<TResult> : Policy<TResult>, ISyncNoOpPolicy<TResult>
    {
        internal NoOpPolicy()
        {
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult SyncGenericImplementation<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
        {
            return NoOpEngine.Implementation<TExecutable, TResult>(action, context, cancellationToken);
        }
    }
}
