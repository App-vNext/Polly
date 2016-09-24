using System;
using FluentAssertions;
using Polly.Retry;
using Xunit;

namespace Polly.Specs
{
    public class PolicyKeyAsyncSpecs
    {
        [Fact]
        public void Should_be_able_fluently_to_configure_the_policy_key()
        {
            var policy = Policy.Handle<Exception>().RetryAsync().WithPolicyKey(Guid.NewGuid().ToString());

            policy.Should().BeAssignableTo<Policy>();
        }

        [Fact]
        public void PolicyKey_property_should_be_the_fluently_configured_policy_key()
        {
            const string key = "SomePolicyKey";

            var policy = Policy.Handle<Exception>().RetryAsync().WithPolicyKey(key);

            policy.PolicyKey.Should().Be(key);
        }

        [Fact]
        public void Should_not_be_able_to_configure_the_policy_key_explicitly_more_than_once()
        {
            var policy = Policy.Handle<Exception>().RetryAsync();

            Action configure = () => policy.WithPolicyKey(Guid.NewGuid().ToString());

            configure.ShouldNotThrow();

            configure.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("policyKey");
        }

        [Fact]
        public void PolicyKey_property_should_be_non_null_or_empty_if_not_explicitly_configured()
        {
            var policy = Policy.Handle<Exception>().RetryAsync();

            policy.PolicyKey.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void PolicyKey_property_should_start_with_policy_type_if_not_explicitly_configured()
        {
            var policy = Policy.Handle<Exception>().RetryAsync();
            
            policy.PolicyKey.Should().StartWith("Retry");
        }

        [Fact]
        public void PolicyKey_property_should_be_unique_for_different_instances_if_not_explicitly_configured()
        {
            var policy1 = Policy.Handle<Exception>().RetryAsync();
            var policy2 = Policy.Handle<Exception>().RetryAsync();

            policy1.PolicyKey.Should().NotBe(policy2.PolicyKey);
        }

        [Fact]
        public void PolicyKey_property_should_return_consistent_value_for_same_policy_instance_if_not_explicitly_configured()
        {
            var policy = Policy.Handle<Exception>().RetryAsync();

            var keyRetrievedFirst = policy.PolicyKey;
            var keyRetrievedSecond = policy.PolicyKey;

            keyRetrievedSecond.Should().Be(keyRetrievedFirst);
        }


        [Fact]
        public void Should_not_be_able_to_configure_the_policy_key_explicitly_after_retrieving_default_value()
        {
            var policy = Policy.Handle<Exception>().RetryAsync();

            string retrieveKeyWhenNotExplicitlyConfigured = policy.PolicyKey;

            Action configure = () => policy.WithPolicyKey(Guid.NewGuid().ToString());

            configure.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("policyKey");
        }
    }


    public class PolicyTResultKeyAsyncSpecs
    {
        [Fact]
        public void Should_be_able_fluently_to_configure_the_policy_key()
        {
            var policy = Policy.HandleResult<int>(0).RetryAsync().WithPolicyKey(Guid.NewGuid().ToString());

            policy.Should().BeAssignableTo<Policy<int>>();
        }

        [Fact]
        public void PolicyKey_property_should_be_the_fluently_configured_policy_key()
        {
            const string key = "SomePolicyKey";

            var policy = Policy.HandleResult(0).RetryAsync().WithPolicyKey(key);

            policy.PolicyKey.Should().Be(key);
        }

        [Fact]
        public void Should_not_be_able_to_configure_the_policy_key_explicitly_more_than_once()
        {
            var policy = Policy.HandleResult(0).RetryAsync();

            Action configure = () => policy.WithPolicyKey(Guid.NewGuid().ToString());

            configure.ShouldNotThrow();

            configure.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("policyKey");
        }

        [Fact]
        public void PolicyKey_property_should_be_non_null_or_empty_if_not_explicitly_configured()
        {
            var policy = Policy.HandleResult(0).RetryAsync();

            policy.PolicyKey.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void PolicyKey_property_should_start_with_policy_type_if_not_explicitly_configured()
        {
            var policy = Policy.HandleResult(0).RetryAsync();

            policy.PolicyKey.Should().StartWith("Retry");
        }

        [Fact]
        public void PolicyKey_property_should_be_unique_for_different_instances_if_not_explicitly_configured()
        {
            var policy1 = Policy.HandleResult(0).RetryAsync();
            var policy2 = Policy.HandleResult(0).RetryAsync();

            policy1.PolicyKey.Should().NotBe(policy2.PolicyKey);
        }

        [Fact]
        public void PolicyKey_property_should_return_consistent_value_for_same_policy_instance_if_not_explicitly_configured()
        {
            var policy = Policy.HandleResult(0).RetryAsync();

            var keyRetrievedFirst = policy.PolicyKey;
            var keyRetrievedSecond = policy.PolicyKey;

            keyRetrievedSecond.Should().Be(keyRetrievedFirst);
        }


        [Fact]
        public void Should_not_be_able_to_configure_the_policy_key_explicitly_after_retrieving_default_value()
        {
            var policy = Policy.HandleResult(0).RetryAsync();

            string retrieveKeyWhenNotExplicitlyConfigured = policy.PolicyKey;

            Action configure = () => policy.WithPolicyKey(Guid.NewGuid().ToString());

            configure.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("policyKey");
        }
    }
}
