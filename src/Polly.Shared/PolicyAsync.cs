#if SUPPORTS_ASYNC

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Polly
{
    public partial class Policy
    {
        private readonly Func<Func<CancellationToken, Task>, CancellationToken, bool, Task> _asyncExceptionPolicy;

        internal Policy(Func<Func<CancellationToken, Task>, CancellationToken, bool, Task> asyncExceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates)
        {
            if (asyncExceptionPolicy == null) throw new ArgumentNullException("asyncExceptionPolicy");

            _asyncExceptionPolicy = asyncExceptionPolicy;
            _exceptionPredicates = exceptionPredicates ?? Enumerable.Empty<ExceptionPredicate>();
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action)
        {
            return ExecuteAsync(action, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action, bool continueOnCapturedContext)
        {
            return ExecuteAsync(ct => action(), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            return ExecuteAsync(action, cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.InvalidOperationException">Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.</exception>
        [DebuggerStepThrough]
        public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException
                ("Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.");

            await _asyncExceptionPolicy(action, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action)
        {
            return ExecuteAndCaptureAsync(action, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync(ct => action(), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        [DebuggerStepThrough]
        public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync(action, cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <exception cref="System.InvalidOperationException">Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.</exception>
        [DebuggerStepThrough]
        public async Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException
                ("Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.");

            try
            {
                await _asyncExceptionPolicy(action, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
                return PolicyResult.Successful();
            }
            catch (Exception exception)
            {
                return PolicyResult.Failure(exception, GetExceptionType(_exceptionPredicates, exception));
            }
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action)
        {
            return ExecuteAsync(action, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            return ExecuteAsync(action, cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action, bool continueOnCapturedContext)
        {
            return ExecuteAsync(ct => action(), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
        /// <returns>The value returned by the action</returns>
        /// <exception cref="System.InvalidOperationException">Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.</exception>
        [DebuggerStepThrough]
        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException(
                "Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.");

            var result = default(TResult);
            await _asyncExceptionPolicy(async ct =>
            {
                result = await action(ct).ConfigureAwait(continueOnCapturedContext);
            }, cancellationToken, continueOnCapturedContext)
            .ConfigureAwait(continueOnCapturedContext);
            return result;
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>
        /// The value returned by the action
        /// </returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action)
        {
            return ExecuteAndCaptureAsync(action, false);
        }

        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <returns>
        /// The value returned by the action
        /// </returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action, bool continueOnCapturedContext)
        {
            return ExecuteAndCaptureAsync(ct => action(), CancellationToken.None, continueOnCapturedContext);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken)
        {
            return ExecuteAndCaptureAsync(ct => action(), cancellationToken, false);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
        /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
        /// <returns>The value returned by the action</returns>
        /// <exception cref="System.InvalidOperationException">Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.</exception>
        [DebuggerStepThrough]
        public async Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException(
                "Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.");

            try
            {
                var result = default(TResult);
                await _asyncExceptionPolicy(async ct =>
                {
                    result = await action(ct).ConfigureAwait(continueOnCapturedContext);
                }, cancellationToken, continueOnCapturedContext)
                .ConfigureAwait(continueOnCapturedContext);

                return PolicyResult<TResult>.Successful(result);
            }
            catch (Exception exception)
            {
                return PolicyResult<TResult>.Failure(exception, GetExceptionType(_exceptionPredicates, exception));
            }
        }
    }
}

#endif