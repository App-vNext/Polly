using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Monkey
{
    public partial class MonkeyPolicy
    {
        /// <summary>
        /// Builds an <see cref="AsyncInjectOutcomePolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectOutcomePolicy InjectFaultAsync(
            Exception fault,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<Exception> FaultLambda(Context _, CancellationToken __) => Task.FromResult<Exception>(fault);
            Task<double> InjectionRateLambda(Context _) => Task.FromResult<Double>(injectionRate);
            Task<bool> EnabledLambda(Context _) => Task.FromResult<bool>(enabled());

            return new AsyncInjectOutcomePolicy(FaultLambda, InjectionRateLambda, EnabledLambda);
        }

        /// <summary>
        /// Builds an <see cref="AsyncInjectOutcomePolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectOutcomePolicy InjectFaultAsync(
            Exception fault,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<Exception> FaultLambda(Context _, CancellationToken __) => Task.FromResult<Exception>(fault);
            Task<double> InjectionRateLambda(Context _) => Task.FromResult<Double>(injectionRate);

            return new AsyncInjectOutcomePolicy(FaultLambda, InjectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds an <see cref="AsyncInjectOutcomePolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="faultProvider">lambda to get the fault exception object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectOutcomePolicy InjectFaultAsync(
            Func<Context, CancellationToken, Task<Exception>> faultProvider,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (faultProvider == null) throw new ArgumentNullException(nameof(faultProvider));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));
            
            return new AsyncInjectOutcomePolicy(faultProvider, injectionRate, enabled);
        }
    }

    public partial class MonkeyPolicy
    {
        #region Exception Based Faults
        /// <summary>
        /// Builds an <see cref="AsyncInjectOutcomePolicy{TResult}"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectOutcomePolicy<TResult> InjectFaultAsync<TResult>(
            Exception fault,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<Exception> FaultLambda(Context _, CancellationToken __) => Task.FromResult<Exception>(fault);
            Task<double> InjectionRateLambda(Context _) => Task.FromResult<Double>(injectionRate);
            Task<bool> EnabledLambda(Context _) => Task.FromResult<bool>(enabled());

            return new AsyncInjectOutcomePolicy<TResult>((Func<Context, CancellationToken, Task<Exception>>)FaultLambda, InjectionRateLambda, EnabledLambda);
        }

        /// <summary>
        /// Builds an <see cref="AsyncInjectOutcomePolicy{TResult}"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectOutcomePolicy<TResult> InjectFaultAsync<TResult>(
            Exception fault,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<Exception> FaultLambda(Context _, CancellationToken __) => Task.FromResult<Exception>(fault);
            Task<double> InjectionRateLambda(Context _) => Task.FromResult<Double>(injectionRate);

            return new AsyncInjectOutcomePolicy<TResult>((Func<Context, CancellationToken, Task<Exception>>)FaultLambda, InjectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds an <see cref="AsyncInjectOutcomePolicy{TResult}"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="faultProvider">lambda to get the fault exception object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectOutcomePolicy<TResult> InjectFaultAsync<TResult>(
            Func<Context, CancellationToken, Task<Exception>> faultProvider,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (faultProvider == null) throw new ArgumentNullException(nameof(faultProvider));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));
            
            return new AsyncInjectOutcomePolicy<TResult>(faultProvider, injectionRate, enabled);
        }
        #endregion

        #region TResult based Faults

        /// <summary>
        /// Builds an <see cref="AsyncInjectOutcomePolicy{TResult}"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectOutcomePolicy<TResult> InjectFaultAsync<TResult>(
            TResult fault,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<TResult> FaultLambda(Context _, CancellationToken __) => Task.FromResult<TResult>(fault);
            Task<double> InjectionRateLambda(Context _) => Task.FromResult<Double>(injectionRate);
            Task<bool> EnabledLambda(Context _)
            {
                return Task.FromResult<bool>(enabled());
            }

            return new AsyncInjectOutcomePolicy<TResult>((Func<Context, CancellationToken, Task<TResult>>)FaultLambda, InjectionRateLambda, EnabledLambda);
        }

        /// <summary>
        /// Builds an <see cref="AsyncInjectOutcomePolicy{TResult}"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectOutcomePolicy<TResult> InjectFaultAsync<TResult>(
            TResult fault,
            Double injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Task<TResult> FaultLambda(Context _, CancellationToken __) => Task.FromResult<TResult>(fault);
            Task<double> InjectionRateLambda(Context _) => Task.FromResult<Double>(injectionRate);

            return new AsyncInjectOutcomePolicy<TResult>((Func<Context, CancellationToken, Task<TResult>>)FaultLambda, InjectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds an <see cref="AsyncInjectOutcomePolicy{TResult}"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">lambda to get the fault exception object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static AsyncInjectOutcomePolicy<TResult> InjectFaultAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> fault,
            Func<Context, Task<Double>> injectionRate,
            Func<Context, Task<bool>> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return new AsyncInjectOutcomePolicy<TResult>(fault, injectionRate, enabled);
        }
        #endregion
    }
}