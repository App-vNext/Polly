using System;
using System.Threading.Tasks;
using Polly.Monkey;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining Monkey <see cref="Policy"/>. 
    /// </summary>
    public partial class Policy
    {
        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> MonkeyAsync<TResult>(
            Exception fault,
            Decimal injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, Task<Exception>> faultLambda = _ => Task.FromResult<Exception>(fault);
            Func<Context, Task<Decimal>> injectionRateLambda = _ => Task.FromResult<Decimal>(injectionRate);

            return Policy.MonkeyAsync<TResult>(faultLambda, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">lambda to get the fault exception object</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> MonkeyAsync<TResult>(
            Func<Context, Task<Exception>> fault,
            Decimal injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, Task<Decimal>> injectionRateLambda = _ => Task.FromResult<Decimal>(injectionRate);
            return Policy.MonkeyAsync<TResult>(fault, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> MonkeyAsync<TResult>(
            Exception fault,
            Func<Context, Task<Decimal>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, Task<Exception>> faultLambda = _ => Task.FromResult<Exception>(fault);
            return Policy.MonkeyAsync<TResult>(faultLambda, injectionRate, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">lambda to get the fault exception object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> MonkeyAsync<TResult>(
            Func<Context, Task<Exception>> fault,
            Func<Context, Task<Decimal>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new MonkeyPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) => MonkeyEngine.ImplementationAsync<TResult>(
                    action,
                    context,
                    cancellationToken,
                    fault,
                    injectionRate,
                    enabled,
                    continueOnCapturedContext));
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">Fault Delegate to be executed</param>
        /// <param name="injectionRate">The injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> MonkeyAsync<TResult>(
            Func<Context, Task> fault,
            Decimal injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, Task<Decimal>> injectionRateLambda = _ => Task.FromResult<Decimal>(injectionRate);
            return Policy.MonkeyAsync<TResult>(fault, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">Fault Delegate to be executed</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> MonkeyAsync<TResult>(
            Func<Context, Task> fault,
            Func<Context, Task<Decimal>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new MonkeyPolicy<TResult>(
                (action, context, cancellationToken, continueOnCapturedContext) => MonkeyEngine.ImplementationAsync<TResult>(
                    action,
                    context,
                    cancellationToken,
                    fault,
                    injectionRate,
                    enabled,
                    continueOnCapturedContext));
        }
    }
}
