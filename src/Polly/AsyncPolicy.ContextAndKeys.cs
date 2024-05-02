namespace Polly;

public abstract partial class AsyncPolicy
{
    /// <summary>
    /// Sets the PolicyKey for this <see cref="AsyncPolicy"/> instance.
    /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
    /// </summary>
    /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="AsyncPolicy"/> instance.</param>
    /// <returns>An instance of <see cref="AsyncPolicy"/>.</returns>
    public AsyncPolicy WithPolicyKey(string policyKey)
    {
        if (policyKeyInternal != null)
        {
            throw PolicyKeyMustBeImmutableException(nameof(policyKey));
        }

        policyKeyInternal = policyKey;
        return this;
    }

    /// <summary>
    /// Sets the PolicyKey for this <see cref="IAsyncPolicy"/> instance.
    /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
    /// </summary>
    /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="IAsyncPolicy"/> instance.</param>
    /// <returns>An instance of <see cref="IAsyncPolicy"/>.</returns>
    IAsyncPolicy IAsyncPolicy.WithPolicyKey(string policyKey)
    {
        if (policyKeyInternal != null)
        {
            throw PolicyKeyMustBeImmutableException(nameof(policyKey));
        }

        policyKeyInternal = policyKey;
        return this;
    }
}

public abstract partial class AsyncPolicy<TResult>
{
    /// <summary>
    /// Sets the PolicyKey for this <see cref="AsyncPolicy{TResult}"/> instance.
    /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
    /// </summary>
    /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="AsyncPolicy{TResult}"/> instance.</param>
    /// <returns>An instance of <see cref="AsyncPolicy{TResult}"/>.</returns>
    public AsyncPolicy<TResult> WithPolicyKey(string policyKey)
    {
        if (policyKeyInternal != null)
        {
            throw PolicyKeyMustBeImmutableException(nameof(policyKey));
        }

        policyKeyInternal = policyKey;
        return this;
    }

    /// <summary>
    /// Sets the PolicyKey for this <see cref="IAsyncPolicy{TResult}"/> instance.
    /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
    /// </summary>
    /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="IAsyncPolicy{TResult}"/> instance.</param>
    /// <returns>An instance of <see cref="IAsyncPolicy{TResult}"/>.</returns>
    IAsyncPolicy<TResult> IAsyncPolicy<TResult>.WithPolicyKey(string policyKey)
    {
        if (policyKeyInternal != null)
        {
            throw PolicyKeyMustBeImmutableException(nameof(policyKey));
        }

        policyKeyInternal = policyKey;
        return this;
    }
}
