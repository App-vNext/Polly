namespace Polly.Specs.Registry;

public class ReadOnlyPolicyRegistrySpecs
{
    private readonly IPolicyRegistry<string> _registry;

    private IReadOnlyPolicyRegistry<string> ReadOnlyRegistry => _registry;

    public ReadOnlyPolicyRegistrySpecs() =>
        _registry = new PolicyRegistry();

    #region Tests for retrieving policy

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_using_TryGet()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.TryGet(key, out Policy? outPolicy).ShouldBeTrue();
        outPolicy.ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_PolicyTResult_using_TryGet()
    {
        Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.TryGet(key, out Policy<ResultPrimitive>? outPolicy).ShouldBeTrue();
        outPolicy.ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_TryGet()
    {
        ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.TryGet(key, out ISyncPolicy<ResultPrimitive>? outPolicy).ShouldBeTrue();
        outPolicy.ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_using_Get()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.Get<Policy>(key).ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_PolicyTResult_using_Get()
    {
        Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.Get<Policy<ResultPrimitive>>(key).ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_Get()
    {
        ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.Get<ISyncPolicy<ResultPrimitive>>(key).ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_using_Indexer()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry[key].ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_PolicyTResult_using_Indexer()
    {
        Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry[key].ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_Indexer()
    {
        ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry[key].ShouldBeSameAs(policy);
    }

    [Fact]
    public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGet()
    {
        string key = Guid.NewGuid().ToString();
        Policy? policy = null;
        bool result = false;

        Should.NotThrow(() => result = ReadOnlyRegistry.TryGet(key, out policy));

        result.ShouldBeFalse();
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGetPolicyTResult()
    {
        string key = Guid.NewGuid().ToString();
        Policy<ResultPrimitive>? policy = null;
        bool result = false;

        Should.NotThrow(() => result = ReadOnlyRegistry.TryGet(key, out policy));

        result.ShouldBeFalse();
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGetPolicy_by_interface()
    {
        string key = Guid.NewGuid().ToString();
        ISyncPolicy<ResultPrimitive>? policy = null;
        bool result = false;

        Should.NotThrow(() => result = ReadOnlyRegistry.TryGet<ISyncPolicy<ResultPrimitive>>(key, out policy));

        result.ShouldBeFalse();
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_while_retrieving_using_Get_when_key_does_not_exist()
    {
        string key = Guid.NewGuid().ToString();
        Policy? policy = null;
        Should.Throw<KeyNotFoundException>(() => policy = ReadOnlyRegistry.Get<Policy>(key));
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_while_retrieving_using_GetTResult_when_key_does_not_exist()
    {
        string key = Guid.NewGuid().ToString();
        Policy<ResultPrimitive>? policy = null;
        Should.Throw<KeyNotFoundException>(() => policy = ReadOnlyRegistry.Get<Policy<ResultPrimitive>>(key));
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_while_retrieving_using_Get_by_interface_when_key_does_not_exist()
    {
        string key = Guid.NewGuid().ToString();
        ISyncPolicy<ResultPrimitive>? policy = null;
        Should.Throw<KeyNotFoundException>(() => policy = ReadOnlyRegistry.Get<ISyncPolicy<ResultPrimitive>>(key));
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_while_retrieving_when_key_does_not_exist_using_Indexer()
    {
        string key = Guid.NewGuid().ToString();
        IsPolicy? outPolicy = null;
        Should.Throw<KeyNotFoundException>(() => outPolicy = ReadOnlyRegistry[key]);
        outPolicy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_when_retrieving_using_Get_when_key_is_null()
    {
        string key = null!;
        Policy? policy = null;
        Should.Throw<ArgumentNullException>(() => policy = ReadOnlyRegistry.Get<Policy>(key));
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_when_retrieving_using_GetTResult_when_key_is_null()
    {
        string key = null!;
        Policy<ResultPrimitive>? policy = null;
        Should.Throw<ArgumentNullException>(() => policy = ReadOnlyRegistry.Get<Policy<ResultPrimitive>>(key));
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_when_retrieving_using_Get_by_interface_when_key_is_null()
    {
        string key = null!;
        ISyncPolicy<ResultPrimitive>? policy = null;
        Should.Throw<ArgumentNullException>(() => policy = ReadOnlyRegistry.Get<ISyncPolicy<ResultPrimitive>>(key));
        policy.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_when_retrieving_using_Indexer_when_key_is_null()
    {
        string key = null!;
        IsPolicy? policy = null;
        Should.Throw<ArgumentNullException>(() => policy = ReadOnlyRegistry[key]);
        policy.ShouldBeNull();
    }
    #endregion

    #region Tests for checking if key exists

    [Fact]
    public void Should_be_able_to_check_if_key_exists()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.ContainsKey(key).ShouldBeTrue();

        string key2 = Guid.NewGuid().ToString();
        ReadOnlyRegistry.ContainsKey(key2).ShouldBeFalse();
    }

    [Fact]
    public void Should_throw_when_checking_if_key_exists_when_key_is_null()
    {
        string key = null!;
        Should.Throw<ArgumentNullException>(() => ReadOnlyRegistry.ContainsKey(key));
    }
    #endregion

    #region Tests for the GetEnumerator method

    [Fact]
    public void Calling_The_GetEnumerator_Method_Returning_A_IEnumerator_Of_KeyValuePair_Of_String_And_IsPolicy_Calls_The_Registrys_GetEnumerator_Method()
    {
        var testDictionary = Substitute.For<IDictionary<string, IsPolicy>>();
        var testRegistry = new PolicyRegistry(testDictionary);

        testRegistry.GetEnumerator();

        testDictionary.Received(1).GetEnumerator();
    }

    #endregion

    #region Collection initializer tests

    [Fact]
    public void Policies_Should_Be_Added_To_The_Registry_When_Using_Collection_Initializer_Syntax()
    {
        string key = Guid.NewGuid().ToString();
        var policy = Policy.NoOp();

        var testRegistry = new PolicyRegistry
        {
            [key] = policy
        };

        testRegistry.ShouldBe(new Dictionary<string, IsPolicy>
        {
            [key] = policy
        });
    }
    #endregion
}
