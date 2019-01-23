using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Polly.Registry;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs.Registry
{
    public class IReadOnlyPolicyRegistrySpecs
    {
        readonly IPolicyRegistry<string> _registry;

        public IReadOnlyPolicyRegistrySpecs() => _registry = new PolicyRegistry();

        #region Tests for retrieving policy

        [Fact]
        public void Should_be_able_to_retrieve_stored_Policy_using_TryGet()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry.TryGet(key, out Policy outPolicy).Should().BeTrue();
            outPolicy.Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_be_able_to_retrieve_stored_PolicyTResult_using_TryGet()
        {
            Policy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry.TryGet(key, out Policy<ResultPrimitive> outPolicy).Should().BeTrue();
            outPolicy.Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_be_able_to_retrieve_stored_Policy_by_interface_using_TryGet()
        {
            ISyncPolicy<ResultPrimitive> policy = Policy<ResultPrimitive>.HandleResult(ResultPrimitive.Fault).Retry();
            string key = Guid.NewGuid().ToString();

            _registry.Add(key, policy);
            _registry.TryGet(key, out ISyncPolicy<ResultPrimitive> outPolicy).Should().BeTrue();
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
        
        #region Tests for the GetEnumerator method

        [Fact]
        public void Calling_The_GetEnumerator_Method_Returning_A_IEnumerator_Of_KeyValuePair_Of_String_And_IsPolicy_Calls_The_Registrys_GetEnumerator_Method()
        {
            var testDictionary = new Mock<IDictionary<string, IsPolicy>>();
            var testRegistry = new PolicyRegistry(testDictionary.Object);

            testRegistry.GetEnumerator();

            testDictionary.Verify(x => x.GetEnumerator(), Times.Once);
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

            testRegistry.Should().Equal(new KeyValuePair<string, IsPolicy>(key, policy));
        }
        #endregion
    }
}
