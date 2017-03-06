using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using Polly.Registry;
using Polly.NoOp;

namespace Polly.Specs.Registry
{
    public class DefaultPolicyRegistrySpecs
    {
        IPolicyRegistry<string, Policy> _registry;

        public DefaultPolicyRegistrySpecs()
        {
            _registry = new DefaultPolicyRegistry();
        }

        [Fact]
        public void Should_be_able_to_add_Policy_using_Add()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r.Add(key, policy))
                .ShouldNotThrow();

            _registry.Count.Should().Be(1);

            Policy policy2 = Policy.NoOp();
            string key2 = Guid.NewGuid().ToString();
            _registry.Invoking(r => r.Add(key2, policy2))
                .ShouldNotThrow();

            _registry.Count.Should().Be(2);
        }

        [Fact]
        public void Should_be_able_to_add_Policy_using_Indexer()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r[key] = policy)
                .ShouldNotThrow();

            _registry.Count.Should().Be(1);

            Policy policy2 = Policy.NoOp();
            string key2 = Guid.NewGuid().ToString();
            _registry.Invoking(r => r[key2] = policy2)
                .ShouldNotThrow();

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
        public void Should_be_able_to_retrieve_stored_Policy_using_TryGetValue()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();
            Policy outPolicy = null;
            bool result = false;

            _registry.Add(key, policy);

            _registry.Invoking(r => result = r.TryGetValue(key, out outPolicy))
                .ShouldNotThrow<ArgumentException>();

            result.Should().BeTrue();

            outPolicy.Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_be_able_to_retrieve_stored_Policy_using_Indexer()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();
            Policy outPolicy = null;

            _registry.Add(key, policy);

            _registry.Invoking(r => outPolicy = r[key])
                .ShouldNotThrow<KeyNotFoundException>();

            outPolicy.Should().BeSameAs(policy);
        }

        [Fact]
        public void Should_not_throw_while_retrieving_when_key_does_not_exist_using_TryGetValue()
        {
            string key = Guid.NewGuid().ToString();
            Policy outPolicy = null;
            bool result = false;

            _registry.Invoking(r => result = r.TryGetValue(key, out outPolicy))
                .ShouldNotThrow();

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_throw_while_retrieving_using_indexer_when_key_does_not_exist()
        {
            string key = Guid.NewGuid().ToString();
            Policy policy = null;
            _registry.Invoking(r => policy = r[key])
                .ShouldThrow<KeyNotFoundException>();
        }

        [Fact]
        public void Should_be_able_to_overwrite_existing_Policy_if_key_exists_when_inserting_using_Idexer()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry.Invoking(r => r.Add(key, policy))
                .ShouldNotThrow();

            Policy policy_new = Policy.NoOp();

            _registry.Invoking(r => r[key] = policy_new)
                .ShouldNotThrow();

            _registry.Count.Should().Be(1);

            Policy output = null;
            _registry.Invoking(r => output = r[key])
                .ShouldNotThrow();
            output.Should().BeSameAs(policy_new);
        }

        [Fact]
        public void Should_be_able_to_clear_registry()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r.Add(key, policy))
                .ShouldNotThrow();

            _registry.Count.Should().Be(1);

            Policy policy2 = Policy.NoOp();
            string key2 = Guid.NewGuid().ToString();
            _registry.Invoking(r => r.Add(key2, policy2))
                .ShouldNotThrow();

            _registry.Count.Should().Be(2);

            _registry.Invoking(r => r.Clear())
                .ShouldNotThrow();

            _registry.Count.Should().Be(0);
        }

        [Fact]
        public void Should_be_able_to_remove_policy()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r.Add(key, policy))
                .ShouldNotThrow();

            _registry.Count.Should().Be(1);

            _registry.Invoking(r => r.Remove(key))
                .ShouldNotThrow();

            _registry.Count.Should().Be(0);
        }

        [Fact]
        public void Should_be_able_to_check_if_key_exists()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r.Add(key, policy))
                .ShouldNotThrow();

            bool result = false;
            _registry.Invoking(r => result = r.ContainsKey(key))
                .ShouldNotThrow();

            result.Should().BeTrue();

            string key2 = Guid.NewGuid().ToString();

            _registry.Invoking(r => result = r.ContainsKey(key2))
                .ShouldNotThrow();

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_throw_when_retrieving_using_indexer_when_key_is_null()
        {
            string key = null;
            Policy policy = null;
            _registry.Invoking(r => policy = r[key])
                .ShouldThrow<ArgumentNullException>();
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

        [Fact]
        public void Should_throw_when_removing_Policy_when_key_is_null()
        {
            string key = null;
            _registry.Invoking(r => r.Remove(key))
                .ShouldThrow<ArgumentNullException>();
        }
    }
}
