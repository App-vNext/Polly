using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Timeout
{
    /// <summary>
    /// A timeout policy that can be applied to synchronous executions.
    /// </summary>
    public class TimeoutPolicy : Policy, ISyncTimeoutPolicy
    {
        private readonly Func<Context, TimeSpan> _timeoutProvider;
        private readonly TimeoutStrategy _timeoutStrategy;
        private readonly Action<Context, TimeSpan, Task, Exception> _onTimeout;

        internal TimeoutPolicy(
            Func<Context, TimeSpan> timeoutProvider,
            TimeoutStrategy timeoutStrategy,
            Action<Context, TimeSpan, Task, Exception> onTimeout) 
        {
            _timeoutProvider = timeoutProvider ?? throw new ArgumentNullException(nameof(timeoutProvider));
            _timeoutStrategy = timeoutStrategy;
            _onTimeout = onTimeout;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult SyncGenericImplementation<TExecutable, TResult>(in TExecutable action, Context context,
            CancellationToken cancellationToken)
            => TimeoutEngine.Implementation<TExecutable, TResult>(
                action,
                context,
                cancellationToken,
                _timeoutProvider,
                _timeoutStrategy,
                _onTimeout);
    }

    /// <summary>
    /// A timeout policy that can be applied to synchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class TimeoutPolicy<TResult> : Policy<TResult>, ISyncTimeoutPolicy<TResult>
    {
        private readonly Func<Context, TimeSpan> _timeoutProvider;
        private readonly TimeoutStrategy _timeoutStrategy;
        private readonly Action<Context, TimeSpan, Task, Exception> _onTimeout;

        internal TimeoutPolicy(
            Func<Context, TimeSpan> timeoutProvider,
            TimeoutStrategy timeoutStrategy,
            Action<Context, TimeSpan, Task, Exception> onTimeout)
        {
            _timeoutProvider = timeoutProvider ?? throw new ArgumentNullException(nameof(timeoutProvider));
            _timeoutStrategy = timeoutStrategy;
            _onTimeout = onTimeout;
        }

        /// <inheritdoc/>
        protected override TResult SyncGenericImplementation<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
            => TimeoutEngine.Implementation<TExecutable, TResult>(
                action,
                context,
                cancellationToken,
                _timeoutProvider,
                _timeoutStrategy,
                _onTimeout);
    }
}