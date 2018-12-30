using System;
using System.Threading;
using Polly.Utilities;

namespace Polly.Monkey
{
    public partial class MonkeyPolicy
    {
        /// <summary>
        /// Builds an <see cref="InjectLatencyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">The latency to inject</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static InjectLatencyPolicy InjectLatency(
            TimeSpan latency,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new InjectLatencyPolicy(_ => latency, _ => injectionRate, _ => enabled());
        }

        /// <summary>
        /// Builds an <see cref="InjectLatencyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">The latency to inject</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static InjectLatencyPolicy InjectLatency(
            TimeSpan latency,
            Double injectionRate,
            Func<Context, bool> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new InjectLatencyPolicy(_ => latency, _ => injectionRate, enabled);
        }

        /// <summary>
        /// Builds an <see cref="InjectLatencyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static InjectLatencyPolicy InjectLatency(
            TimeSpan latency,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new InjectLatencyPolicy(_ => latency, injectionRate, enabled);
        }

        /// <summary>
        /// Builds an <see cref="InjectLatencyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static InjectLatencyPolicy InjectLatency(
            Func<Context, TimeSpan> latency,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));
            
            return new InjectLatencyPolicy(latency, injectionRate, enabled);
        }
    }

    public partial class MonkeyPolicy
    {
        /// <summary>
        /// Builds an <see cref="InjectLatencyPolicy{TResult}"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static InjectLatencyPolicy<TResult> InjectLatency<TResult>(
            TimeSpan latency,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Action<Context> latencyProvider = _ => { SystemClock.Sleep(latency, CancellationToken.None); };

            return new InjectLatencyPolicy<TResult>(_ => latency, _ => injectionRate, _ => enabled());
        }

        /// <summary>
        /// Builds an <see cref="InjectLatencyPolicy{TResult}"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static InjectLatencyPolicy<TResult> InjectLatency<TResult>(
            TimeSpan latency,
            Double injectionRate,
            Func<Context, bool> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new InjectLatencyPolicy<TResult>(_ => latency, _ => injectionRate, enabled);
        }

        /// <summary>
        /// Builds an <see cref="InjectLatencyPolicy{TResult}"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static InjectLatencyPolicy<TResult> InjectLatency<TResult>(
            TimeSpan latency,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new InjectLatencyPolicy<TResult>(_ => latency, injectionRate, enabled);
        }

        /// <summary>
        /// Builds an <see cref="InjectLatencyPolicy{TResult}"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static InjectLatencyPolicy<TResult> InjectLatency<TResult>(
            Func<Context, TimeSpan> latency,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new InjectLatencyPolicy<TResult>(latency, injectionRate, enabled);
        }
    }
}
