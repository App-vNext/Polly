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
        public PolicyWrap Wrap(ISyncPolicy innerPolicy)
        {
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new PolicyWrap(
                (action, context, cancellationtoken) => PolicyWrapEngine.Implementation(action, context, cancellationtoken, this, innerPolicy),
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
        public PolicyWrap<TResult> Wrap<TResult>(ISyncPolicy<TResult> innerPolicy)
        {
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new PolicyWrap<TResult>(
                (func, context, cancellationtoken) => PolicyWrapEngine.Implementation<TResult>(func, context, cancellationtoken, this, innerPolicy),
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
        public PolicyWrap<TResult> Wrap(ISyncPolicy innerPolicy)
        {
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new PolicyWrap<TResult>(
                (func, context, cancellationtoken) => PolicyWrapEngine.Implementation<TResult>(func, context, cancellationtoken, this, innerPolicy),
                this,
                innerPolicy
                );
        }

        /// <summary>
        /// Wraps the specified inner policy.
        /// </summary>
        /// <param name="innerPolicy">The inner policy.</param>
        /// <returns>PolicyWrap.PolicyWrap.</returns>
        public PolicyWrap<TResult> Wrap(ISyncPolicy<TResult> innerPolicy)
        {
            if (innerPolicy == null) throw new ArgumentNullException(nameof(innerPolicy));

            return new PolicyWrap<TResult>(
                (func, context, cancellationtoken) => PolicyWrapEngine.Implementation<TResult>(func, context, cancellationtoken, this, innerPolicy),
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
        public static PolicyWrap Wrap(params ISyncPolicy[] policies)
        {
            switch (policies.Length)
            {
                case 0:
                case 1:
                    throw new ArgumentException("The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies));
                case 2:
                    return new PolicyWrap(
                        (func, context, cancellationtoken) => PolicyWrapEngine.Implementation(
                            func, 
                            context, 
                            cancellationtoken, 
                            policies[0], 
                            policies[1]), 
                        (Policy)policies[0], policies[1]);

                default:
                    return Wrap(policies[0], Wrap(policies.Skip(1).ToArray()));
            }
        }

        /// <summary>
        /// Creates a <see cref="PolicyWrap" /> of the given policies governing delegates returning values of type <typeparamref name="TResult" />.
        /// </summary>
        /// <param name="policies">The policies to place in the wrap, outermost (at left) to innermost (at right).</param>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <returns>The PolicyWrap.</returns>
        /// <exception cref="System.ArgumentException">The enumerable of policies to form the wrap must contain at least two policies.</exception>
        public static PolicyWrap<TResult> Wrap<TResult>(params ISyncPolicy<TResult>[] policies)
        {
            switch (policies.Length)
            {
                case 0:
                case 1:
                    throw new ArgumentException("The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies));
                case 2:
                    return new PolicyWrap<TResult>(
                        (func, context, cancellationtoken) => PolicyWrapEngine.Implementation(
                            func,
                            context,
                            cancellationtoken,
                            policies[0],
                            policies[1]),
                        (Policy<TResult>)policies[0], policies[1]);

                default:
                    return Wrap(policies[0], Wrap(policies.Skip(1).ToArray()));
            }
        }
    }
}
