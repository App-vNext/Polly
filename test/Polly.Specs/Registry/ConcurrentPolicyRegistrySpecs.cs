namespace Polly.Specs.Registry;

public class ConcurrentPolicyRegistrySpecs
{
    private readonly IConcurrentPolicyRegistry<string> _registry;

    public ConcurrentPolicyRegistrySpecs() =>
        _registry = new PolicyRegistry();

    [Fact]
    public void Should_be_able_to_add_Policy_using_TryAdd()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        var insert = _registry.TryAdd(key, policy);
        _registry.Count.Should().Be(1);
        insert.Should().Be(true);

        Policy policy2 = Policy.NoOp();
        string key2 = Guid.NewGuid().ToString();

        var insert2 = _registry.TryAdd(key2, policy2);
        _registry.Count.Should().Be(2);
        insert2.Should().Be(true);
    }

    [Fact]
    public void Should_be_able_to_add_PolicyTResult_using_TryAdd()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        var insert = _registry.TryAdd(key, policy);
        _registry.Count.Should().Be(1);
        insert.Should().Be(true);

        Policy<ResultPrimitive> policy2 = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key2 = Guid.NewGuid().ToString();

        var insert2 = _registry.TryAdd(key2, policy2);
        _registry.Count.Should().Be(2);
        insert2.Should().Be(true);
    }

    [Fact]
    public void Should_be_able_to_add_Policy_by_interface_using_TryAdd()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        var insert = _registry.TryAdd(key, policy);
        _registry.Count.Should().Be(1);
        insert.Should().Be(true);

        ISyncPolicy<ResultPrimitive> policy2 = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
        string key2 = Guid.NewGuid().ToString();

        var insert2 = _registry.TryAdd(key2, policy2);
        _registry.Count.Should().Be(2);
        insert2.Should().Be(true);
    }

    [Fact]
    public void Should_be_able_to_remove_policy_with_TryRemove()
    {
        Policy policy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        _registry.Add(key, policy);
        _registry.Count.Should().Be(1);

        bool removed = _registry.TryRemove(key, out IsPolicy removedPolicy);
        _registry.Count.Should().Be(0);
        removedPolicy.Should().BeSameAs(policy);
        removed.Should().BeTrue();
    }

    [Fact]
    public void Should_report_false_from_TryRemove_if_no_Policy()
    {
        string key = Guid.NewGuid().ToString();

        bool removed = _registry.TryRemove(key, out IsPolicy removedPolicy);
        removed.Should().BeFalse();
    }

    [Fact]
    public void Should_be_able_to_update_policy_with_TryUpdate()
    {
        Policy existingPolicy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();
        _registry.Add(key, existingPolicy);

        Policy<ResultClass> newPolicy = Policy.NoOp<ResultClass>();

        bool updated = _registry.TryUpdate<IsPolicy>(key, newPolicy, existingPolicy);

        updated.Should().BeTrue();
        _registry[key].Should().BeSameAs(newPolicy);
    }

    [Fact]
    public void Should_not_update_policy_with_TryUpdate_when_existingPolicy_mismatch()
    {
        Policy existingPolicy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();
        _registry.Add(key, existingPolicy);

        NoOpPolicy<ResultPrimitive> someOtherPolicy = Policy.NoOp<ResultPrimitive>();
        Policy<ResultClass> newPolicy = Policy.NoOp<ResultClass>();

        bool updated = _registry.TryUpdate<IsPolicy>(key, newPolicy, someOtherPolicy);

        updated.Should().BeFalse();
        _registry[key].Should().BeSameAs(existingPolicy);
    }

    [Fact]
    public void Should_not_update_policy_with_TryUpdate_when_no_existing_value()
    {
        string key = Guid.NewGuid().ToString();

        NoOpPolicy<ResultPrimitive> someOtherPolicy = Policy.NoOp<ResultPrimitive>();
        Policy<ResultClass> newPolicy = Policy.NoOp<ResultClass>();

        bool updated = _registry.TryUpdate<IsPolicy>(key, newPolicy, someOtherPolicy);

        updated.Should().BeFalse();
        _registry.ContainsKey(key).Should().BeFalse();
    }

    [Fact]
    public void Should_add_with_GetOrAdd_with_value_when_no_existing_policy()
    {
        Policy newPolicy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        var returnedPolicy = _registry.GetOrAdd(key, newPolicy);

        returnedPolicy.Should().BeSameAs(newPolicy);
    }

    [Fact]
    public void Should_add_with_GetOrAdd_with_factory_when_no_existing_policy()
    {
        Policy newPolicy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        var returnedPolicy = _registry.GetOrAdd(key, _ => newPolicy);

        returnedPolicy.Should().BeSameAs(newPolicy);
    }

    [Fact]
    public void Should_return_existing_with_GetOrAdd_with_value_when_existing_policy()
    {
        Policy existingPolicy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();
        _registry.Add(key, existingPolicy);

        Policy newPolicy = Policy.NoOp();

        var returnedPolicy = _registry.GetOrAdd(key, newPolicy);

        returnedPolicy.Should().BeSameAs(existingPolicy);
    }

    [Fact]
    public void Should_return_existing_with_GetOrAdd_with_factory_when_existing_policy()
    {
        Policy existingPolicy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();
        _registry.Add(key, existingPolicy);

        Policy newPolicy = Policy.NoOp();

        var returnedPolicy = _registry.GetOrAdd(key, _ => newPolicy);

        returnedPolicy.Should().BeSameAs(existingPolicy);
    }

    [Fact]
    public void Should_add_with_AddOrUpdate_with_value_when_no_existing_policy()
    {
        Policy newPolicy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        var returnedPolicy = _registry.AddOrUpdate(
            key,
            newPolicy,
            (_, _) => throw new InvalidOperationException("Update factory should not be called in this test."));

        returnedPolicy.Should().BeSameAs(newPolicy);
    }

    [Fact]
    public void Should_add_with_AddOrUpdate_with_addfactory_when_no_existing_policy()
    {
        Policy newPolicy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();

        var returnedPolicy = _registry.AddOrUpdate(
            key,
            _ => newPolicy,
            (_, _) => throw new InvalidOperationException("Update factory should not be called in this test."));

        returnedPolicy.Should().BeSameAs(newPolicy);
    }

    [Fact]
    public void Should_update_with_AddOrUpdate_with_updatefactory_ignoring_addvalue_when_existing_policy()
    {
        Policy existingPolicy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();
        _registry.Add(key, existingPolicy);

        const string PolicyKeyToDecorate = "SomePolicyKey";

        Policy otherPolicyNotExpectingToAdd = Policy.Handle<Exception>().Retry();

        var returnedPolicy = _registry.AddOrUpdate(
            key,
            otherPolicyNotExpectingToAdd,
            (_, _) => existingPolicy.WithPolicyKey(PolicyKeyToDecorate));

        returnedPolicy.Should().NotBeSameAs(otherPolicyNotExpectingToAdd);
        returnedPolicy.Should().BeSameAs(existingPolicy);
        returnedPolicy.PolicyKey.Should().Be(PolicyKeyToDecorate);
    }

    [Fact]
    public void Should_update_with_AddOrUpdate_with_updatefactory_ignoring_addfactory_when_existing_policy()
    {
        Policy existingPolicy = Policy.NoOp();
        string key = Guid.NewGuid().ToString();
        _registry.Add(key, existingPolicy);

        const string PolicyKeyToDecorate = "SomePolicyKey";

        Policy otherPolicyNotExpectingToAdd = Policy.Handle<Exception>().Retry();

        var returnedPolicy = _registry.AddOrUpdate(
            key,
            _ => otherPolicyNotExpectingToAdd,
            (_, _) => existingPolicy.WithPolicyKey(PolicyKeyToDecorate));

        returnedPolicy.Should().NotBeSameAs(otherPolicyNotExpectingToAdd);
        returnedPolicy.Should().BeSameAs(existingPolicy);
        returnedPolicy.PolicyKey.Should().Be(PolicyKeyToDecorate);
    }
}
