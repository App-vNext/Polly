﻿using System;
using System.Threading;
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
        #region Exception Based Monkey Policies
        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free mode</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy MonkeyAsync(
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
        public static MonkeyPolicy MonkeyAsync(
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
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy MonkeyAsync(
            Func<Context, CancellationToken, Task<Exception>> fault,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult<Double>(injectionRate);
            return Policy.MonkeyAsync(fault, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy MonkeyAsync(
            Exception fault,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task<Exception>> faultLambda = (_, __) => Task.FromResult<Exception>(fault);
            return Policy.MonkeyAsync(faultLambda, injectionRate, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">lambda to get the fault exception object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy MonkeyAsync(
            Func<Context, CancellationToken, Task<Exception>> fault,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new MonkeyPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => MonkeyEngine.ImplementationAsync(
                    async (ctx, ct) => { await action(ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    context,
                    cancellationToken,
                    fault,
                    injectionRate,
                    enabled,
                    continueOnCapturedContext));
        }
        #endregion

        #region Action Delegate based Monkey Policies
        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">Fault Delegate to be executed without context</param>
        /// <param name="injectionRate">The injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy MonkeyAsync(
            Func<CancellationToken, Task> fault,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, CancellationToken, Task> faultLamda = async (_, cancellationToken) => await fault(cancellationToken);
            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult<Double>(injectionRate);
            return Policy.MonkeyAsync(faultLamda, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">Fault Delegate to be executed</param>
        /// <param name="injectionRate">The injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy MonkeyAsync(
            Func<Context, CancellationToken, Task> fault,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, Task<Double>> injectionRateLambda = _ => Task.FromResult<Double>(injectionRate);
            return Policy.MonkeyAsync(fault, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>

        /// <param name="fault">Fault Delegate to be executed</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy MonkeyAsync(
            Func<Context, CancellationToken, Task> fault,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new MonkeyPolicy(
                (action, context, cancellationToken, continueOnCapturedContext) => MonkeyEngine.ImplementationAsync(
                    async (ctx, ct) => { await action(ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
                    context,
                    cancellationToken,
                    fault,
                    injectionRate,
                    enabled,
                    continueOnCapturedContext));
        }
        #endregion
    }
}
