using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Timeout
{
    /// <summary>
    /// A timeout policy that can be applied to asynchronous executions.
    /// </summary>
    public class AsyncTimeoutPolicy : AsyncPolicy, IAsyncTimeoutPolicy
    {
        private readonly Func<Context, TimeSpan> _timeoutProvider;
        private readonly TimeoutStrategy _timeoutStrategy;
        private readonly Func<Context, TimeSpan, Task, Exception, Task> _onTimeoutAsync;

        internal AsyncTimeoutPolicy(
            Func<Context, TimeSpan> timeoutProvider,
            TimeoutStrategy timeoutStrategy,
            Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync
            )
        {
            _timeoutProvider = timeoutProvider ?? throw new ArgumentNullException(nameof(timeoutProvider));
            _timeoutStrategy = timeoutStrategy;
            _onTimeoutAsync = onTimeoutAsync;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> AsyncGenericImplementation<TExecutableAsync, TResult>(
            TExecutableAsync action, 
            Context context, 
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            return AsyncTimeoutEngine.ImplementationAsync<TExecutableAsync, TResult>(
                action,
                context,
                cancellationToken,
                _timeoutProvider,
                _timeoutStrategy,
                _onTimeoutAsync, 
                continueOnCapturedContext);
        }
    }

    /// <summary>
    /// A timeout policy that can be applied to asynchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class AsyncTimeoutPolicy<TResult> : AsyncPolicy<TResult>, IAsyncTimeoutPolicy<TResult>
    {
        private readonly Func<Context, TimeSpan> _timeoutProvider;
        private readonly TimeoutStrategy _timeoutStrategy;
        private readonly Func<Context, TimeSpan, Task, Exception, Task> _onTimeoutAsync;

        internal AsyncTimeoutPolicy(
            Func<Context, TimeSpan> timeoutProvider,
            TimeoutStrategy timeoutStrategy,
            Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync)
        {
            _timeoutProvider = timeoutProvider ?? throw new ArgumentNullException(nameof(timeoutProvider));
            _timeoutStrategy = timeoutStrategy;
            _onTimeoutAsync = onTimeoutAsync;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> AsyncGenericImplementation<TExecutableAsync>(
            TExecutableAsync action,
            Context context, 
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
            => AsyncTimeoutEngine.ImplementationAsync<TExecutableAsync, TResult>(
                action,
                context,
                cancellationToken,
                _timeoutProvider,
                _timeoutStrategy,
                _onTimeoutAsync,
                continueOnCapturedContext);
    }
}