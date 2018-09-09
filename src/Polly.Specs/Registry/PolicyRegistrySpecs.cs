using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using Polly.Registry;
using Polly.NoOp;
using Polly.Specs.Helpers;

namespace Polly.Specs.Registry
{
    public class PolicyRegistrySpecs
    {
        IPolicyRegistry<string> _registry;

        public PolicyRegistrySpecs()
        {
            _registry = new PolicyRegistry();
        }

        #region Tests for adding Policy

        [Fact]
        public void Should_be_able_to_add_Policy_using_Add()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry.Count.Should().Be(1);

            Policy policy2 = Policy.NoOp();
            string key2 = Guid.NewGuid().ToString();

            _registry.Add(key2, policy2);
            _registry.Count.Should().Be(2);
        }

        [Fact]
        public void Should_be_able_to_add_PolicyTResult_using_Add()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry.Count.Should().Be(1);

            Policy<ResultPrimitive> policy2 = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            string key2 = Guid.NewGuid().ToString();

            _registry.Add(key2, policy2);
            _registry.Count.Should().Be(2);
        }

        [Fact]
        public void Should_be_able_to_add_Policy_by_interface_using_Add()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry.Count.Should().Be(1);

            ISyncPolicy<ResultPrimitive> policy2 = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            string key2 = Guid.NewGuid().ToString();

            _registry.Add<ISyncPolicy<ResultPrimitive>>(key2, policy2);
            _registry.Count.Should().Be(2);
        }

        [Fact]
        public void Should_be_able_to_add_Policy_using_Indexer()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry[key] = policy;
            _registry.Count.Should().Be(1);

            Policy policy2 = Policy.NoOp();
            string key2 = Guid.NewGuid().ToString();

            _registry[key2] = policy2;
            _registry.Count.Should().Be(2);
        }

        [Fact]
        public void Should_be_able_to_add_PolicyTResult_using_Indexer()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry[key] = policy;
            _registry.Count.Should().Be(1);

            Policy<ResultPrimitive> policy2 = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            string key2 = Guid.NewGuid().ToString();

            _registry[key2] = policy2;
            _registry.Count.Should().Be(2);
        }
        
        [Fact]
        public void Should_not_be_able_to_add_Policy_with_duplicate_key_using_Add()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry.Invoking(r => r.Add(key, policy))
                .ShouldNotThrow();

            _registry.Invoking(r => r.Add(key, policy))
                .ShouldThrow<ArgumentException>();

            _registry.Count.Should().Be(1);
        }

        [Fact]
        public void Should_be_able_to_overwrite_existing_Policy_if_key_exists_when_inserting_using_Indexer()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();
            _registry.Add(key, policy);

            Policy policy_new = Policy.NoOp();
            _registry[key] = policy_new;

            _registry.Count.Should().Be(1);

            _registry.Get<Policy>(key).Should().BeSameAs(policy_new);
        }

        [Fact]
        public void Should_be_able_to_overwrite_existing_PolicyTResult_if_key_exists_when_inserting_using_Indexer()
        {
            Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            string key = Guid.NewGuid().ToString();
            _registry.Add<ISyncPolicy<ResultPrimitive>>(key, policy);

            Policy<ResultPrimitive> policy_new = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            _registry[key] = policy_new;

            _registry.Count.Should().Be(1);

            _registry.Get<Policy<ResultPrimitive>>(key).Should().BeSameAs(policy_new);
        }
        
        [Fact]
        public void Should_throw_when_adding_Policy_using_Add_when_key_is_null()
        {
            string key = null;
            Policy policy = Policy.NoOp();
            _registry.Invoking(r => r.Add(key, policy))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Should_throw_when_adding_Policy_using_Indexer_when_key_is_null()
        {
            string key = null;
            Policy policy = Policy.NoOp();
            _registry.Invoking(r => r[key] = policy)
                .ShouldThrow<ArgumentNullException>();
        }

        #endregion

        #region Tests for retrieving policy

        [Fact]
        public void Should_be_able_to_retrieve_stored_Policy_using_TryGet()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();
            Policy outPolicy = null;

            _registry.Add(key, policy);
            _registry.TryGet(key, out outPolicy).Should().BeTrue();
            outPolicy.Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_be_able_to_retrieve_stored_PolicyTResult_using_TryGet()
        {
            Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            string key = Guid.NewGuid().ToString();
            Policy<ResultPrimitive> outPolicy = null;

            _registry.Add(key, policy);
            _registry.TryGet(key, out outPolicy).Should().BeTrue();
            outPolicy.Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_TryGet()
        {
            ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            string key = Guid.NewGuid().ToString();
            ISyncPolicy<ResultPrimitive> outPolicy = null;

            _registry.Add(key, policy);
            _registry.TryGet(key, out outPolicy).Should().BeTrue();
            outPolicy.Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_be_able_to_retrieve_stored_Policy_using_Get()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry.Get<Policy>(key).Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_be_able_to_retrieve_stored_PolicyTResult_using_Get()
        {
            Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry.Get<Policy<ResultPrimitive>>(key).Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_Get()
        {
            ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry.Get<ISyncPolicy<ResultPrimitive>>(key).Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_be_able_to_retrieve_stored_Policy_using_Indexer()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry[key].Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_be_able_to_retrieve_stored_PolicyTResult_using_Indexer()
        {
            Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry[key].Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_Indexer()
        {
            ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry[key].Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGet()
        {
            string key = Guid.NewGuid().ToString();
            Policy outPolicy = null;
            bool result = false;

            _registry.Invoking(r => result = r.TryGet(key, out outPolicy))
                .ShouldNotThrow();

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGetPolicyTResult()
        {
            string key = Guid.NewGuid().ToString();
            Policy<ResultPrimitive> outPolicy = null;
            bool result = false;

            _registry.Invoking(r => result = r.TryGet(key, out outPolicy))
                .ShouldNotThrow();

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGetPolicy_by_interface()
        {
            string key = Guid.NewGuid().ToString();
            ISyncPolicy<ResultPrimitive> outPolicy = null;
            bool result = false;

            _registry.Invoking(r => result = r.TryGet<ISyncPolicy<ResultPrimitive>>(key, out outPolicy))
                .ShouldNotThrow();

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_throw_while_retrieving_using_Get_when_key_does_not_exist()
        {
            string key = Guid.NewGuid().ToString();
            Policy policy = null;
            _registry.Invoking(r => policy = r.Get<Policy>(key))
                .ShouldThrow<KeyNotFoundException>();
        }

        [Fact]
        public void Should_throw_while_retrieving_using_GetTResult_when_key_does_not_exist()
        {
            string key = Guid.NewGuid().ToString();
            Policy<ResultPrimitive> policy = null;
            _registry.Invoking(r => policy = r.Get<Policy<ResultPrimitive>>(key))
                .ShouldThrow<KeyNotFoundException>();
        }

        [Fact]
        public void Should_throw_while_retrieving_using_Get_by_interface_when_key_does_not_exist()
        {
            string key = Guid.NewGuid().ToString();
            ISyncPolicy<ResultPrimitive> policy = null;
            _registry.Invoking(r => policy = r.Get<ISyncPolicy<ResultPrimitive>>(key))
                .ShouldThrow<KeyNotFoundException>();
        }

        [Fact]
        public void Should_throw_while_retrieving_when_key_does_not_exist_using_Indexer()
        {
            string key = Guid.NewGuid().ToString();
            IsPolicy outPolicy = null;
            _registry.Invoking(r => outPolicy = r[key])
                .ShouldThrow<KeyNotFoundException>();
        }


        [Fact]
        public void Should_throw_when_retrieving_using_Get_when_key_is_null()
        {
            string key = null;
            Policy policy = null;
            _registry.Invoking(r => policy = r.Get<Policy>(key))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Should_throw_when_retrieving_using_GetTResult_when_key_is_null()
        {
            string key = null;
            Policy<ResultPrimitive> policy = null;
            _registry.Invoking(r => policy = r.Get<Policy<ResultPrimitive>>(key))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Should_throw_when_retrieving_using_Get_by_interface_when_key_is_null()
        {
            string key = null;
            ISyncPolicy<ResultPrimitive> policy = null;
            _registry.Invoking(r => policy = r.Get<ISyncPolicy<ResultPrimitive>>(key))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Should_throw_when_retrieving_using_Indexer_when_key_is_null()
        {
            string key = null;
            IsPolicy policy = null;
            _registry.Invoking(r => policy = r[key])
                .ShouldThrow<ArgumentNullException>();
        }
        #endregion

        #region Tests for removing policy
        [Fact]
        public void Should_be_able_to_clear_registry()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry.Count.Should().Be(1);

            Policy policy2 = Policy.NoOp();
            string key2 = Guid.NewGuid().ToString();

            _registry.Add(key2, policy2);
            _registry.Count.Should().Be(2);

            _registry.Clear();
            _registry.Count.Should().Be(0);
        }

        [Fact]
        public void Should_be_able_to_remove_policy()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry.Count.Should().Be(1);

            _registry.Remove(key);
            _registry.Count.Should().Be(0);
        }

        [Fact]
        public void Should_throw_when_removing_Policy_when_key_is_null()
        {
            string key = null;
            _registry.Invoking(r => r.Remove(key))
                .ShouldThrow<ArgumentNullException>();
        }
        #endregion

        #region Tests for checking if key exists

        [Fact]
        public void Should_be_able_to_check_if_key_exists()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry.ContainsKey(key).Should().BeTrue();

            string key2 = Guid.NewGuid().ToString();
            _registry.ContainsKey(key2).Should().BeFalse();
        }

        [Fact]
        public void Should_throw_when_checking_if_key_exists_when_key_is_null()
        {
            string key = null;
            _registry.Invoking(r => r.ContainsKey(key))
                .ShouldThrow<ArgumentNullException>();
        }
        #endregion
    }
}
