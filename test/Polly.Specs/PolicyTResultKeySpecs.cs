namespace Polly.Specs;

public class PolicyTResultKeySpecs
{
    #region Configuration

    [Fact]
    public void Should_be_able_fluently_to_configure_the_policy_key()
    {
        var policy = Policy.HandleResult<int>(0).Retry().WithPolicyKey(Guid.NewGuid().ToString());

        policy.ShouldBeAssignableTo<Policy<int>>();
    }

    [Fact]
    public void Should_be_able_fluently_to_configure_the_policy_key_via_interface()
    {
        ISyncPolicy<int> policyAsInterface = Policy.HandleResult<int>(0).Retry();
        var policyAsInterfaceAfterWithPolicyKey = policyAsInterface.WithPolicyKey(Guid.NewGuid().ToString());

        policyAsInterfaceAfterWithPolicyKey.ShouldBeAssignableTo<ISyncPolicy<int>>();
    }

    [Fact]
    public void PolicyKey_property_should_be_the_fluently_configured_policy_key()
    {
        const string Key = "SomePolicyKey";

        var policy = Policy.HandleResult(0).Retry().WithPolicyKey(Key);

        policy.PolicyKey.ShouldBe(Key);
    }

    [Fact]
    public void Should_not_be_able_to_configure_the_policy_key_explicitly_more_than_once()
    {
        var policy = Policy.HandleResult(0).Retry();

        Action configure = () => policy.WithPolicyKey(Guid.NewGuid().ToString());

        Should.NotThrow(configure);

        Should.Throw<ArgumentException>(configure).ParamName.ShouldBe("policyKey");
    }

    [Fact]
    public void PolicyKey_property_should_be_non_null_or_empty_if_not_explicitly_configured()
    {
        var policy = Policy.HandleResult(0).Retry();

        policy.PolicyKey.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void PolicyKey_property_should_start_with_policy_type_if_not_explicitly_configured()
    {
        var policy = Policy.HandleResult(0).Retry();

        policy.PolicyKey.ShouldStartWith("Retry");
    }

    [Fact]
    public void PolicyKey_property_should_be_unique_for_different_instances_if_not_explicitly_configured()
    {
        var policy1 = Policy.HandleResult(0).Retry();
        var policy2 = Policy.HandleResult(0).Retry();

        policy1.PolicyKey.ShouldNotBe(policy2.PolicyKey);
    }

    [Fact]
    public void PolicyKey_property_should_return_consistent_value_for_same_policy_instance_if_not_explicitly_configured()
    {
        var policy = Policy.HandleResult(0).Retry();

        var keyRetrievedFirst = policy.PolicyKey;
        var keyRetrievedSecond = policy.PolicyKey;

        keyRetrievedSecond.ShouldBe(keyRetrievedFirst);
    }

    [Fact]
    public void Should_not_be_able_to_configure_the_policy_key_explicitly_after_retrieving_default_value()
    {
        var policy = Policy.HandleResult(0).Retry();

        string retrieveKeyWhenNotExplicitlyConfigured = policy.PolicyKey;

        Action configure = () => policy.WithPolicyKey(Guid.NewGuid().ToString());

        Should.Throw<ArgumentException>(configure).ParamName.ShouldBe("policyKey");
    }

    #endregion

    #region PolicyKey and execution Context tests

    [Fact]
    public void Should_pass_PolicyKey_to_execution_context()
    {
        string policyKey = Guid.NewGuid().ToString();

        string? policyKeySetOnExecutionContext = null;
        Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = (_, _, context) => { policyKeySetOnExecutionContext = context.PolicyKey; };
        var retry = Policy.HandleResult(ResultPrimitive.Fault).Retry(1, onRetry).WithPolicyKey(policyKey);

        retry.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Good);

        policyKeySetOnExecutionContext.ShouldBe(policyKey);
    }

    [Fact]
    public void Should_pass_OperationKey_to_execution_context()
    {
        string operationKey = "SomeKey";

        string? operationKeySetOnContext = null;
        Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = (_, _, context) => { operationKeySetOnContext = context.OperationKey; };
        var retry = Policy.HandleResult(ResultPrimitive.Fault).Retry(1, onRetry);

        bool firstExecution = true;
        retry.Execute(_ =>
        {
            if (firstExecution)
            {
                firstExecution = false;
                return ResultPrimitive.Fault;
            }

            return ResultPrimitive.Good;
        }, new Context(operationKey));

        operationKeySetOnContext.ShouldBe(operationKey);
    }

    #endregion
}
