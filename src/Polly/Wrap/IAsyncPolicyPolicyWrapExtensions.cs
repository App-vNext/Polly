namespace Polly;

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
    public static AsyncPolicyWrap WrapAsync(this IAsyncPolicy outerPolicy, IAsyncPolicy innerPolicy)
    {
        if (outerPolicy == null)
        {
            throw new ArgumentNullException(nameof(outerPolicy));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
        return ((AsyncPolicy)outerPolicy).WrapAsync(innerPolicy);
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types
    }

    /// <summary>
    /// Wraps the specified outer policy round the inner policy.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="outerPolicy">The outer policy.</param>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
    public static AsyncPolicyWrap<TResult> WrapAsync<TResult>(this IAsyncPolicy outerPolicy, IAsyncPolicy<TResult> innerPolicy)
    {
        if (outerPolicy == null)
        {
            throw new ArgumentNullException(nameof(outerPolicy));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
        return ((AsyncPolicy)outerPolicy).WrapAsync(innerPolicy);
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types
    }

    /// <summary>
    /// Wraps the specified outer policy round the inner policy.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="outerPolicy">The outer policy.</param>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
    public static AsyncPolicyWrap<TResult> WrapAsync<TResult>(this IAsyncPolicy<TResult> outerPolicy, IAsyncPolicy innerPolicy)
    {
        if (outerPolicy == null)
        {
            throw new ArgumentNullException(nameof(outerPolicy));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
        return ((AsyncPolicy<TResult>)outerPolicy).WrapAsync(innerPolicy);
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types
    }

    /// <summary>
    /// Wraps the specified outer policy round the inner policy.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="outerPolicy">The outer policy.</param>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <returns>A <see cref="PolicyWrap"/> instance representing the combined wrap.</returns>
    public static AsyncPolicyWrap<TResult> WrapAsync<TResult>(this IAsyncPolicy<TResult> outerPolicy, IAsyncPolicy<TResult> innerPolicy)
    {
        if (outerPolicy == null)
        {
            throw new ArgumentNullException(nameof(outerPolicy));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
        return ((AsyncPolicy<TResult>)outerPolicy).WrapAsync(innerPolicy);
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types
    }
}

public partial class AsyncPolicy
{
    /// <summary>
    /// Wraps the specified inner policy.
    /// </summary>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <returns>PolicyWrap.PolicyWrap.</returns>
    public AsyncPolicyWrap WrapAsync(IAsyncPolicy innerPolicy)
    {
        if (innerPolicy == null)
        {
            throw new ArgumentNullException(nameof(innerPolicy));
        }

        return new AsyncPolicyWrap(
            this,
            innerPolicy);
    }

    /// <summary>
    /// Wraps the specified inner policy.
    /// </summary>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <returns>PolicyWrap.PolicyWrap.</returns>
    public AsyncPolicyWrap<TResult> WrapAsync<TResult>(IAsyncPolicy<TResult> innerPolicy)
    {
        if (innerPolicy == null)
        {
            throw new ArgumentNullException(nameof(innerPolicy));
        }

        return new AsyncPolicyWrap<TResult>(
            this,
            innerPolicy);
    }
}

public partial class AsyncPolicy<TResult>
{
    /// <summary>
    /// Wraps the specified inner policy.
    /// </summary>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <returns>PolicyWrap.PolicyWrap.</returns>
    public AsyncPolicyWrap<TResult> WrapAsync(IAsyncPolicy innerPolicy)
    {
        if (innerPolicy == null)
        {
            throw new ArgumentNullException(nameof(innerPolicy));
        }

        return new AsyncPolicyWrap<TResult>(
            this,
            innerPolicy);
    }

    /// <summary>
    /// Wraps the specified inner policy.
    /// </summary>
    /// <param name="innerPolicy">The inner policy.</param>
    /// <returns>PolicyWrap.PolicyWrap.</returns>
    public AsyncPolicyWrap<TResult> WrapAsync(IAsyncPolicy<TResult> innerPolicy)
    {
        if (innerPolicy == null)
        {
            throw new ArgumentNullException(nameof(innerPolicy));
        }

        return new AsyncPolicyWrap<TResult>(
            this,
            innerPolicy);
    }
}

public partial class Policy
{
    private const int MinimumPoliciesRequiredForWrap = 2;

    /// <summary>
    /// Creates a <see cref="PolicyWrap" /> of the given policies.
    /// </summary>
    /// <param name="policies">The policies to place in the wrap, outermost (at left) to innermost (at right).</param>
    /// <returns>The PolicyWrap.</returns>
    /// <exception cref="ArgumentException">The enumerable of policies to form the wrap must contain at least two policies.</exception>
    public static AsyncPolicyWrap WrapAsync(params IAsyncPolicy[] policies)
    {
        if (policies is null)
        {
            throw new ArgumentNullException(nameof(policies));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
        return policies.Length switch
        {
            < MinimumPoliciesRequiredForWrap => throw new ArgumentException("The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies)),
            MinimumPoliciesRequiredForWrap => new AsyncPolicyWrap((AsyncPolicy)policies[0], policies[1]),
            _ => WrapAsync(policies[0], WrapAsync([.. policies.Skip(1)])),
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
    public static AsyncPolicyWrap<TResult> WrapAsync<TResult>(params IAsyncPolicy<TResult>[] policies)
    {
        if (policies is null)
        {
            throw new ArgumentNullException(nameof(policies));
        }

#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
        return policies.Length switch
        {
            < MinimumPoliciesRequiredForWrap => throw new ArgumentException("The enumerable of policies to form the wrap must contain at least two policies.", nameof(policies)),
            MinimumPoliciesRequiredForWrap => new AsyncPolicyWrap<TResult>((AsyncPolicy<TResult>)policies[0], policies[1]),
            _ => WrapAsync(policies[0], WrapAsync([.. policies.Skip(1)])),
        };
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types
    }
}
