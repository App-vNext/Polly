using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.NoOp
{
    /// <summary>
    /// A no-op policy that can be applied to asynchronous executions.  Code executed through the policy is executed as if no policy was applied.
    /// </summary>
    public class AsyncNoOpPolicy : AsyncPolicyV8, IAsyncNoOpPolicy
    {
        internal AsyncNoOpPolicy() 
        {
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> ImplementationAsyncV8<TExecutableAsync, TResult>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
            => AsyncNoOpEngineV8.ExecuteAsync<TExecutableAsync, TResult>(action, context, cancellationToken, continueOnCapturedContext);
    }

    /// <summary>
    /// A no-op policy that can be applied to asynchronous executions returning a value of type <typeparamref name="TResult"/>.  Code executed through the policy is executed as if no policy was applied.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class AsyncNoOpPolicy<TResult> : AsyncPolicyV8<TResult>, IAsyncNoOpPolicy<TResult>
    {
        internal AsyncNoOpPolicy() 
        {
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> ImplementationAsyncV8<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
            => AsyncNoOpEngineV8.ExecuteAsync<TExecutableAsync, TResult>(action, context, cancellationToken, continueOnCapturedContext);
    }
}