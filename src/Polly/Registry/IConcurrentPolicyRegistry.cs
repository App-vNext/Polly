namespace Polly.Registry;

/// <summary>
/// Represents a collection of policies keyed by <typeparamref name="TKey"/> which can be updated and consumed in a thread-safe manner.
/// </summary>
/// <typeparam name="TKey">The type of keys in the policy registry.</typeparam>
public interface IConcurrentPolicyRegistry<TKey> : IPolicyRegistry<TKey>
{
    /// <summary>
    /// Adds an element with the provided key and policy to the registry.
    /// </summary>
    /// <param name="key">The key for the policy.</param>
    /// <param name="policy">The policy to store in the registry.</param>
    /// <typeparam name="TPolicy">The type of Policy.</typeparam>
    /// <returns>True if Policy was added. False otherwise.</returns>
    bool TryAdd<TPolicy>(TKey key, TPolicy policy)
        where TPolicy : IsPolicy;

    /// <summary>
    /// Removes the policy stored under the specified <paramref name="key"/> from the registry.
    /// </summary>
    /// <param name="key">The <paramref name="key"/> of the policy to remove.</param>
    /// <param name="policy">
    /// This method returns the policy associated with the specified <paramref name="key"/>, if the
    /// key is found; otherwise null.
    /// This parameter is passed uninitialized.
    /// </param>
    /// <typeparam name="TPolicy">The type of Policy.</typeparam>
    /// <returns>True if the policy is successfully removed. Otherwise false.</returns>
    bool TryRemove<TPolicy>(TKey key, out TPolicy policy)
        where TPolicy : IsPolicy;

    /// <summary>
    /// Compares the existing policy for the specified key with a specified policy, and if they are equal, updates the policy with a third value.
    /// </summary>
    /// <typeparam name="TPolicy">The type of Policy.</typeparam>
    /// <param name="key">The key whose value is compared with comparisonPolicy, and possibly replaced.</param>
    /// <param name="newPolicy">The policy that replaces the value for the specified <paramref name="key"/>, if the comparison results in equality.</param>
    /// <param name="comparisonPolicy">The policy that is compared to the existing policy at the specified key.</param>
    /// <returns></returns>
    bool TryUpdate<TPolicy>(TKey key, TPolicy newPolicy, TPolicy comparisonPolicy)
        where TPolicy : IsPolicy;

    /// <summary>
    /// Adds a policy with the provided key and policy to the registry
    /// if the key does not already exist.
    /// </summary>
    /// <param name="key">The key of the policy to add.</param>
    /// <param name="policyFactory">The function used to generate a policy for the key.</param>
    /// <returns>The policy for the key. This will be either the existing policy for the key if the
    /// key is already in the registry, or the new policy for the key as returned by policyFactory
    /// if the key was not in the registry.</returns>
    TPolicy GetOrAdd<TPolicy>(TKey key, Func<TKey, TPolicy> policyFactory)
        where TPolicy : IsPolicy;

    /// <summary>
    /// Adds a key/policy pair to the registry
    /// if the key does not already exist.
    /// </summary>
    /// <param name="key">The key of the policy to add.</param>
    /// <param name="policy">The value to be added, if the key does not already exist.</param>
    /// <returns>The policy for the key. This will be either the existing policy for the key if the
    /// key is already in the registry, or the new policy if the key was not in the registry.</returns>
    TPolicy GetOrAdd<TPolicy>(TKey key, TPolicy policy)
        where TPolicy : IsPolicy;

    /// <summary>
    /// Adds a key/policy pair to the registry if the key does not already
    /// exist, or updates a key/policy pair in the registry if the key
    /// already exists.
    /// </summary>
    /// <param name="key">The key to be added or whose policy should be updated.</param>
    /// <param name="addPolicyFactory">The function used to generate a policy for an absent key.</param>
    /// <param name="updatePolicyFactory">The function used to generate a new policy for an existing key
    /// based on the key's existing value.</param>
    /// <returns>The new policy for the key. This will be either be the result of addPolicyFactory (if the key was
    /// absent) or the result of updatePolicyFactory (if the key was present).</returns>
    TPolicy AddOrUpdate<TPolicy>(TKey key, Func<TKey, TPolicy> addPolicyFactory, Func<TKey, TPolicy, TPolicy> updatePolicyFactory)
        where TPolicy : IsPolicy;

    /// <summary>
    /// Adds a key/policy pair to the registry if the key does not already
    /// exist, or updates a key/policy pair in the registry if the key
    /// already exists.
    /// </summary>
    /// <param name="key">The key to be added or whose policy should be updated.</param>
    /// <param name="addPolicy">The policy to be added for an absent key.</param>
    /// <param name="updatePolicyFactory">The function used to generate a new policy for an existing key based on
    /// the key's existing value.</param>
    /// <returns>The new policy for the key. This will be either be addPolicy (if the key was
    /// absent) or the result of updatePolicyFactory (if the key was present).</returns>
    TPolicy AddOrUpdate<TPolicy>(TKey key, TPolicy addPolicy, Func<TKey, TPolicy, TPolicy> updatePolicyFactory)
        where TPolicy : IsPolicy;
}
