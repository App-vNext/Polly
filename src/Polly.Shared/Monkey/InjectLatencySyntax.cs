using System;
using System.Threading;
using Polly.Monkey;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining a Latency Injection <see cref="Policy"/>. 
    /// </summary>
    public partial class Policy
    {
        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">The latency to inject</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectLatency(
            TimeSpan latency,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Action latencyProvider = () => { SystemClock.Sleep(latency, CancellationToken.None); };
            return Policy.Monkey(latencyProvider, injectionRate, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">The latency to inject</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectLatency(
            TimeSpan latency,
            Double injectionRate,
            Func<Context, bool> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Action<Context> latencyProvider = _ => { SystemClock.Sleep(latency, CancellationToken.None); };
            return Policy.Monkey(latencyProvider, injectionRate, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectLatency(
            TimeSpan latency,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Action<Context> latencyProvider = _ => { SystemClock.Sleep(latency, CancellationToken.None); };
            return Policy.Monkey(latencyProvider, injectionRate, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectLatency(
            Func<Context, TimeSpan> latency,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));
            
            Action<Context> latencyProvider = ctx => { SystemClock.Sleep(latency(ctx), CancellationToken.None); };
            return Policy.Monkey(latencyProvider, injectionRate, enabled);
        }
    }

    /// <summary>
    /// Fluent API for defining a Latency Injection <see cref="Policy"/>. 
    /// </summary>
    public partial class Policy
    {
        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectLatency<TResult>(
            TimeSpan latency,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Action<Context> latencyProvider = _ => { SystemClock.Sleep(latency, CancellationToken.None); };
            Func<Context, Double> injectionRateLambda = _ => injectionRate;
            Func<Context, bool> enabledLambda = _ =>
            {
                return enabled();
            };

            return Policy.Monkey<TResult>(latencyProvider, injectionRateLambda, enabledLambda);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectLatency<TResult>(
            TimeSpan latency,
            Double injectionRate,
            Func<Context, bool> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Action<Context> latencyProvider = _ => { SystemClock.Sleep(latency, CancellationToken.None); };
            Func<Context, Double> injectionRateLambda = _ => injectionRate;

            return Policy.Monkey<TResult>(latencyProvider, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectLatency<TResult>(
            TimeSpan latency,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Action<Context> latencyProvider = _ => { SystemClock.Sleep(latency, CancellationToken.None); };
            return Policy.Monkey<TResult>(latencyProvider, injectionRate, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectLatency<TResult>(
            Func<Context, TimeSpan> latency,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Action<Context> latencyProvider = ctx => { SystemClock.Sleep(latency(ctx), CancellationToken.None); };
            return Policy.Monkey<TResult>(latencyProvider, injectionRate, enabled);
        }
    }
}
