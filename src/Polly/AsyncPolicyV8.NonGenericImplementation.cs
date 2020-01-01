using System.Threading;
using System.Threading.Tasks;

namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can be applied to asynchronous delegates
    /// </summary>
    public abstract partial class AsyncPolicyV8 : AsyncPolicy
    {
/*
        /// <summary>
        /// Defines the implementation of a policy for async executions with no return value.
        /// </summary>
        /// <param name="action">The action passed by calling code to execute through the policy.</param>
        /// <param name="context">The policy execution context.</param>
        /// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
        /// <param name="continueOnCapturedContext">Whether async continuations should continue on a captured context.</param>
        /// <returns>A <see cref="Task"/> representing the result of the execution.</returns>
        protected virtual Task ImplementationAsync(
            Func<Context, CancellationToken, Task> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
            => ImplementationAsync<EmptyStruct>(async (ctx, token) =>
            {
                await action(ctx, token).ConfigureAwait(continueOnCapturedContext);
                return EmptyStruct.Instance;
            }, context, cancellationToken, continueOnCapturedContext);

        /// <summary>
        /// Defines the implementation of a policy for async executions with no return value.
        /// </summary>
        /// <param name="action">The action passed by calling code to execute through the policy.</param>
        /// <param name="context">The policy execution context.</param>
        /// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
        /// <param name="continueOnCapturedContext">Whether async continuations should continue on a captured context.</param>
        /// <returns>A <see cref="Task"/> representing the result of the execution.</returns>
        protected abstract Task ImplementationAsyncV8<TExecutableAsync, EmptyStruct>(
            TExecutableAsync action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<EmptyStruct>;
*/

        /// <summary>
        /// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="action">The action passed by calling code to execute through the policy.</param>
        /// <param name="context">The policy execution context.</param>
        /// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
        /// <param name="continueOnCapturedContext">Whether async continuations should continue on a captured context.</param>
        /// <returns>A <see cref="Task"/> representing the result of the execution.</returns>
        protected abstract Task<TResult> ImplementationAsyncV8<TExecutableAsync, TResult>(
            TExecutableAsync action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<TResult>;
    }
}
