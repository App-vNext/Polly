using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Timeout
{
    /// <summary>
    /// A timeout policy that can be applied to synchronous executions.
    /// </summary>
    public class TimeoutPolicy : PolicyV8, ISyncTimeoutPolicy
    {
        private Func<Context, TimeSpan> _timeoutProvider;
        private TimeoutStrategy _timeoutStrategy;
        private Action<Context, TimeSpan, Task, Exception> _onTimeout;

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
        protected override TResult ImplementationSyncV8<TExecutable, TResult>(in TExecutable action, Context context,
            CancellationToken cancellationToken)
            => TimeoutEngineV8.Implementation<TExecutable, TResult>(
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
    public class TimeoutPolicy<TResult> : PolicyV8<TResult>, ISyncTimeoutPolicy<TResult>
    {
        private Func<Context, TimeSpan> _timeoutProvider;
        private TimeoutStrategy _timeoutStrategy;
        private Action<Context, TimeSpan, Task, Exception> _onTimeout;

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
        protected override TResult ImplementationSyncV8<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
            => TimeoutEngineV8.Implementation<TExecutable, TResult>(
                action,
                context,
                cancellationToken,
                _timeoutProvider,
                _timeoutStrategy,
                _onTimeout);
    }
}