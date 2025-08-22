namespace Polly;

/// <summary>
/// Defines extensions for configuring <see cref="PolicyWrap"/> instances on an <see cref="ISyncPolicy"/> or <see cref="ISyncPolicy{TResult}"/>.
/// </summary>
public static class ISyncPolicyPolicyWrapExtensions
{
    /// <summary>
    /// Wraps the specified outer policy round the inner policy.
    /// </summary>
    /// <param name="outerPolicy">The outer policy.</param>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
    public static PolicyWrap Wrap(this ISyncPolicy outerPolicy, ISyncPolicy innerPolicy)
    {
        if (outerPolicy == null)
        {
            throw new ArgumentNullException(nameof(outerPolicy));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
        return ((Policy)outerPolicy).Wrap(innerPolicy);
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types
    }

    /// <summary>
    /// Wraps the specified outer policy round the inner policy.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="outerPolicy">The outer policy.</param>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
    public static PolicyWrap<TResult> Wrap<TResult>(this ISyncPolicy outerPolicy, ISyncPolicy<TResult> innerPolicy)
    {
        if (outerPolicy == null)
        {
            throw new ArgumentNullException(nameof(outerPolicy));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
        return ((Policy)outerPolicy).Wrap(innerPolicy);
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types
    }

    /// <summary>
    /// Wraps the specified outer policy round the inner policy.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="outerPolicy">The outer policy.</param>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
    public static PolicyWrap<TResult> Wrap<TResult>(this ISyncPolicy<TResult> outerPolicy, ISyncPolicy innerPolicy)
    {
        if (outerPolicy == null)
        {
            throw new ArgumentNullException(nameof(outerPolicy));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
        return ((Policy<TResult>)outerPolicy).Wrap(innerPolicy);
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types
    }

    /// <summary>
    /// Wraps the specified outer policy round the inner policy.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="outerPolicy">The outer policy.</param>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
    public static PolicyWrap<TResult> Wrap<TResult>(this ISyncPolicy<TResult> outerPolicy, ISyncPolicy<TResult> innerPolicy)
    {
        if (outerPolicy == null)
        {
            throw new ArgumentNullException(nameof(outerPolicy));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
        return ((Policy<TResult>)outerPolicy).Wrap(innerPolicy);
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types
    }
}

public partial class Policy
{
    /// <summary>
    /// Wraps the specified inner policy.
    /// </summary>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <returns>PolicyWrap.PolicyWrap.</returns>
    public PolicyWrap Wrap(ISyncPolicy innerPolicy)
    {
        if (innerPolicy == null)
        {
            throw new ArgumentNullException(nameof(innerPolicy));
        }

        return new PolicyWrap(
            this,
            innerPolicy);
    }

    /// <summary>
    /// Wraps the specified inner policy.
    /// </summary>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <returns>PolicyWrap.PolicyWrap.</returns>
    public PolicyWrap<TResult> Wrap<TResult>(ISyncPolicy<TResult> innerPolicy)
    {
        if (innerPolicy == null)
        {
            throw new ArgumentNullException(nameof(innerPolicy));
        }

        return new PolicyWrap<TResult>(
            this,
            innerPolicy);
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
        if (innerPolicy == null)
        {
            throw new ArgumentNullException(nameof(innerPolicy));
        }

        return new PolicyWrap<TResult>(
            this,
            innerPolicy);
    }

    /// <summary>
    /// Wraps the specified inner policy.
    /// </summary>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <returns>PolicyWrap.PolicyWrap.</returns>
    public PolicyWrap<TResult> Wrap(ISyncPolicy<TResult> innerPolicy)
    {
        if (innerPolicy == null)
        {
            throw new ArgumentNullException(nameof(innerPolicy));
        }

        return new PolicyWrap<TResult>(
            this,
            innerPolicy);
    }
}

public partial class Policy
{
    /// <summary>
    /// Creates a <see cref="PolicyWrap" /> of the given policies.
    /// </summary>
    /// <param name="policies">The policies to place in the wrap, outermost (at left) to innermost (at right).</param>
    /// <returns>The PolicyWrap.</returns>
    /// <exception cref="ArgumentException">The enumerable of policies to form the wrap must contain at least two policies.</exception>
    public static PolicyWrap Wrap(params ISyncPolicy[] policies)
    {
        if (policies is null)
        {
            throw new ArgumentNullException(nameof(policies));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
        return policies.Length switch
        {
            < MinimumPoliciesRequiredForWrap => throw new ArgumentException(
                "The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies)),
            MinimumPoliciesRequiredForWrap => new PolicyWrap((Policy)policies[0], policies[1]),
            _ => Wrap(policies[0], Wrap(policies.Skip(1).ToArray())),
        };
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types
    }

    /// <summary>
    /// Creates a <see cref="PolicyWrap" /> of the given policies governing delegates returning values of type <typeparamref name="TResult" />.
    /// </summary>
    /// <param name="policies">The policies to place in the wrap, outermost (at left) to innermost (at right).</param>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <returns>The PolicyWrap.</returns>
    /// <exception cref="ArgumentException">The enumerable of policies to form the wrap must contain at least two policies.</exception>
    public static PolicyWrap<TResult> Wrap<TResult>(params ISyncPolicy<TResult>[] policies)
    {
        if (policies is null)
        {
            throw new ArgumentNullException(nameof(policies));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
        return policies.Length switch
        {
            < MinimumPoliciesRequiredForWrap => throw new ArgumentException(
                "The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies)),
            MinimumPoliciesRequiredForWrap => new PolicyWrap<TResult>((Policy<TResult>)policies[0], policies[1]),
            _ => Wrap(policies[0], Wrap([.. policies.Skip(1)])),
        };
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types
    }
}
