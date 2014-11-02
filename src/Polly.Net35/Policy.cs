using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can
    /// be applied to delegates
    /// </summary>
    public class Policy
    {
        private readonly Action<Action> _exceptionPolicy;
        private readonly Func<Func<Task>, Task> _asyncExceptionPolicy;

        internal Policy(Action<Action> exceptionPolicy, Func<Func<Task>, Task> asyncExceptionPolicy)
        {
            if (exceptionPolicy == null) throw new ArgumentNullException("exceptionPolicy");
            if (asyncExceptionPolicy == null) throw new ArgumentNullException("asyncExceptionPolicy");

            _exceptionPolicy = exceptionPolicy;
            _asyncExceptionPolicy = asyncExceptionPolicy;
        }

        /// <summary>
        /// Executes the specified action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public void Execute(Action action)
        {
            _exceptionPolicy(action);
        }

#if NET45
        /// <summary>
        /// Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action)
        {

            return _asyncExceptionPolicy(action);
        }
#endif

        /// <summary>
        /// Executes the specified action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public TResult Execute<TResult>(Func<TResult> action)
        {
            var result = default(TResult);
            _exceptionPolicy(() => { result = action(); });
            return result;
        }

#if NET45
        /// <summary>
        /// Executes the specified asynchronous action within the policy and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        /// <returns>The value returned by the action</returns>
        [DebuggerStepThrough]
        public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action)
        {
            var result = default(TResult);
            await _asyncExceptionPolicy(async () => { result = await action(); });
            return result;
        }
#endif

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
}