using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can
    /// be applied to delegates
    /// </summary>
    public partial class Policy
    {
        /// <summary>
        /// returns a list of all defined prediaces
        /// </summary>
        private readonly Action<Action> _exceptionPolicy;
        private readonly IEnumerable<ExceptionPredicate> _exceptionPredicates;

        internal Policy(Action<Action> exceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates)
        {
            if (exceptionPolicy == null) throw new ArgumentNullException("exceptionPolicy");

            _exceptionPolicy = exceptionPolicy;
            _exceptionPredicates = exceptionPredicates ?? Enumerable.Empty<ExceptionPredicate>();
        }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public void Execute(Action action)
        {
            if (_exceptionPolicy == null)
                throw new InvalidOperationException(
"Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");

            _exceptionPolicy(action);
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the Result.
        /// </summary>
        /// <typeparam name="TResult">The type of the Result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<TResult> action)
        {
            if (_exceptionPolicy == null)
                throw new InvalidOperationException(
"Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");

            var result = default(TResult);
            _exceptionPolicy(() => { result = action(); });
            return result;
        }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public HandledPolicy ExecuteAnd(Action action)
        {
            if (_exceptionPolicy == null)
                throw new InvalidOperationException(
"Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");

            try
            {
                _exceptionPolicy(action);
                return new HandledPolicy(null);
            }
            catch (Exception ex)
            {
                if (_exceptionPredicates.Any(x => x(ex)))
                    return new HandledPolicy(ex);

                throw;
            }
        }

        /// <summary>
        /// Executes the specified action within the policy and returns the Result.
        /// </summary>
        /// <typeparam name="TResult">The type of the Result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public HandledPolicy<TResult> ExecuteAnd<TResult>(Func<TResult> action)
        {
            if (_exceptionPolicy == null)
                throw new InvalidOperationException(
"Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");

            try
            {
                var result = default(TResult);
                _exceptionPolicy(() => { result = action(); });
                return new HandledPolicy<TResult>(result);
            }
            catch (Exception ex)
            {
                if (_exceptionPredicates.Any(x => x(ex)))
                    return new HandledPolicy<TResult>(ex);

                throw;
            }
        }

        /// <summary>
        /// Specifies the type of exception that this policy can handle.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder Handle<TException>() where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException;

            return new PolicyBuilder(predicate);
        }

        /// <summary>
        /// Specifies the type of exception that this policy can handle with addition filters on this exception type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyBuilder Handle<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
        {
            ExceptionPredicate predicate = exception => exception is TException &&
                                                        exceptionPredicate((TException)exception);

            return new PolicyBuilder(predicate);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HandledPolicy
    {
        /// <summary>
        /// 
        /// </summary>
        public Exception InnerException { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool HasException
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return InnerException != null; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="innerException"></param>
        public HandledPolicy(Exception innerException)
        {
            InnerException = innerException;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class HandledPolicy<TResult>
    {
        /// <summary>
        /// 
        /// </summary>
        public TResult Result { get; }

        /// <summary>
        /// 
        /// </summary>
        public Exception InnerException { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool HasException
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return InnerException != null; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public HandledPolicy(TResult result)
        {
            this.Result = result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="innerException"></param>
        public HandledPolicy(Exception innerException)
        {
            InnerException = innerException;
        }
    }
}