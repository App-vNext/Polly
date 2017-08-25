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
            switch (policies.Count())
            {
                case 0:
                case 1:
                    throw new ArgumentException("The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies));
                default:
                    IAsyncPolicy[] remainder = policies.Skip(1).ToArray();
                    return WrapAsync(policies.First(), remainder.Count() == 1 ? remainder.Single() : WrapAsync(remainder));
            }
        }

        private static PolicyWrap WrapAsync(IAsyncPolicy outerPolicy, IAsyncPolicy innerPolicy)
        {
            if (outerPolicy == null) throw new ArgumentNullException(nameof(outerPolicy));
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new PolicyWrap(
                (action, context, cancellationtoken, continueOnCapturedContext) => PolicyWrapEngine.ImplementationAsync(action, context, cancellationtoken, continueOnCapturedContext, outerPolicy, innerPolicy),
                outerPolicy,
                innerPolicy
                );
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
            switch (policies.Count())
            {
                case 0:
                case 1:
                    throw new ArgumentException("The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies));
                default:
                    IAsyncPolicy<TResult>[] remainder = policies.Skip(1).ToArray();
                    return WrapAsync(policies.First(), remainder.Count() == 1 ? remainder.Single() : WrapAsync(remainder));
            }
        }

        private static PolicyWrap<TResult> WrapAsync<TResult>(IAsyncPolicy<TResult> outerPolicy, IAsyncPolicy<TResult> innerPolicy)
        {
            if (outerPolicy == null) throw new ArgumentNullException(nameof(outerPolicy));
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new PolicyWrap<TResult>(
                (func, context, cancellationtoken, continueOnCapturedContext) => PolicyWrapEngine.ImplementationAsync<TResult>(func, context, cancellationtoken, continueOnCapturedContext, outerPolicy, innerPolicy),
                outerPolicy,
                innerPolicy
                );
        }
    }
}
