using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Monkey
{
    public partial class MonkeyPolicy
    {
        /// <summary>
        /// Builds an <see cref="AsyncInjectLatencyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">The latency to inject</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectLatencyPolicy InjectLatencyAsync(
            TimeSpan latency,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<TimeSpan> LatencyProvider(Context _, CancellationToken __) => Task.FromResult(latency);
            Task<double> InjectionRateLambda(Context _) => Task.FromResult(injectionRate);
            Task<bool> EnabledLambda(Context _) => Task.FromResult(enabled());

            return new AsyncInjectLatencyPolicy(LatencyProvider, InjectionRateLambda, EnabledLambda);
        }

        /// <summary>
        /// Builds an <see cref="AsyncInjectLatencyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">The latency to inject</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectLatencyPolicy InjectLatencyAsync(
            TimeSpan latency,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<TimeSpan> LatencyProvider(Context _, CancellationToken __) => Task.FromResult(latency);
            Task<double> InjectionRateLambda(Context _) => Task.FromResult(injectionRate);

            return new AsyncInjectLatencyPolicy(LatencyProvider, InjectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds an <see cref="AsyncInjectLatencyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectLatencyPolicy InjectLatencyAsync(
            TimeSpan latency,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<TimeSpan> LatencyProvider(Context _, CancellationToken __) => Task.FromResult(latency);
            return new AsyncInjectLatencyPolicy(LatencyProvider, injectionRate, enabled);
        }

        /// <summary>
        /// Builds an <see cref="AsyncInjectLatencyPolicy"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectLatencyPolicy InjectLatencyAsync(
            Func<Context, CancellationToken, Task<TimeSpan>> latency,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new AsyncInjectLatencyPolicy(latency, injectionRate, enabled);
        }
    }

    public partial class MonkeyPolicy
    {
        /// <summary>
        /// Builds an <see cref="AsyncInjectLatencyPolicy{TResult}"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectLatencyPolicy<TResult> InjectLatencyAsync<TResult>(
            TimeSpan latency,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<TimeSpan> LatencyProvider(Context _, CancellationToken __) => Task.FromResult(latency);
            Task<double> InjectionRateLambda(Context _) => Task.FromResult(injectionRate);
            Task<bool> EnabledLambda(Context _) => Task.FromResult(enabled());

            return new AsyncInjectLatencyPolicy<TResult>(LatencyProvider, InjectionRateLambda, EnabledLambda);
        }

        /// <summary>
        /// Builds an <see cref="AsyncInjectLatencyPolicy{TResult}"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectLatencyPolicy<TResult> InjectLatencyAsync<TResult>(
            TimeSpan latency,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<TimeSpan> LatencyProvider(Context _, CancellationToken __) => Task.FromResult(latency);
            Task<double> InjectionRateLambda(Context _) => Task.FromResult(injectionRate);

            return new AsyncInjectLatencyPolicy<TResult>(LatencyProvider, InjectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds an <see cref="AsyncInjectLatencyPolicy{TResult}"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectLatencyPolicy<TResult> InjectLatencyAsync<TResult>(
            TimeSpan latency,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<TimeSpan> LatencyProvider(Context _, CancellationToken __) => Task.FromResult(latency);
            return new AsyncInjectLatencyPolicy<TResult>(LatencyProvider, injectionRate, enabled);
        }

        /// <summary>
        /// Builds an <see cref="AsyncInjectLatencyPolicy{TResult}"/> which injects latency if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="latency">lambda to get the latency object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectLatencyPolicy<TResult> InjectLatencyAsync<TResult>(
            Func<Context, CancellationToken, Task<TimeSpan>> latency,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (latency == null) throw new ArgumentNullException(nameof(latency));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new AsyncInjectLatencyPolicy<TResult>(latency, injectionRate, enabled);
        }
    }
}
