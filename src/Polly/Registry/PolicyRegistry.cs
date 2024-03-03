namespace Polly.Registry;

/// <inheritdoc />
/// <summary>
/// Stores a registry of <see cref="string" /> and policy pairs.
/// </summary>
/// <remarks>Uses ConcurrentDictionary to store the collection.</remarks>
public class PolicyRegistry : IConcurrentPolicyRegistry<string>
{
    private readonly IDictionary<string, IsPolicy> _registry = new ConcurrentDictionary<string, IsPolicy>();

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyRegistry"/> class, with <see cref="string"/> keys.
    /// </summary>
    public PolicyRegistry()
    {
        // This empty public constructor must be retained while the adjacent internal constructor exists for testing.
        // The integration with HttpClientFactory, method services.AddPolicyRegistry(), depends on this empty public constructor.
        // Do not collapse the two constructors into a constructor with optional parameter registry == null.
        // That breaks the requirement for a noargs public constructor, against which nuget-published .NET Core dlls have been compiled.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyRegistry"/> class, with <see cref="IDictionary{string, IsPolicy}"/> dictionary.
    /// <remarks>This internal constructor exists solely to facilitate testing of the GetEnumerator() methods, which allow us to support collection initialisation syntax.</remarks>
    /// </summary>
    /// <param name="registry">a dictionary containing keys and policies used for testing.</param>
    internal PolicyRegistry(IDictionary<string, IsPolicy> registry) =>
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));

    private ConcurrentDictionary<string, IsPolicy> ThrowIfNotConcurrentImplementation()
    {
        if (_registry is ConcurrentDictionary<string, IsPolicy> concurrentRegistry)
        {
            return concurrentRegistry;
        }

        throw new InvalidOperationException($"This {nameof(PolicyRegistry)} is not configured for concurrent operations. This exception should never be thrown in production code as the only public constructors create {nameof(PolicyRegistry)} instances of the correct form.");
    }

    /// <summary>
    /// Total number of policies in the registry.
    /// </summary>
    public int Count => _registry.Count;

    /// <summary>
    /// Adds a policy with the provided key and policy to the registry.
    /// </summary>
    /// <param name="key">The key for the policy.</param>
    /// <param name="policy">The policy to store in the registry.</param>
    /// <typeparam name="TPolicy">The type of Policy.</typeparam>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">A Policy with same <paramref name="key"/> already exists.</exception>
    public void Add<TPolicy>(string key, TPolicy policy)
        where TPolicy : IsPolicy => _registry.Add(key, policy);

    /// <summary>
    /// Adds a policy with the provided key and policy to the registry.
    /// </summary>
    /// <param name="key">The key for the policy.</param>
    /// <param name="policy">The policy to store in the registry.</param>
    /// <typeparam name="TPolicy">The type of Policy.</typeparam>
    /// <returns>True if Policy was added. False otherwise.</returns>
    public bool TryAdd<TPolicy>(string key, TPolicy policy)
        where TPolicy : IsPolicy
    {
        var registry = ThrowIfNotConcurrentImplementation();

        return registry.TryAdd(key, policy);
    }

    /// <summary>
    /// Gets of sets the <see cref="IsPolicy"/> with the specified key.
    /// <remarks>To retrieve a policy directly as a particular Policy type or Policy interface (avoiding a cast), use the <see cref="Get{TPolicy}"/> method.</remarks>
    /// </summary>
    /// <param name="key">The key of the policy to get or set.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException">The given key was not present in the registry.</exception>
    /// <returns>The policy associated with the specified key.</returns>
    public IsPolicy this[string key]
    {
        get => _registry[key];
        set => _registry[key] = value;
    }

    /// <summary>
    /// Gets the policy stored under the provided key, casting to <typeparamref name="TPolicy"/>.
    /// </summary>
    /// <typeparam name="TPolicy">The type of Policy.</typeparam>
    /// <param name="key">The key of the policy to get.</param>
    /// <returns>The policy stored in the registry under the given key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException">The given key was not present in the registry.</exception>
    public TPolicy Get<TPolicy>(string key)
        where TPolicy : IsPolicy => (TPolicy)_registry[key];

    /// <summary>
    /// Gets the policy stored under the provided key, casting to <typeparamref name="TPolicy"/>.
    /// </summary>
    /// <param name="key">The key of the policy to get.</param>
    /// <param name="policy">
    /// This method returns the policy associated with the specified <paramref name="key"/>, if the
    /// key is found; otherwise null.
    /// This parameter is passed uninitialized.
    /// </param>
    /// <typeparam name="TPolicy">The type of Policy.</typeparam>
    /// <returns>True if Policy exists for the provided Key. False otherwise.</returns>
    public bool TryGet<TPolicy>(string key, out TPolicy policy)
        where TPolicy : IsPolicy
    {
        bool got = _registry.TryGetValue(key, out IsPolicy value);
        policy = got ? (TPolicy)value : default;
        return got;
    }

    /// <summary>
    /// Removes all keys and policies from registry.
    /// </summary>
    public void Clear() =>
        _registry.Clear();

    /// <summary>
    /// Determines whether the specified <paramref name="key"/> exists.
    /// </summary>
    /// <param name="key">The key to locate in the registry.</param>
    /// <returns>True if <paramref name="key"/> exists otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public bool ContainsKey(string key) =>
        _registry.ContainsKey(key);

    /// <summary>
    /// Removes the policy stored under the specified <paramref name="key"/> from the registry.
    /// </summary>
    /// <param name="key">The <paramref name="key"/> of the policy to remove.</param>
    /// <returns>True if the policy is successfully removed. Otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public bool Remove(string key) =>
        _registry.Remove(key);

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
    public bool TryRemove<TPolicy>(string key, out TPolicy policy)
        where TPolicy : IsPolicy
    {
        var registry = ThrowIfNotConcurrentImplementation();

        bool got = registry.TryRemove(key, out IsPolicy value);
        policy = got ? (TPolicy)value : default;
        return got;
    }

    /// <summary>
    /// Compares the existing policy for the specified key with a specified policy, and if they are equal, updates the policy with a third value.
    /// </summary>
    /// <typeparam name="TPolicy">The type of the policy.</typeparam>
    /// <param name="key">The key whose value is compared with comparisonPolicy, and possibly replaced.</param>
    /// <param name="newPolicy">The policy that replaces the value for the specified <paramref name="key"/>, if the comparison results in equality.</param>
    /// <param name="comparisonPolicy">The policy that is compared to the existing policy at the specified key.</param>
    /// <returns>
    /// <see langword="true"/> if the value with <paramref name="key"/> was equal to <paramref name="comparisonPolicy"/> and
    /// replaced with <paramref name="newPolicy"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryUpdate<TPolicy>(string key, TPolicy newPolicy, TPolicy comparisonPolicy)
        where TPolicy : IsPolicy
    {
        var registry = ThrowIfNotConcurrentImplementation();

        return registry.TryUpdate(key, newPolicy, comparisonPolicy);
    }

    /// <summary>
    /// Adds a policy with the provided key and policy to the registry
    /// if the key does not already exist.
    /// </summary>
    /// <param name="key">The key of the policy to add.</param>
    /// <param name="policyFactory">The function used to generate a policy for the key</param>
    /// <returns>The policy for the key.  This will be either the existing policy for the key if the
    /// key is already in the registry, or the new policy for the key as returned by policyFactory
    /// if the key was not in the registry.</returns>
    public TPolicy GetOrAdd<TPolicy>(string key, Func<string, TPolicy> policyFactory)
        where TPolicy : IsPolicy
    {
        var registry = ThrowIfNotConcurrentImplementation();

        return (TPolicy)registry.GetOrAdd(key, k => policyFactory(k));
    }

    /// <summary>
    /// Adds a key/policy pair to the registry
    /// if the key does not already exist.
    /// </summary>
    /// <param name="key">The key of the policy to add.</param>
    /// <param name="policy">the policy to be added, if the key does not already exist</param>
    /// <returns>The policy for the key.  This will be either the existing policy for the key if the
    /// key is already in the registry, or the new policy if the key was not in the registry.</returns>
    public TPolicy GetOrAdd<TPolicy>(string key, TPolicy policy)
        where TPolicy : IsPolicy
    {
        var registry = ThrowIfNotConcurrentImplementation();

        return (TPolicy)registry.GetOrAdd(key, policy);
    }

    /// <summary>
    /// Adds a key/policy pair to the registry if the key does not already
    /// exist, or updates a key/policy pair in the registry if the key
    /// already exists.
    /// </summary>
    /// <param name="key">The key to be added or whose policy should be updated</param>
    /// <param name="addPolicyFactory">The function used to generate a policy for an absent key</param>
    /// <param name="updatePolicyFactory">The function used to generate a new policy for an existing key
    /// based on the key's existing value</param>
    /// <returns>The new policy for the key.  This will be either be the result of addPolicyFactory (if the key was
    /// absent) or the result of updatePolicyFactory (if the key was present).</returns>
    public TPolicy AddOrUpdate<TPolicy>(string key, Func<string, TPolicy> addPolicyFactory, Func<string, TPolicy, TPolicy> updatePolicyFactory)
        where TPolicy : IsPolicy
    {
        var registry = ThrowIfNotConcurrentImplementation();

        return (TPolicy)registry.AddOrUpdate(key, k => addPolicyFactory(k), (k, e) => updatePolicyFactory(k, (TPolicy)e));
    }

    /// <summary>
    /// Adds a key/policy pair to the registry if the key does not already
    /// exist, or updates a key/policy pair in the registry if the key
    /// already exists.
    /// </summary>
    /// <param name="key">The key to be added or whose policy should be updated</param>
    /// <param name="addPolicy">The policy to be added for an absent key</param>
    /// <param name="updatePolicyFactory">The function used to generate a new policy for an existing key based on
    /// the key's existing value</param>
    /// <returns>The new policy for the key.  This will be either be addPolicy (if the key was
    /// absent) or the result of updatePolicyFactory (if the key was present).</returns>
    public TPolicy AddOrUpdate<TPolicy>(string key, TPolicy addPolicy, Func<string, TPolicy, TPolicy> updatePolicyFactory)
        where TPolicy : IsPolicy
    {
        var registry = ThrowIfNotConcurrentImplementation();

        return (TPolicy)registry.AddOrUpdate(key, addPolicy, (k, e) => updatePolicyFactory(k, (TPolicy)e));
    }

    /// <summary>Returns an enumerator that iterates through the policy objects in the <see
    /// cref="PolicyRegistry"/>.</summary>
    /// <returns>An enumerator for the <see cref="PolicyRegistry"/>.</returns>
    /// <remarks>
    /// The enumerator returned from the registry is safe to use concurrently with
    /// reads and writes to the registry, however it does not represent a moment-in-time snapshot
    /// of the registry's contents.  The contents exposed through the enumerator may contain modifications
    /// made to the registry after <see cref="GetEnumerator"/> was called.
    /// This is not considered a significant issue as typical usage of PolicyRegistry is for bulk population at app startup,
    /// with only infrequent changes to the PolicyRegistry during app running, if using PolicyRegistry for dynamic updates during running.
    /// </remarks>
    public IEnumerator<KeyValuePair<string, IsPolicy>> GetEnumerator() =>
        _registry.GetEnumerator();

    /// <summary>Returns an enumerator that iterates through the policy objects in the <see
    /// cref="PolicyRegistry"/>.</summary>
    /// <returns>An enumerator for the <see cref="PolicyRegistry"/>.</returns>
    /// <remarks>
    /// The enumerator returned from the registry is safe to use concurrently with
    /// reads and writes to the registry, however it does not represent a moment-in-time snapshot
    /// of the registry's contents.  The contents exposed through the enumerator may contain modifications
    /// made to the registry after <see cref="GetEnumerator"/> was called.
    /// This is not considered a significant issue as typical usage of PolicyRegistry is for bulk population at app startup,
    /// with only infrequent changes to the PolicyRegistry during app running, if using PolicyRegistry for dynamic updates during running.
    /// </remarks>
    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
