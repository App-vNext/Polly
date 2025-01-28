namespace Polly;

public abstract partial class Policy
{
    /// <summary>
    /// Sets the PolicyKey for this <see cref="Policy"/> instance.
    /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
    /// </summary>
    /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
    /// <returns>An instance of <see cref="Policy"/>.</returns>
    public Policy WithPolicyKey(string policyKey)
    {
        if (policyKeyInternal != null)
        {
            throw PolicyKeyMustBeImmutableException(nameof(policyKey));
        }

        policyKeyInternal = policyKey;
        return this;
    }

    /// <summary>
    /// Sets the PolicyKey for this <see cref="Policy"/> instance.
    /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
    /// </summary>
    /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
    /// <returns>An instance of <see cref="ISyncPolicy"/>.</returns>
    ISyncPolicy ISyncPolicy.WithPolicyKey(string policyKey)
    {
        if (policyKeyInternal != null)
        {
            throw PolicyKeyMustBeImmutableException(nameof(policyKey));
        }

        policyKeyInternal = policyKey;
        return this;
    }
}

#pragma warning disable CA1724
public abstract partial class Policy<TResult>
#pragma warning restore CA1724
{
    /// <summary>
    /// Sets the PolicyKey for this <see cref="Policy{TResult}"/> instance.
    /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
    /// </summary>
    /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy{TResult}"/> instance.</param>
    /// <returns>An instance of <see cref="Policy{TResult}"/>.</returns>
    public Policy<TResult> WithPolicyKey(string policyKey)
    {
        if (policyKeyInternal != null)
        {
            throw PolicyKeyMustBeImmutableException(nameof(policyKey));
        }

        policyKeyInternal = policyKey;
        return this;
    }

    /// <summary>
    /// Sets the PolicyKey for this <see cref="Policy{TResult}"/> instance.
    /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
    /// </summary>
    /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy{TResult}"/> instance.</param>
    /// <returns>An instance of <see cref="ISyncPolicy{TResult}"/>.</returns>
    ISyncPolicy<TResult> ISyncPolicy<TResult>.WithPolicyKey(string policyKey)
    {
        if (policyKeyInternal != null)
        {
            throw PolicyKeyMustBeImmutableException(nameof(policyKey));
        }

        policyKeyInternal = policyKey;
        return this;
    }
}
