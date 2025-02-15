namespace Polly.Specs.Registry;

public class PolicyRegistrySpecs
{
    private readonly PolicyRegistry _registry = [];

    #region Tests for adding Policy

    [Fact]
    public void Should_be_able_to_add_Policy_using_Add()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.Count.ShouldBe(1);

        Policy policy2 = Policy.NoOp();
        string key2 = Guid.NewGuid().ToString();

        _registry.Add(key2, policy2);
        _registry.Count.ShouldBe(2);
    }

    [Fact]
    public void Should_be_able_to_add_PolicyTResult_using_Add()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.Count.ShouldBe(1);

        Policy<ResultPrimitive> policy2 = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key2 = Guid.NewGuid().ToString();

        _registry.Add(key2, policy2);
        _registry.Count.ShouldBe(2);
    }

    [Fact]
    public void Should_be_able_to_add_Policy_by_interface_using_Add()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.Count.ShouldBe(1);

        ISyncPolicy<ResultPrimitive> policy2 = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key2 = Guid.NewGuid().ToString();

        _registry.Add(key2, policy2);
        _registry.Count.ShouldBe(2);
    }

    [Fact]
    public void Should_be_able_to_add_Policy_using_Indexer()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry[key] = policy;
        _registry.Count.ShouldBe(1);

        Policy policy2 = Policy.NoOp();
        string key2 = Guid.NewGuid().ToString();

        _registry[key2] = policy2;
        _registry.Count.ShouldBe(2);
    }

    [Fact]
    public void Should_be_able_to_add_PolicyTResult_using_Indexer()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry[key] = policy;
        _registry.Count.ShouldBe(1);

        Policy<ResultPrimitive> policy2 = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key2 = Guid.NewGuid().ToString();

        _registry[key2] = policy2;
        _registry.Count.ShouldBe(2);
    }

    [Fact]
    public void Should_not_be_able_to_add_Policy_with_duplicate_key_using_Add()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        Should.NotThrow(() => _registry.Add(key, policy));

        Should.Throw<ArgumentException>(() => _registry.Add(key, policy));

        _registry.Count.ShouldBe(1);
    }

    [Fact]
    public void Should_be_able_to_overwrite_existing_Policy_if_key_exists_when_inserting_using_Indexer()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();
        _registry.Add(key, policy);

        Policy policy_new = Policy.NoOp();
        _registry[key] = policy_new;

        _registry.Count.ShouldBe(1);

        _registry.Get<Policy>(key).ShouldBeSameAs(policy_new);
    }

    [Fact]
    public void Should_be_able_to_overwrite_existing_PolicyTResult_if_key_exists_when_inserting_using_Indexer()
    {
        Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();
        _registry.Add<ISyncPolicy<ResultPrimitive>>(key, policy);

        Policy<ResultPrimitive> policy_new = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        _registry[key] = policy_new;

        _registry.Count.ShouldBe(1);

        _registry.Get<Policy<ResultPrimitive>>(key).ShouldBeSameAs(policy_new);
    }

    [Fact]
    public void Should_throw_when_adding_Policy_using_Add_when_key_is_null()
    {
        string key = null!;
        Policy policy = Policy.NoOp();
        Should.Throw<ArgumentNullException>(() => _registry.Add(key, policy));
    }

    [Fact]
    public void Should_throw_when_adding_Policy_using_Indexer_when_key_is_null()
    {
        string key = null!;
        Policy policy = Policy.NoOp();
        Should.Throw<ArgumentNullException>(() => _registry[key] = policy);
    }

    #endregion

    #region Tests for retrieving policy

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_using_TryGet()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.TryGet(key, out Policy? outPolicy).ShouldBeTrue();
        outPolicy.ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_return_false_if_policy_does_not_exist_TryGet()
    {
        var policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.TryGet<NoOpPolicy>(key, out var outPolicy).ShouldBeFalse();
        outPolicy.ShouldBeNull();
    }

    [Fact]
    public void Should_return_false_if_policy_does_not_exist_TryRemove()
    {
        var policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.TryRemove<NoOpPolicy>(key, out var outPolicy).ShouldBeFalse();
        outPolicy.ShouldBeNull();
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_PolicyTResult_using_TryGet()
    {
        Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.TryGet(key, out Policy<ResultPrimitive> outPolicy).ShouldBeTrue();
        outPolicy.ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_TryGet()
    {
        ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.TryGet(key, out ISyncPolicy<ResultPrimitive> outPolicy).ShouldBeTrue();
        outPolicy.ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_using_Get()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.Get<Policy>(key).ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_PolicyTResult_using_Get()
    {
        Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.Get<Policy<ResultPrimitive>>(key).ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_Get()
    {
        ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.Get<ISyncPolicy<ResultPrimitive>>(key).ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_using_Indexer()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry[key].ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_PolicyTResult_using_Indexer()
    {
        Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry[key].ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_Indexer()
    {
        ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry[key].ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGet()
    {
        string key = Guid.NewGuid().ToString();
        Policy? policy = null;
        bool result = false;

        Should.NotThrow(() => result = _registry.TryGet(key, out policy));

        result.ShouldBeFalse();
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGetPolicyTResult()
    {
        string key = Guid.NewGuid().ToString();
        Policy<ResultPrimitive>? policy = null;
        bool result = false;

        Should.NotThrow(() => result = _registry.TryGet(key, out policy));

        result.ShouldBeFalse();
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGetPolicy_by_interface()
    {
        string key = Guid.NewGuid().ToString();
        ISyncPolicy<ResultPrimitive>? policy = null;
        bool result = false;

        Should.NotThrow(() => result = _registry.TryGet<ISyncPolicy<ResultPrimitive>>(key, out policy));

        result.ShouldBeFalse();
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_while_retrieving_using_Get_when_key_does_not_exist()
    {
        string key = Guid.NewGuid().ToString();
        Policy? policy = null;
        Should.Throw<KeyNotFoundException>(() => policy = _registry.Get<Policy>(key));
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_while_retrieving_using_GetTResult_when_key_does_not_exist()
    {
        string key = Guid.NewGuid().ToString();
        Policy<ResultPrimitive>? policy = null;
        Should.Throw<KeyNotFoundException>(() => policy = _registry.Get<Policy<ResultPrimitive>>(key));
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_while_retrieving_using_Get_by_interface_when_key_does_not_exist()
    {
        string key = Guid.NewGuid().ToString();
        ISyncPolicy<ResultPrimitive>? policy = null;
        Should.Throw<KeyNotFoundException>(() => policy = _registry.Get<ISyncPolicy<ResultPrimitive>>(key));
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_while_retrieving_when_key_does_not_exist_using_Indexer()
    {
        string key = Guid.NewGuid().ToString();
        IsPolicy? policy = null;
        Should.Throw<KeyNotFoundException>(() => policy = _registry[key]);
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_when_retrieving_using_Get_when_key_is_null()
    {
        string key = null!;
        Policy? policy = null;
        Should.Throw<ArgumentNullException>(() => policy = _registry.Get<Policy>(key));
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_when_retrieving_using_GetTResult_when_key_is_null()
    {
        string key = null!;
        Policy<ResultPrimitive>? policy = null;
        Should.Throw<ArgumentNullException>(() => policy = _registry.Get<Policy<ResultPrimitive>>(key));
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_when_retrieving_using_Get_by_interface_when_key_is_null()
    {
        string key = null!;
        ISyncPolicy<ResultPrimitive>? policy = null;
        Should.Throw<ArgumentNullException>(() => policy = _registry.Get<ISyncPolicy<ResultPrimitive>>(key));
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_when_retrieving_using_Indexer_when_key_is_null()
    {
        string key = null!;
        IsPolicy? policy = null;
        Should.Throw<ArgumentNullException>(() => policy = _registry[key]);
        policy.ShouldBeNull();
    }
    #endregion

    #region Tests for removing policy
    [Fact]
    public void Should_be_able_to_clear_registry()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.Count.ShouldBe(1);

        Policy policy2 = Policy.NoOp();
        string key2 = Guid.NewGuid().ToString();

        _registry.Add(key2, policy2);
        _registry.Count.ShouldBe(2);

        _registry.Clear();
        _registry.Count.ShouldBe(0);
    }

    [Fact]
    public void Should_be_able_to_remove_policy()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.Count.ShouldBe(1);

        _registry.Remove(key);
        _registry.Count.ShouldBe(0);
    }

    [Fact]
    public void Should_throw_when_removing_Policy_when_key_is_null()
    {
        string key = null!;
        Should.Throw<ArgumentNullException>(() => _registry.Remove(key));
    }
    #endregion

    #region Tests for checking if key exists

    [Fact]
    public void Should_be_able_to_check_if_key_exists()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.ContainsKey(key).ShouldBeTrue();

        string key2 = Guid.NewGuid().ToString();
        _registry.ContainsKey(key2).ShouldBeFalse();
    }

    [Fact]
    public void Should_throw_when_checking_if_key_exists_when_key_is_null()
    {
        string key = null!;
        Should.Throw<ArgumentNullException>(() => _registry.ContainsKey(key));
    }
    #endregion

    #region Tests for the constructor
    [Fact]
    public void Constructor_Called_With_A_Registry_Parameter_Should_Assign_The_Passed_In_Registry_To_The_Registry_Field()
    {
        var testDictionary = Substitute.For<IDictionary<string, IsPolicy>>();
        var testRegistry = new PolicyRegistry(testDictionary);

        // Generally, using reflection is a bad practice, but we are accepting it given we own the implementation.
        var registryField = typeof(PolicyRegistry).GetField("_registry", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance)!;
        var registryFieldValue = registryField.GetValue(testRegistry);
        registryFieldValue.ShouldBe(testDictionary);
    }

    [Fact]
    public void Constructor_Called_With_Default_Parameters_Assigns_A_ConcurrentDictionary_Of_TKey_And_IsPolicy_To_The_Private_Registry_Field()
    {
        var expectedDictionaryType = typeof(ConcurrentDictionary<string, IsPolicy>);
        var testRegistry = new PolicyRegistry();

        // Generally, using reflection is a bad practice, but we are accepting it given we own the implementation.
        var registryField = typeof(PolicyRegistry).GetField("_registry", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance)!;
        var registryFieldValue = registryField.GetValue(testRegistry);
        registryFieldValue.ShouldBeOfType(expectedDictionaryType);
    }

    #endregion
}
