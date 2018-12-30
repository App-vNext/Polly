using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Monkey
{
    /// <summary>
    /// Fluent API for defining Monkey <see cref="Policy"/>. 
    /// </summary>
    public partial class MonkeyPolicy
    {
        #region Action Delegate Based Monkey Policies

        /// <summary>
        /// Builds a <see cref="AsyncInjectBehaviourPolicy"/> which executes a behaviour if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="behaviour">Behaviour Delegate to be executed without context</param>
        /// <param name="injectionRate">The injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free mode</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectBehaviourPolicy<TResult> InjectBehaviourAsync<TResult>(
            Func<Task> behaviour,
            Double injectionRate,
            Func<Task<bool>> enabled)
        {
            if (behaviour == null) throw new ArgumentNullException(nameof(behaviour));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task BehaviourLambda(Context _, CancellationToken __) => behaviour();
            Task<Double> InjectionRateLambda(Context _) => Task.FromResult(injectionRate);
            Task<bool> EnabledLambda(Context _) => enabled();

            return InjectBehaviourAsync<TResult>(BehaviourLambda, InjectionRateLambda, EnabledLambda);
        }

        /// <summary>
        /// Builds a <see cref="AsyncInjectBehaviourPolicy"/> which executes a behaviour if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="behaviour">Behaviour Delegate to be executed</param>
        /// <param name="injectionRate">The injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free mode</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectBehaviourPolicy<TResult> InjectBehaviourAsync<TResult>(
            Func<Context, CancellationToken, Task> behaviour,
            Double injectionRate,
            Func<Task<bool>> enabled)
        {
            if (behaviour == null) throw new ArgumentNullException(nameof(behaviour));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<Double> InjectionRateLambda(Context _) => Task.FromResult(injectionRate);
            Task<bool> EnabledLambda(Context _) => enabled();

            return InjectBehaviourAsync<TResult>(behaviour, InjectionRateLambda, EnabledLambda);
        }

        /// <summary>
        /// Builds a <see cref="AsyncInjectBehaviourPolicy"/> which executes a behaviour if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="behaviour">Behaviour Delegate to be executed</param>
        /// <param name="injectionRate">The injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectBehaviourPolicy<TResult> InjectBehaviourAsync<TResult>(
            Func<Context, CancellationToken, Task> behaviour,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (behaviour == null) throw new ArgumentNullException(nameof(behaviour));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<Double> InjectionRateLambda(Context _) => Task.FromResult(injectionRate);

            return InjectBehaviourAsync<TResult>(behaviour, InjectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="AsyncInjectBehaviourPolicy"/> which executes a behaviour if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="behaviour">Behaviour Delegate to be executed</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectBehaviourPolicy<TResult> InjectBehaviourAsync<TResult>(
            Func<Context, CancellationToken, Task> behaviour,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (behaviour == null) throw new ArgumentNullException(nameof(behaviour));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new AsyncInjectBehaviourPolicy<TResult>(
                    behaviour,
                    injectionRate,
                    enabled);
        }

        #endregion
    }
}