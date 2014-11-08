using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Polly
{
    public partial class Policy
    {
        private readonly Func<Func<Task>, Task> _asyncExceptionPolicy;

        internal Policy(Action<Action> exceptionPolicy, Func<Func<Task>, Task> asyncExceptionPolicy)
            : this(exceptionPolicy)
        {
            if (asyncExceptionPolicy == null) throw new ArgumentNullException("asyncExceptionPolicy");

            _asyncExceptionPolicy = asyncExceptionPolicy;
        }

        /// <summary>
        ///     Executes the specified asynchronous action within the policy.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        [DebuggerStepThrough]
        public Task ExecuteAsync(Func<Task> action)
        {
            return _asyncExceptionPolicy(action);
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
            TResult result = default(TResult);
            await _asyncExceptionPolicy(async () => { result = await action(); });
            return result;
        }
    }
}