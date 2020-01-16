﻿using System.Threading;

namespace Polly
{
    public abstract partial class Policy
    {
        /// <summary>
        /// Defines the implementation of a policy for sync executions with no return value.
        /// </summary>
        /// <param name="action">The action passed by calling code to execute through the policy.</param>
        /// <param name="context">The policy execution context.</param>
        /// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
        protected virtual void SyncNonGenericImplementation<TExecutable>(
            in TExecutable action,
            Context context,
            CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable
        {
            SyncGenericImplementation<TExecutable, object>(action, context, cancellationToken);
        }

        /// <summary>
        /// Defines the implementation of a policy for sync executions returning <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="action">The action passed by calling code to execute through the policy.</param>
        /// <param name="context">The policy execution context.</param>
        /// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
        /// <returns>A <typeparamref name="TResult"/> result of the execution.</returns>
        protected abstract TResult SyncGenericImplementation<TExecutable, TResult>(
            in TExecutable action,
            Context context,
            CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>;
    }
}
