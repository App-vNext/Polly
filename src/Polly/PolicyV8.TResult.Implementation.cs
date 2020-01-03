using System.Threading;

namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can be applied to synchronous delegates
    /// </summary>
    public abstract partial class PolicyV8<TResult>
    {
        /// <summary>
        /// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="action">The action passed by calling code to execute through the policy.</param>
        /// <param name="context">The policy execution context.</param>
        /// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
        /// <returns>A <typeparamref name="TResult"/> result of the execution.</returns>
        protected abstract TResult ImplementationSyncV8<TExecutable>(
            in TExecutable action,
            Context context,
            CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>;
    }
}