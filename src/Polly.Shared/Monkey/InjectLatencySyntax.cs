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
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">Fault Delegate to be executed</param>
        /// <param name="injectionRate">The injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectLatency(
            Action<Context> fault,
            Decimal injectionRate,
            Func<Context, bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, Decimal> injectionRateLambda = _ => injectionRate;
            return Policy.Monkey(fault, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">Fault Delegate to be executed</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy InjectLatency(
            Action<Context> fault,
            Func<Context, Decimal> injectionRate,
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
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">Fault Delegate to be executed</param>
        /// <param name="injectionRate">The injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectLatency<TResult>(
            Action<Context> fault,
            Decimal injectionRate,
            Func<Context, bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            Func<Context, Decimal> injectionRateLambda = _ => injectionRate;
            return Policy.Monkey<TResult>(fault, injectionRateLambda, enabled);
        }

        /// <summary>
        /// Builds a <see cref="MonkeyPolicy"/> which executes a fault if <paramref name="enabled"/> returns true and
        /// a random number is within range of <paramref name="injectionRate"/>.
        /// </summary>
        /// <param name="fault">Fault Delegate to be executed</param>
        /// <param name="injectionRate">lambda to get injection rate between [0, 1]</param>
        /// <param name="enabled">Lambda to check if this policy is enabled in current context</param>
        /// <returns>The policy instance.</returns>
        public static MonkeyPolicy<TResult> InjectLatency<TResult>(
            Action<Context> fault,
            Func<Context, Decimal> injectionRate,
            Func<Context, bool> enabled)
        {
            if (fault == null) throw new ArgumentNullException(nameof(fault));
            if (injectionRate == null) throw new ArgumentNullException(nameof(injectionRate));
            if (enabled == null) throw new ArgumentNullException(nameof(enabled));

            return Policy.Monkey<TResult>(fault, injectionRate, enabled);
        }
    }
}