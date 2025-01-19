﻿namespace Polly.Specs;

public class PolicyKeyAsyncSpecs
{
    #region Configuration

    [Fact]
    public void Should_be_able_fluently_to_configure_the_policy_key()
    {
        var policy = Policy.Handle<Exception>().RetryAsync().WithPolicyKey(Guid.NewGuid().ToString());

        policy.ShouldBeAssignableTo<AsyncPolicy>();
    }

    [Fact]
    public void Should_be_able_fluently_to_configure_the_policy_key_via_interface()
    {
        IAsyncPolicy policyAsInterface = Policy.Handle<Exception>().RetryAsync();
        var policyAsInterfaceAfterWithPolicyKey = policyAsInterface.WithPolicyKey(Guid.NewGuid().ToString());

        policyAsInterfaceAfterWithPolicyKey.ShouldBeAssignableTo<IAsyncPolicy>();
    }

    [Fact]
    public void PolicyKey_property_should_be_the_fluently_configured_policy_key()
    {
        const string Key = "SomePolicyKey";

        var policy = Policy.Handle<Exception>().RetryAsync().WithPolicyKey(Key);

        policy.PolicyKey.ShouldBe(Key);
    }

    [Fact]
    public void Should_not_be_able_to_configure_the_policy_key_explicitly_more_than_once()
    {
        var policy = Policy.Handle<Exception>().RetryAsync();

        Action configure = () => policy.WithPolicyKey(Guid.NewGuid().ToString());

        Should.NotThrow(configure);

        Should.Throw<ArgumentException>(configure).ParamName.ShouldBe("policyKey");
    }

    [Fact]
    public void Should_not_be_able_to_configure_the_policy_key_explicitly_more_than_once_via_interface()
    {
        IAsyncPolicy policyAsInterface = Policy.Handle<Exception>().RetryAsync();

        Action configure = () => policyAsInterface.WithPolicyKey(Guid.NewGuid().ToString());

        Should.NotThrow(configure);

        Should.Throw<ArgumentException>(configure).ParamName.ShouldBe("policyKey");
    }

    [Fact]
    public void PolicyKey_property_should_be_non_null_or_empty_if_not_explicitly_configured()
    {
        var policy = Policy.Handle<Exception>().RetryAsync();

        policy.PolicyKey.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void PolicyKey_property_should_start_with_policy_type_if_not_explicitly_configured()
    {
        var policy = Policy.Handle<Exception>().RetryAsync();

        policy.PolicyKey.ShouldStartWith("AsyncRetry");
    }

    [Fact]
    public void PolicyKey_property_should_be_unique_for_different_instances_if_not_explicitly_configured()
    {
        var policy1 = Policy.Handle<Exception>().RetryAsync();
        var policy2 = Policy.Handle<Exception>().RetryAsync();

        policy1.PolicyKey.ShouldNotBe(policy2.PolicyKey);
    }

    [Fact]
    public void PolicyKey_property_should_return_consistent_value_for_same_policy_instance_if_not_explicitly_configured()
    {
        var policy = Policy.Handle<Exception>().RetryAsync();

        var keyRetrievedFirst = policy.PolicyKey;
        var keyRetrievedSecond = policy.PolicyKey;

        keyRetrievedSecond.ShouldBe(keyRetrievedFirst);
    }

    [Fact]
    public void Should_not_be_able_to_configure_the_policy_key_explicitly_after_retrieving_default_value()
    {
        var policy = Policy.Handle<Exception>().RetryAsync();

        string retrieveKeyWhenNotExplicitlyConfigured = policy.PolicyKey;

        Action configure = () => policy.WithPolicyKey(Guid.NewGuid().ToString());

        Should.Throw<ArgumentException>(configure).ParamName.ShouldBe("policyKey");
    }

    [Fact]
    public void Should_throw_when_onretry_is_null()
    {
        var policyBuilder = Policy.Handle<Exception>();
        Assert.Throws<ArgumentNullException>("onRetry", () => policyBuilder.RetryAsync(3, default(Action<Exception, int, Context>)));
    }

    #endregion

    #region PolicyKey and execution Context tests

    [Fact]
    public async Task Should_pass_PolicyKey_to_execution_context()
    {
        string policyKey = Guid.NewGuid().ToString();

        string? policyKeySetOnExecutionContext = null;
        Action<Exception, int, Context> onRetry = (_, _, context) => { policyKeySetOnExecutionContext = context.PolicyKey; };
        var retry = Policy.Handle<Exception>().RetryAsync(1, onRetry).WithPolicyKey(policyKey);

        await retry.RaiseExceptionAsync<Exception>(1);

        policyKeySetOnExecutionContext.ShouldBe(policyKey);
    }

    [Fact]
    public async Task Should_pass_OperationKey_to_execution_context()
    {
        string operationKey = "SomeKey";

        string? operationKeySetOnContext = null;
        Action<Exception, int, Context> onRetry = (_, _, context) => { operationKeySetOnContext = context.OperationKey; };
        var retry = Policy.Handle<Exception>().RetryAsync(1, onRetry);

        bool firstExecution = true;
        await retry.ExecuteAsync(async _ =>
        {
            await TaskHelper.EmptyTask;
            if (firstExecution)
            {
                firstExecution = false;
                throw new Exception();
            }
        }, new Context(operationKey));

        operationKeySetOnContext.ShouldBe(operationKey);
    }

    [Fact]
    public async Task Should_pass_PolicyKey_to_execution_context_in_generic_execution_on_non_generic_policy()
    {
        string policyKey = Guid.NewGuid().ToString();

        string? policyKeySetOnExecutionContext = null;
        Action<Exception, int, Context> onRetry = (_, _, context) => { policyKeySetOnExecutionContext = context.PolicyKey; };
        var retry = Policy.Handle<Exception>().RetryAsync(1, onRetry).WithPolicyKey(policyKey);

        bool firstExecution = true;
        await retry.ExecuteAsync<int>(async () =>
        {
            await TaskHelper.EmptyTask;
            if (firstExecution)
            {
                firstExecution = false;
                throw new Exception();
            }

            return 0;
        });

        policyKeySetOnExecutionContext.ShouldBe(policyKey);
    }

    [Fact]
    public async Task Should_pass_OperationKey_to_execution_context_in_generic_execution_on_non_generic_policy()
    {
        string operationKey = "SomeKey";

        string? operationKeySetOnContext = null;
        Action<Exception, int, Context> onRetry = (_, _, context) => { operationKeySetOnContext = context.OperationKey; };
        var retry = Policy.Handle<Exception>().RetryAsync(1, onRetry);

        bool firstExecution = true;
        await retry.ExecuteAsync(async _ =>
        {
            await TaskHelper.EmptyTask;
            if (firstExecution)
            {
                firstExecution = false;
                throw new Exception();
            }

            return 0;
        }, new Context(operationKey));

        operationKeySetOnContext.ShouldBe(operationKey);
    }
    #endregion
}
