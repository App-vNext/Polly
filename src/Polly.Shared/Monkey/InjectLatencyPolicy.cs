using System;
using System.Threading;
using Polly.Utilities;

namespace Polly.Monkey
{
    /// <summary>
    /// A policy that injects latency before the execution of delegates.
    /// </summary>
    public class InjectLatencyPolicy : MonkeyPolicy
    {
        internal static readonly CancellationToken DefaultCancellationForInjectedLatency = CancellationToken.None; // It is intended that injected latency is not susceptible to cancellation. (TO CONFIRM)

        private readonly Func<Context, TimeSpan> _latencyProvider;

        internal InjectLatencyPolicy(
            Func<Context, TimeSpan> latencyProvider,
            Func<Context, double> injectionRate, 
            Func<Context, bool> enabled) 
            : base(injectionRate, enabled)
        {
            _latencyProvider = latencyProvider ?? throw new ArgumentNullException(nameof(latencyProvider));
        }

        /// <inheritdoc/>
        protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return MonkeyEngine.InjectBehaviourImplementation(
                action,
                context,
                cancellationToken,
                (ctx, ct) => SystemClock.Sleep(_latencyProvider(ctx), DefaultCancellationForInjectedLatency),
                InjectionRate,
                Enabled);
        }
    }

    /// <summary>
    /// A policy that injects latency before the execution of delegates.
    /// </summary>
    /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
    public class InjectLatencyPolicy<TResult> : MonkeyPolicy<TResult>
    {
        private readonly Func<Context, TimeSpan> _latencyProvider;

        internal InjectLatencyPolicy(
            Func<Context, TimeSpan> latencyProvider,
            Func<Context, double> injectionRate,
            Func<Context, bool> enabled)
            : base(injectionRate, enabled)
        {
            _latencyProvider = latencyProvider ?? throw new ArgumentNullException(nameof(latencyProvider));
        }

        /// <inheritdoc/>
        protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return MonkeyEngine.InjectBehaviourImplementation(
                action,
                context,
                cancellationToken,
                (ctx, ct) => SystemClock.Sleep(_latencyProvider(ctx), InjectLatencyPolicy.DefaultCancellationForInjectedLatency),
                InjectionRate,
                Enabled);
        }

    }
}
