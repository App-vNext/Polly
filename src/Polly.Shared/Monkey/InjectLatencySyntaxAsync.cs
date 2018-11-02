using System;
using System.Threading;
using System.Threading.Tasks;
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
        public static MonkeyPolicy InjectLatencyAsync(
            TimeSpan latency,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task> latencyProvider = (_, ct) => SystemClock.SleepAsync(latency, ct);
            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult(injectionRate);
            Func<Context, Task<bool>> enabledLambda = _ => Task.FromResult(enabled());

            return Policy.MonkeyAsync(latencyProvider, injectionRateLambda, enabledLambda);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">The latency to inject</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectLatencyAsync(
            TimeSpan latency,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task> latencyProvider = (_, ct) => SystemClock.SleepAsync(latency, ct);
            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult(injectionRate);

            return Policy.MonkeyAsync(latencyProvider, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectLatencyAsync(
            TimeSpan latency,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task> latencyProvider = (_, ct) => SystemClock.SleepAsync(latency, ct);
            return Policy.MonkeyAsync(latencyProvider, injectionRate, enabled);
        }
        
        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectLatencyAsync(
            Func<Context, CancellationToken, Task<TimeSpan>> latency,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task> latencyProvider = async (ctx, ct) =>
            {
                await SystemClock.SleepAsync(await latency(ctx, ct).ConfigureAwait(false), ct).ConfigureAwait(false);
            };
            return Policy.MonkeyAsync(latencyProvider, injectionRate, enabled);
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
        public static MonkeyPolicy<TResult> InjectLatencyAsync<TResult>(
            TimeSpan latency,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task> latencyProvider = (_, ct) => SystemClock.SleepAsync(latency, ct);
            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult(injectionRate);
            Func<Context, Task<bool>> enabledLambda = _ => Task.FromResult(enabled());

            return Policy.MonkeyAsync<TResult>(latencyProvider, injectionRateLambda, enabledLambda);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectLatencyAsync<TResult>(
            TimeSpan latency,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task> latencyProvider = (_, ct) => SystemClock.SleepAsync(latency, ct);
            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult(injectionRate);

            return Policy.MonkeyAsync<TResult>(latencyProvider, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectLatencyAsync<TResult>(
            TimeSpan latency,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task> latencyProvider = (_, ct) => SystemClock.SleepAsync(latency, ct);
            return Policy.MonkeyAsync<TResult>(latencyProvider, injectionRate, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectLatencyAsync<TResult>(
            Func<Context, CancellationToken, Task<TimeSpan>> latency,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task> latencyProvider = async (ctx, ct) =>
            {
                await SystemClock.SleepAsync(await latency(ctx, ct).ConfigureAwait(false), ct).ConfigureAwait(false);
            };
            return Policy.MonkeyAsync<TResult>(latencyProvider, injectionRate, enabled);
        }
    }
}
