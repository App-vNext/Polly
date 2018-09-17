using System;
using System.Collections.Generic;
using System.Linq;
using Polly.Wrap;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// Wraps the specified inner policy.
        /// </summary>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <returns>PolicyWrap.PolicyWrap.</returns>
        public PolicyWrap WrapAsync(IAsyncPolicy innerPolicy)
        {
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new PolicyWrap(
                (action, context, cancellationtoken, continueOnCapturedContext) => PolicyWrapEngine.ImplementationAsync(action, context, cancellationtoken, continueOnCapturedContext, this, innerPolicy),
                this,
                innerPolicy
                );
        }

        /// <summary>
        /// Wraps the specified inner policy.
        /// </summary>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <returns>PolicyWrap.PolicyWrap.</returns>
        public PolicyWrap<TResult> WrapAsync<TResult>(IAsyncPolicy<TResult> innerPolicy)
        {
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new PolicyWrap<TResult>(
                (func, context, cancellationtoken, continueOnCapturedContext) => PolicyWrapEngine.ImplementationAsync<TResult>(func, context, cancellationtoken, continueOnCapturedContext, this, innerPolicy),
                this,
                innerPolicy
                );
        }
    }

    public partial class Policy<TResult>
    {
        /// <summary>
        /// Wraps the specified inner policy.
        /// </summary>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <returns>PolicyWrap.PolicyWrap.</returns>
        public PolicyWrap<TResult> WrapAsync(IAsyncPolicy innerPolicy)
        {
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new PolicyWrap<TResult>(
                (func, context, cancellationtoken, continueOnCapturedContext) => PolicyWrapEngine.ImplementationAsync<TResult>(func, context, cancellationtoken, continueOnCapturedContext, this, innerPolicy),
                this,
                innerPolicy
                );
        }

        /// <summary>
        /// Wraps the specified inner policy.
        /// </summary>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <returns>PolicyWrap.PolicyWrap.</returns>
        public PolicyWrap<TResult> WrapAsync(IAsyncPolicy<TResult> innerPolicy)
        {
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new PolicyWrap<TResult>(
                (func, context, cancellationtoken, continueOnCapturedContext) => PolicyWrapEngine.ImplementationAsync<TResult>(func, context, cancellationtoken, continueOnCapturedContext, this, innerPolicy),
                this,
                innerPolicy
                );
        }
    }

    public partial class Policy
    {
        /// <summary>
        /// Creates a <see cref="PolicyWrap" /> of the given policies.
        /// </summary>
        /// <param name="policies">The policies to place in the wrap, outermost (at left) to innermost (at right).</param>
        /// <returns>The PolicyWrap.</returns>
        /// <exception cref="System.ArgumentException">The enumerable of policies to form the wrap must contain at least two policies.</exception>
        public static PolicyWrap WrapAsync(params IAsyncPolicy[] policies)
        {
            switch (policies.Length)
            {
                case 0:
                case 1:
                    throw new ArgumentException("The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies));
                case 2:
                    return new PolicyWrap(
                        (func, context, cancellationtoken, continueOnCapturedContext) => PolicyWrapEngine.ImplementationAsync(
                            func,
                            context,
                            cancellationtoken,
                            continueOnCapturedContext,
                            policies[0],
                            policies[1]),
                        (Policy)policies[0], policies[1]);

                default:
                    return WrapAsync(policies[0], WrapAsync(policies.Skip(1).ToArray()));
            }
        }

        /// <summary>
        /// Creates a <see cref="PolicyWrap" /> of the given policies governing delegates returning values of type <typeparamref name="TResult" />.
        /// </summary>
        /// <param name="policies">The policies to place in the wrap, outermost (at left) to innermost (at right).</param>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <returns>The PolicyWrap.</returns>
        /// <exception cref="System.ArgumentException">The enumerable of policies to form the wrap must contain at least two policies.</exception>
        public static PolicyWrap<TResult> WrapAsync<TResult>(params IAsyncPolicy<TResult>[] policies)
        {
            switch (policies.Length)
            {
                case 0:
                case 1:
                    throw new ArgumentException("The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies));
                case 2:
                    return new PolicyWrap<TResult>(
                        (func, context, cancellationtoken, continueOnCapturedContext) => PolicyWrapEngine.ImplementationAsync<TResult>(
                            func,
                            context,
                            cancellationtoken,
                            continueOnCapturedContext,
                            policies[0],
                            policies[1]),
                        (Policy<TResult>)policies[0], policies[1]);

                default:
                    return WrapAsync(policies[0], WrapAsync(policies.Skip(1).ToArray()));
            }
        }
    }

    /// <summary>
    /// Defines extensions for configuring <see cref="PolicyWrap"/> instances on an <see cref="IAsyncPolicy"/> or <see cref="IAsyncPolicy{TResult}"/>.
    /// </summary>
    public static class IAsyncPolicyPolicyWrapExtensions
    {
        /// <summary>
        /// Wraps the specified outer policy round the inner policy.
        /// </summary>
        /// <param name="outerPolicy">The outer policy.</param>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
        public static PolicyWrap WrapAsync(this IAsyncPolicy outerPolicy, IAsyncPolicy innerPolicy)
        {
            if (outerPolicy == null) throw new ArgumentNullException(nameof(outerPolicy));
            return ((Policy)outerPolicy).WrapAsync(innerPolicy);
        }

        /// <summary>
        /// Wraps the specified outer policy round the inner policy.
        /// </summary>
        /// <param name="outerPolicy">The outer policy.</param>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
        public static PolicyWrap<TResult> WrapAsync<TResult>(this IAsyncPolicy outerPolicy, IAsyncPolicy<TResult> innerPolicy)
        {
            if (outerPolicy == null) throw new ArgumentNullException(nameof(outerPolicy));
            return ((Policy)outerPolicy).WrapAsync(innerPolicy);
        }

        /// <summary>
        /// Wraps the specified outer policy round the inner policy.
        /// </summary>
        /// <param name="outerPolicy">The outer policy.</param>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
        public static PolicyWrap<TResult> WrapAsync<TResult>(this IAsyncPolicy<TResult> outerPolicy, IAsyncPolicy innerPolicy)
        {
            if (outerPolicy == null) throw new ArgumentNullException(nameof(outerPolicy));
            return ((Policy<TResult>)outerPolicy).WrapAsync(innerPolicy);
        }

        /// <summary>
        /// Wraps the specified outer policy round the inner policy.
        /// </summary>
        /// <param name="outerPolicy">The outer policy.</param>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
        public static PolicyWrap<TResult> WrapAsync<TResult>(this IAsyncPolicy<TResult> outerPolicy, IAsyncPolicy<TResult> innerPolicy)
        {
            if (outerPolicy == null) throw new ArgumentNullException(nameof(outerPolicy));
            return ((Policy<TResult>)outerPolicy).WrapAsync(innerPolicy);
        }
    }
}
