using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using Polly.Registry;
using Polly.NoOp;

namespace Polly.SharedSpecs.Registry
{
    public class DefaultPolicyRegistrySpecs
    {
        IPolicyRegistry<string, Policy> _registry;

        public DefaultPolicyRegistrySpecs()
        {
            _registry = new DefaultPolicyRegistry();
        }

        [Fact]
        public void Should_Allow_Adding_Policy()
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
        public void Should_Not_Allow_Adding_Policy_With_Duplicate_Key_Using_Add()
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
        public void Should_Allow_Retrieving_Stored_Policy()
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
        public void Should_Not_Throw_While_Retrieving_When_Key_Does_Not_Exist_Using_TryGetValue()
        {
            string key = Guid.NewGuid().ToString();
            Policy outPolicy = null;
            bool result = false;

            _registry.Invoking(r => result = r.TryGetValue(key, out outPolicy))
                .ShouldNotThrow();

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_Allow_Clearing_Registry()
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
        public void Should_Allow_Removing_Policy()
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
        public void Should_Check_If_Key_Exists()
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
        public void Should_Overwrite_Existing_Policy_When_Using_Indexer()
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
