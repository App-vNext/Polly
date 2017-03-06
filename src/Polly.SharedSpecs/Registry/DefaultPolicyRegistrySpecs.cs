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

            policy = Policy.NoOp();
            key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r.Add(key, policy))
                .ShouldNotThrow();

            _registry.Count.Should().Be(2);

            //Using Indexer

            policy = Policy.NoOp();
            key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r[key] = policy)
                .ShouldNotThrow();

            _registry.Count.Should().Be(3);

            policy = Policy.NoOp();
            key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r[key] = policy)
                .ShouldNotThrow();

            _registry.Count.Should().Be(4);
        }

        [Fact]
        public void Should_be_able_to_add_Policy_using_Indexer()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r[key] = policy)
                .ShouldNotThrow();

            _registry.Count.Should().Be(1);

            policy = Policy.NoOp();
            key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r[key] = policy)
                .ShouldNotThrow();

            _registry.Count.Should().Be(2);
        }

        [Fact]
        public void Should_not_be_able_to_add_Policy_with_duplicate_Key_using_Add()
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

            outPolicy.ShouldBeEquivalentTo(policy);

            //indexer
            outPolicy = null;
            _registry.Invoking(r => outPolicy = r[key])
                .ShouldNotThrow<KeyNotFoundException>();

            outPolicy.ShouldBeEquivalentTo(policy);
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

            outPolicy.ShouldBeEquivalentTo(policy);
        }

        [Fact]
        public void Should_not_throw_while_retrieving_when_Key_does_not_exist_using_TryGetValue()
        {
            string key = Guid.NewGuid().ToString();
            Policy outPolicy = null;
            bool result = false;

            _registry.Invoking(r => result = r.TryGetValue(key, out outPolicy))
                .ShouldNotThrow();

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_be_able_to_overwrite_existing_Policy_if_Key_exists_when_inserting_using_Idexer()
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
            output.ShouldBeEquivalentTo(policy_new);
        }

        [Fact]
        public void Should_be_able_to_clear_registry()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r.Add(key, policy))
                .ShouldNotThrow();

            _registry.Count.Should().Be(1);

            policy = Policy.NoOp();
            key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r.Add(key, policy))
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
        public void Should_be_able_to_check_if_Key_exists()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();
            _registry.Invoking(r => r.Add(key, policy))
                .ShouldNotThrow();

            bool result = false;
            _registry.Invoking(r => result = r.ContainsKey(key))
                .ShouldNotThrow();

            result.Should().BeTrue();

            key = Guid.NewGuid().ToString();

            _registry.Invoking(r => result = r.ContainsKey(key))
                .ShouldNotThrow();

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_be_able_to_overwrite_existing_Policy_when_using_Indexer()
        {
            Policy policy = Policy.NoOp();
            string key = Guid.NewGuid().ToString();

            _registry.Invoking(r => r[key] = policy)
                .ShouldNotThrow();

            _registry.Invoking(r => r[key] = policy)
                .ShouldNotThrow();

            _registry.Count.Should().Be(1);
        }
    }
}
