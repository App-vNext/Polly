using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Monkey
{
    /// <summary>
    /// A policy that injects latency before the execution of delegates.
    /// </summary>
    public class AsyncInjectLatencyPolicy : AsyncMonkeyPolicy
    {
        private readonly Func<Context, CancellationToken, Task<TimeSpan>> _latencyProvider;

        internal AsyncInjectLatencyPolicy(
            Func<Context, CancellationToken, Task<TimeSpan>> latencyProvider,
            Func<Context, Task<Double>> injectionRate, 
            Func<Context, Task<bool>> enabled)
            : base(injectionRate, enabled)
        {
            _latencyProvider = latencyProvider ?? throw new ArgumentNullException(nameof(latencyProvider));
        }
        
        /// <inheritdoc/>
        protected override Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action, 
            Context context, 
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            return AsyncMonkeyEngine.InjectBehaviourImplementationAsync(
                action,
                context,
                cancellationToken,
                async (ctx, ct) => await SystemClock.SleepAsync(
                        await _latencyProvider(ctx, cancellationToken).ConfigureAwait(continueOnCapturedContext),
                        InjectLatencyPolicy.DefaultCancellationForInjectedLatency)
                    .ConfigureAwait(continueOnCapturedContext),
                InjectionRate,
                Enabled,
                continueOnCapturedContext);
        }
    }

    /// <summary>
    /// A policy that injects latency before the execution of delegates.
    /// </summary>
    public class AsyncInjectLatencyPolicy<TResult> : AsyncMonkeyPolicy<TResult>
    {
        private readonly Func<Context, CancellationToken, Task<TimeSpan>> _latencyProvider;

        internal AsyncInjectLatencyPolicy(
            Func<Context, CancellationToken, Task<TimeSpan>> latencyProvider,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
            : base(injectionRate, enabled)
        {
            _latencyProvider = latencyProvider ?? throw new ArgumentNullException(nameof(latencyProvider));
        }

        /// <inheritdoc/>
        protected override Task<TResult> ImplementationAsync(
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            return AsyncMonkeyEngine.InjectBehaviourImplementationAsync(
                action,
                context,
                cancellationToken,
                async (ctx, ct) => await SystemClock.SleepAsync(
                    await _latencyProvider(ctx, cancellationToken).ConfigureAwait(continueOnCapturedContext),
                        InjectLatencyPolicy.DefaultCancellationForInjectedLatency)
                    .ConfigureAwait(continueOnCapturedContext),
                InjectionRate,
                Enabled,
                continueOnCapturedContext);
        }
    }
}
