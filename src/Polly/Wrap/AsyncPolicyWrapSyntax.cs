using System;
using System.Linq;
using Polly.Wrap;

namespace Polly
{
    public partial class Policy
    {
        /// <summary>
        /// Creates a <see cref="PolicyWrap" /> of the given policies.
        /// </summary>
        /// <param name="policies">The policies to place in the wrap, outermost (at left) to innermost (at right).</param>
        /// <returns>The PolicyWrap.</returns>
        /// <exception cref="ArgumentException">The enumerable of policies to form the wrap must contain at least two policies.</exception>
        public static IAsyncPolicyWrap WrapAsync(params IAsyncPolicy[] policies)
        {
            switch (policies.Length)
            {
                case 0:
                case 1:
                    throw new ArgumentException("The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies));
                case 2:
                    return new AsyncPolicyWrap(policies[0], policies[1]);

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
        /// <exception cref="ArgumentException">The enumerable of policies to form the wrap must contain at least two policies.</exception>
        public static IAsyncPolicyWrap<TResult> WrapAsync<TResult>(params IAsyncPolicy<TResult>[] policies)
        {
            switch (policies.Length)
            {
                case 0:
                case 1:
                    throw new ArgumentException("The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies));
                case 2:
                    return new AsyncPolicyWrap<TResult>(policies[0], policies[1]);

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
        /// Wraps the given outer policy round the inner policy.
        /// </summary>
        /// <param name="outerPolicy">The outer policy.</param>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
        public static IAsyncPolicyWrap WrapAsync(this IAsyncPolicy outerPolicy, IAsyncPolicy innerPolicy)
        {
            if (outerPolicy == null) throw new ArgumentNullException(nameof(outerPolicy));
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new AsyncPolicyWrap(
                outerPolicy,
                innerPolicy
            );
        }

        /// <summary>
        /// Wraps the given outer policy round the inner policy.
        /// </summary>
        /// <param name="outerPolicy">The outer policy.</param>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
        public static IAsyncPolicyWrap<TResult> WrapAsync<TResult>(this IAsyncPolicy outerPolicy, IAsyncPolicy<TResult> innerPolicy)
        {
            if (outerPolicy == null) throw new ArgumentNullException(nameof(outerPolicy));
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new AsyncPolicyWrap<TResult>(
                outerPolicy,
                innerPolicy
            );
        }

        /// <summary>
        /// Wraps the given outer policy round the inner policy.
        /// </summary>
        /// <param name="outerPolicy">The outer policy.</param>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
        public static IAsyncPolicyWrap<TResult> WrapAsync<TResult>(this IAsyncPolicy<TResult> outerPolicy, IAsyncPolicy innerPolicy)
        {
            if (outerPolicy == null) throw new ArgumentNullException(nameof(outerPolicy));
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new AsyncPolicyWrap<TResult>(
                outerPolicy,
                innerPolicy
            );
        }

        /// <summary>
        /// Wraps the given outer policy round the inner policy.
        /// </summary>
        /// <param name="outerPolicy">The outer policy.</param>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
        public static IAsyncPolicyWrap<TResult> WrapAsync<TResult>(this IAsyncPolicy<TResult> outerPolicy, IAsyncPolicy<TResult> innerPolicy)
        {
            if (outerPolicy == null) throw new ArgumentNullException(nameof(outerPolicy));
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new AsyncPolicyWrap<TResult>(
                outerPolicy,
                innerPolicy
            );
        }
    }
}
