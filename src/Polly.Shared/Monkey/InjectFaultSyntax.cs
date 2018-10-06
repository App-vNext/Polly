using System;
using Polly.Monkey;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining an Fault Injection <see cref="Policy"/>. 
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
        public static MonkeyPolicy InjectFault(
            Exception fault,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return Policy.Monkey(fault, injectionRate, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectFault(
            Exception fault,
            Double injectionRate,
            Func<Context, bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, Double> injectionRateLambda = _ => injectionRate;
            return Policy.Monkey(fault, injectionRate, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">lambda to get the fault exception object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectFault(
            Func<Context, Exception> fault,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return Policy.Monkey(fault, injectionRate, enabled);
        }
    }

    /// <summary>
    /// Fluent API for defining an Fault Injection <see cref="Policy"/>.
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
        public static MonkeyPolicy<TResult> InjectFault<TResult>(
            Exception fault,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, Exception> faultLambda = _ => fault;
            Func<Context, Double> injectionRateLambda = _ => injectionRate;
            Func<Context, bool> enabledLambda = _ =>
            {
                return enabled();
            };

            return Policy.Monkey<TResult>(faultLambda, injectionRateLambda, enabledLambda);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in context free fashion</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectFault<TResult>(
            TResult fault,
            Double injectionRate,
            Func<bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, TResult> faultLambda = _ => fault;
            Func<Context, Double> injectionRateLambda = _ => injectionRate;
            Func<Context, bool> enabledLambda = _ =>
            {
                return enabled();
            };

            return Policy.Monkey<TResult>(faultLambda, injectionRateLambda, enabledLambda);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which injects a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">The fault exception object to throw</param>
        /// <param name="injectionRate">injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectFault<TResult>(
            Exception fault,
            Double injectionRate,
            Func<Context, bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, Exception> faultLambda = _ => fault;
            Func<Context, Double> injectionRateLambda = _ => injectionRate;

            return Policy.Monkey<TResult>(faultLambda, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">lambda to get the fault exception object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectFault<TResult>(
            Func<Context, Exception> fault,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return Policy.Monkey<TResult>(fault, injectionRate, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">lambda to get the fault exception object</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectFault<TResult>(
            Func<Context, TResult> fault,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return Policy.Monkey<TResult>(fault, injectionRate, enabled);
        }
    }
}
