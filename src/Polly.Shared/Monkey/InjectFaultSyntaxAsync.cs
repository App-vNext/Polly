using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Monkey;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining a Fallback <see cref="Policy"/>. 
    /// </summary>
    public partial class Policy
    {
        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectFaultAsync(
            Exception fault,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task<Exception>> faultLambda = (_, __) => Task.FromResult<Exception>(fault);
            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult<Double>(injectionRate);
            Func<Context, Task<bool>> enabledLambda = _ =>
            {
                return Task.FromResult<bool>(enabled());
            };

            return Policy.MonkeyAsync(faultLambda, injectionRateLambda, enabledLambda);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectFaultAsync(
            Exception fault,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task<Exception>> faultLambda = (_, __) => Task.FromResult<Exception>(fault);
            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult<Double>(injectionRate);

            return Policy.MonkeyAsync(faultLambda, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">lambda to get the fault exception object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectFaultAsync(
            Func<Context, CancellationToken, Task<Exception>> fault,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return Policy.MonkeyAsync(fault, injectionRate, enabled);
        }
    }

    /// <summary>
    /// Fluent API for defining a Fallback <see cref="Policy"/>. 
    /// </summary>
    public partial class Policy
    {
        #region Exception Based Faults
        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectFaultAsync<TResult>(
            Exception fault,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task<Exception>> faultLambda = (_, __) => Task.FromResult<Exception>(fault);
            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult<Double>(injectionRate);
            Func<Context, Task<bool>> enabledLambda = _ =>
            {
                return Task.FromResult<bool>(enabled());
            };

            return Policy.MonkeyAsync<TResult>(faultLambda, injectionRateLambda, enabledLambda);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectFaultAsync<TResult>(
            Exception fault,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task<Exception>> faultLambda = (_, __) => Task.FromResult<Exception>(fault);
            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult<Double>(injectionRate);

            return Policy.MonkeyAsync<TResult>(faultLambda, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">lambda to get the fault exception object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectFaultAsync<TResult>(
            Func<Context, CancellationToken, Task<Exception>> fault,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return Policy.MonkeyAsync<TResult>(fault, injectionRate, enabled);
        }
        #endregion

        #region TResult based Faults
        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectFaultAsync<TResult>(
            TResult fault,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task<TResult>> faultLambda = (_, __) => Task.FromResult<TResult>(fault);
            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult<Double>(injectionRate);
            Func<Context, Task<bool>> enabledLambda = _ =>
            {
                return Task.FromResult<bool>(enabled());
            };

            return Policy.MonkeyAsync<TResult>(faultLambda, injectionRateLambda, enabledLambda);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectFaultAsync<TResult>(
            TResult fault,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task<TResult>> faultLambda = (_, __) => Task.FromResult<TResult>(fault);
            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult<Double>(injectionRate);

            return Policy.MonkeyAsync<TResult>(faultLambda, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">lambda to get the fault exception object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectFaultAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> fault,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return Policy.MonkeyAsync<TResult>(fault, injectionRate, enabled);
        }
        #endregion
    }
}