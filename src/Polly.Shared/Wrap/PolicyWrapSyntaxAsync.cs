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
        public PolicyWrap WrapAsync(Policy innerPolicy)
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
        public PolicyWrap<TResult> WrapAsync<TResult>(Policy<TResult> innerPolicy)
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
        public PolicyWrap<TResult> WrapAsync(Policy innerPolicy)
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
        public PolicyWrap<TResult> WrapAsync(Policy<TResult> innerPolicy)
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
        public static PolicyWrap WrapAsync(params Policy[] policies)
        {
            switch (policies.Count())
            {
                case 0:
                case 1:
                    throw new ArgumentException("The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies));
                default:
                    IEnumerable<Policy> remainder = policies.Skip(1);
                    return policies.First().WrapAsync(remainder.Count() == 1 ? remainder.Single() : WrapAsync(remainder.ToArray()));
            }
        }

        /// <summary>
        /// Creates a <see cref="PolicyWrap" /> of the given policies governing delegates returning values of type <typeparamref name="TResult" />.
        /// </summary>
        /// <param name="policies">The policies to place in the wrap, outermost (at left) to innermost (at right).</param>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <returns>The PolicyWrap.</returns>
        /// <exception cref="System.ArgumentException">The enumerable of policies to form the wrap must contain at least two policies.</exception>
        public static PolicyWrap<TResult> WrapAsync<TResult>(params Policy<TResult>[] policies)
        {
            switch (policies.Count())
            {
                case 0:
                case 1:
                    throw new ArgumentException("The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies));
                default:
                    IEnumerable<Policy<TResult>> remainder = policies.Skip(1);
                    return policies.First().WrapAsync(remainder.Count() == 1 ? remainder.Single() : WrapAsync(remainder.ToArray()));
            }
        }
    }
}
