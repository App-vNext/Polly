#if SUPPORTS_ASYNC

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Polly.Extensions;

namespace Polly
{
    public partial class Policy
    {
        private readonly Func<Func<Task>, Task> _asyncExceptionPolicy;

        internal Policy(Func<Func<Task>, Task> asyncExceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates)
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
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException
                ("Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.");

            return _asyncExceptionPolicy(action);
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the captured result.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public async Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action)
        {
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException
                ("Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.");

            try
            {
                await _asyncExceptionPolicy(action).NotOnCapturedContext();
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
        public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action)
        {
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException(
                "Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.");

            var result = default(TResult);
            await _asyncExceptionPolicy(async () => { result = await action().NotOnCapturedContext(); }).NotOnCapturedContext();
            return result;
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public async Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action)
        {
            if (_asyncExceptionPolicy == null) throw new InvalidOperationException(
                "Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.");

            try
            {
                var result = default(TResult);
                await _asyncExceptionPolicy(async () => { result = await action().NotOnCapturedContext(); }).NotOnCapturedContext();
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