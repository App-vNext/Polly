using static Polly.Specs.DictionaryHelpers;

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
        ReadOnlyRegistry.TryGet(key, out Policy? outPolicy).Should().BeTrue();
        outPolicy.Should().BeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_PolicyTResult_using_TryGet()
    {
        Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.TryGet(key, out Policy<ResultPrimitive>? outPolicy).Should().BeTrue();
        outPolicy.Should().BeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_TryGet()
    {
        ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.TryGet(key, out ISyncPolicy<ResultPrimitive>? outPolicy).Should().BeTrue();
        outPolicy.Should().BeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_using_Get()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.Get<Policy>(key).Should().BeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_PolicyTResult_using_Get()
    {
        Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.Get<Policy<ResultPrimitive>>(key).Should().BeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_Get()
    {
        ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.Get<ISyncPolicy<ResultPrimitive>>(key).Should().BeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_using_Indexer()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry[key].Should().BeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_PolicyTResult_using_Indexer()
    {
        Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry[key].Should().BeSameAs(policy);
    }

    [Fact]
    public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_Indexer()
    {
        ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry[key].Should().BeSameAs(policy);
    }

    [Fact]
    public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGet()
    {
        string key = Guid.NewGuid().ToString();
        Policy? policy = null;
        bool result = false;

        ReadOnlyRegistry.Invoking(r => result = r.TryGet(key, out policy))
            .Should().NotThrow();

        result.Should().BeFalse();
        policy.Should().BeNull();
    }

    [Fact]
    public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGetPolicyTResult()
    {
        string key = Guid.NewGuid().ToString();
        Policy<ResultPrimitive>? policy = null;
        bool result = false;

        ReadOnlyRegistry.Invoking(r => result = r.TryGet(key, out policy))
            .Should().NotThrow();

        result.Should().BeFalse();
        policy.Should().BeNull();
    }

    [Fact]
    public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGetPolicy_by_interface()
    {
        string key = Guid.NewGuid().ToString();
        ISyncPolicy<ResultPrimitive>? policy = null;
        bool result = false;

        ReadOnlyRegistry.Invoking(r => result = r.TryGet<ISyncPolicy<ResultPrimitive>>(key, out policy))
            .Should().NotThrow();

        result.Should().BeFalse();
        policy.Should().BeNull();
    }

    [Fact]
    public void Should_throw_while_retrieving_using_Get_when_key_does_not_exist()
    {
        string key = Guid.NewGuid().ToString();
        Policy? policy = null;
        ReadOnlyRegistry.Invoking(r => policy = r.Get<Policy>(key))
            .Should().Throw<KeyNotFoundException>();
        policy.Should().BeNull();
    }

    [Fact]
    public void Should_throw_while_retrieving_using_GetTResult_when_key_does_not_exist()
    {
        string key = Guid.NewGuid().ToString();
        Policy<ResultPrimitive>? policy = null;
        ReadOnlyRegistry.Invoking(r => policy = r.Get<Policy<ResultPrimitive>>(key))
            .Should().Throw<KeyNotFoundException>();
        policy.Should().BeNull();
    }

    [Fact]
    public void Should_throw_while_retrieving_using_Get_by_interface_when_key_does_not_exist()
    {
        string key = Guid.NewGuid().ToString();
        ISyncPolicy<ResultPrimitive>? policy = null;
        ReadOnlyRegistry.Invoking(r => policy = r.Get<ISyncPolicy<ResultPrimitive>>(key))
            .Should().Throw<KeyNotFoundException>();
        policy.Should().BeNull();
    }

    [Fact]
    public void Should_throw_while_retrieving_when_key_does_not_exist_using_Indexer()
    {
        string key = Guid.NewGuid().ToString();
        IsPolicy? outPolicy = null;
        ReadOnlyRegistry.Invoking(r => outPolicy = r[key])
            .Should().Throw<KeyNotFoundException>();
        outPolicy.Should().BeNull();
    }

    [Fact]
    public void Should_throw_when_retrieving_using_Get_when_key_is_null()
    {
        string key = null!;
        Policy? policy = null;
        ReadOnlyRegistry.Invoking(r => policy = r.Get<Policy>(key))
            .Should().Throw<ArgumentNullException>();
        policy.Should().BeNull();
    }

    [Fact]
    public void Should_throw_when_retrieving_using_GetTResult_when_key_is_null()
    {
        string key = null!;
        Policy<ResultPrimitive>? policy = null;
        ReadOnlyRegistry.Invoking(r => policy = r.Get<Policy<ResultPrimitive>>(key))
            .Should().Throw<ArgumentNullException>();
        policy.Should().BeNull();
    }

    [Fact]
    public void Should_throw_when_retrieving_using_Get_by_interface_when_key_is_null()
    {
        string key = null!;
        ISyncPolicy<ResultPrimitive>? policy = null;
        ReadOnlyRegistry.Invoking(r => policy = r.Get<ISyncPolicy<ResultPrimitive>>(key))
            .Should().Throw<ArgumentNullException>();
        policy.Should().BeNull();
    }

    [Fact]
    public void Should_throw_when_retrieving_using_Indexer_when_key_is_null()
    {
        string key = null!;
        IsPolicy? policy = null;
        ReadOnlyRegistry.Invoking(r => policy = r[key])
            .Should().Throw<ArgumentNullException>();
        policy.Should().BeNull();
    }
    #endregion

    #region Tests for checking if key exists

    [Fact]
    public void Should_be_able_to_check_if_key_exists()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        ReadOnlyRegistry.ContainsKey(key).Should().BeTrue();

        string key2 = Guid.NewGuid().ToString();
        ReadOnlyRegistry.ContainsKey(key2).Should().BeFalse();
    }

    [Fact]
    public void Should_throw_when_checking_if_key_exists_when_key_is_null()
    {
        string key = null!;
        ReadOnlyRegistry.Invoking(r => r.ContainsKey(key))
            .Should().Throw<ArgumentNullException>();
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
            {key, policy}
        };

        testRegistry.Should().Equal(new Dictionary<string, IsPolicy>
        {
            {key, policy}
        });
    }
    #endregion
}
