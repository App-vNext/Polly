using System;
using System.Collections.Generic;
using Xunit;
using Polly.Wrap;
using System.Linq;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.NoOp;
using Polly.Retry;

namespace Polly.Specs.Wrap
{
    public class IPolicyWrapExtensionSpecs
    {

        [Fact]
        public void Should_pass_all_nested_policies_from_PolicyWrap_in_same_order_they_were_added()
        {
            NoOpPolicy policy0 = Policy.NoOp();
            NoOpPolicy policy1 = Policy.NoOp();
            NoOpPolicy policy2 = Policy.NoOp();

            PolicyWrap policyWrap = Policy.Wrap(policy0, policy1, policy2);

            List<IsPolicy> policies = policyWrap.GetPolicies().ToList();
            policies.Count.Should().Be(3);
            policies[0].Should().Be(policy0);
            policies[1].Should().Be(policy1);
            policies[2].Should().Be(policy2);
        }

        [Fact]
        public void Should_return_sequence_from_GetPolicies()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.NoOp();
            PolicyWrap wrap = Policy.Wrap(policyA, policyB);

            wrap.GetPolicies().ShouldBeEquivalentTo(new[] { policyA, policyB }, options => options.WithStrictOrdering());
        }

        [Fact]
        public void Threepolicies_by_static_sequence_should_return_correct_sequence_from_GetPolicies()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.NoOp();
            Policy policyC = Policy.NoOp();
            PolicyWrap wrap = Policy.Wrap(policyA, policyB, policyC);

            wrap.GetPolicies().ShouldBeEquivalentTo(new[] { policyA, policyB, policyC }, options => options.WithStrictOrdering());
        }

        [Fact]
        public void Threepolicies_lefttree_should_return_correct_sequence_from_GetPolicies()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.NoOp();
            Policy policyC = Policy.NoOp();
            PolicyWrap wrap = policyA.Wrap(policyB).Wrap(policyC);

            wrap.GetPolicies().ShouldBeEquivalentTo(new[] { policyA, policyB, policyC }, options => options.WithStrictOrdering());
        }

        [Fact]
        public void Threepolicies_righttree_should_return_correct_sequence_from_GetPolicies()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.NoOp();
            Policy policyC = Policy.NoOp();
            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies().ShouldBeEquivalentTo(new[] { policyA, policyB, policyC }, options => options.WithStrictOrdering());
        }

        [Fact]
        public void GetPoliciesTPolicy_should_return_single_policy_of_type_TPolicy()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.Handle<Exception>().Retry();
            Policy policyC = Policy.NoOp();
            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies<RetryPolicy>().ShouldBeEquivalentTo(new[] { policyB }, options => options.WithStrictOrdering());
        }

        [Fact]
        public void GetPoliciesTPolicy_should_return_empty_enumerable_if_no_policy_of_type_TPolicy()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.Handle<Exception>().Retry();
            Policy policyC = Policy.NoOp();
            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies<CircuitBreakerPolicy>().Should().BeEmpty();
        }

        [Fact]
        public void GetPoliciesTPolicy_should_return_multiple_policies_of_type_TPolicy()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.Handle<Exception>().Retry();
            Policy policyC = Policy.NoOp();
            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies<NoOpPolicy>().ShouldBeEquivalentTo(new[] { policyA, policyC }, options => options.WithStrictOrdering());
        }

        [Fact]
        public void GetPoliciesTPolicy_should_return_policies_of_type_TPolicy_matching_predicate()
        {
            CircuitBreakerPolicy policyA = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);
            Policy policyB = Policy.Handle<Exception>().Retry();
            CircuitBreakerPolicy policyC = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);

            policyA.Isolate();

            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies<CircuitBreakerPolicy>(p => p.CircuitState == CircuitState.Closed).ShouldBeEquivalentTo(new[] { policyC }, options => options.WithStrictOrdering());
        }

        [Fact]
        public void GetPoliciesTPolicy_should_return_empty_enumerable_if_none_match_predicate()
        {
            CircuitBreakerPolicy policyA = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);
            Policy policyB = Policy.Handle<Exception>().Retry();
            CircuitBreakerPolicy policyC = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);

            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies<CircuitBreakerPolicy>(p => p.CircuitState == CircuitState.Open).Should().BeEmpty();
        }

        [Fact]
        public void GetPoliciesTPolicy_with_predicate_should_return_multiple_policies_of_type_TPolicy_if_multiple_match_predicate()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.Handle<Exception>().Retry();
            Policy policyC = Policy.NoOp();
            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies<NoOpPolicy>(_ => true).ShouldBeEquivalentTo(new[] { policyA, policyC }, options => options.WithStrictOrdering());
        }

        [Fact]
        public void GetPoliciesTPolicy_with_predicate_should_throw_if_predicate_is_null()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.NoOp();
            PolicyWrap wrap = policyA.Wrap(policyB);

            Action configure = () => wrap.GetPolicies<NoOpPolicy>(null);
            
            configure.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("filter");
        }

        [Fact]
        public void GetPolicyTPolicy_should_return_single_policy_of_type_TPolicy()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.Handle<Exception>().Retry();
            Policy policyC = Policy.NoOp();
            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicy<RetryPolicy>().Should().BeSameAs(policyB);
        }

        [Fact]
        public void GetPolicyTPolicy_should_return_null_if_no_TPolicy()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.Handle<Exception>().Retry();
            Policy policyC = Policy.NoOp();
            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicy<CircuitBreakerPolicy>().Should().BeNull();
        }

        [Fact]
        public void GetPolicyTPolicy_should_throw_if_multiple_TPolicy()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.Handle<Exception>().Retry();
            Policy policyC = Policy.NoOp();
            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.Invoking(p => p.GetPolicy<NoOpPolicy>()).ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void GetPolicyTPolicy_should_return_single_policy_of_type_TPolicy_matching_predicate()
        {
            CircuitBreakerPolicy policyA = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);
            Policy policyB = Policy.Handle<Exception>().Retry();
            CircuitBreakerPolicy policyC = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);

            policyA.Isolate();

            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicy<CircuitBreakerPolicy>(p => p.CircuitState == CircuitState.Closed).Should().BeSameAs(policyC);
        }

        [Fact]
        public void GetPolicyTPolicy_should_return_null_if_none_match_predicate()
        {
            CircuitBreakerPolicy policyA = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);
            Policy policyB = Policy.Handle<Exception>().Retry();
            CircuitBreakerPolicy policyC = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);

            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicy<CircuitBreakerPolicy>(p => p.CircuitState == CircuitState.Open).Should().BeNull();
        }

        [Fact]
        public void GetPolicyTPolicy_with_predicate_should_throw_if_multiple_TPolicy_if_multiple_match_predicate()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.Handle<Exception>().Retry();
            Policy policyC = Policy.NoOp();
            PolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.Invoking(p => p.GetPolicy<NoOpPolicy>(_ => true)).ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void GetPolicyTPolicy_with_predicate_should_throw_if_predicate_is_null()
        {
            Policy policyA = Policy.NoOp();
            Policy policyB = Policy.NoOp();
            PolicyWrap wrap = policyA.Wrap(policyB);

            Action configure = () => wrap.GetPolicy<NoOpPolicy>(null);

            configure.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("filter");
        }
    }
}
