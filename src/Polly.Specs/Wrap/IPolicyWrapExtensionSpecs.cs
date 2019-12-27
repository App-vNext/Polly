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
            ISyncNoOpPolicy policy0 = Policy.NoOp();
            ISyncNoOpPolicy policy1 = Policy.NoOp();
            ISyncNoOpPolicy policy2 = Policy.NoOp();

            ISyncPolicyWrap policyWrap = Policy.Wrap(policy0, policy1, policy2);

            List<IsPolicy> policies = policyWrap.GetPolicies().ToList();
            policies.Count.Should().Be(3);
            policies[0].Should().Be(policy0);
            policies[1].Should().Be(policy1);
            policies[2].Should().Be(policy2);
        }

        [Fact]
        public void Should_return_sequence_from_GetPolicies()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.NoOp();
            ISyncPolicyWrap wrap = Policy.Wrap(policyA, policyB);

            wrap.GetPolicies().Should().BeEquivalentTo(new[] { policyA, policyB },
                options => options
                    .WithStrictOrdering()
                    .Using<IsPolicy>(ctx => ctx.Subject.Should().BeSameAs(ctx.Expectation))
                    .WhenTypeIs<IsPolicy>());
        }

        [Fact]
        public void Threepolicies_by_static_sequence_should_return_correct_sequence_from_GetPolicies()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.NoOp();
            ISyncPolicy policyC = Policy.NoOp();
            ISyncPolicyWrap wrap = Policy.Wrap(policyA, policyB, policyC);

            wrap.GetPolicies().Should().BeEquivalentTo(new[] { policyA, policyB, policyC },
                options => options
                    .WithStrictOrdering()
                    .Using<IsPolicy>(ctx => ctx.Subject.Should().BeSameAs(ctx.Expectation))
                    .WhenTypeIs<IsPolicy>());
        }

        [Fact]
        public void Threepolicies_lefttree_should_return_correct_sequence_from_GetPolicies()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.NoOp();
            ISyncPolicy policyC = Policy.NoOp();
            ISyncPolicyWrap wrap = policyA.Wrap(policyB).Wrap(policyC);

            wrap.GetPolicies().Should().BeEquivalentTo(new[] { policyA, policyB, policyC },
                options => options
                    .WithStrictOrdering()
                    .Using<IsPolicy>(ctx => ctx.Subject.Should().BeSameAs(ctx.Expectation))
                    .WhenTypeIs<IsPolicy>());
        }

        [Fact]
        public void Threepolicies_righttree_should_return_correct_sequence_from_GetPolicies()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.NoOp();
            ISyncPolicy policyC = Policy.NoOp();
            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies().Should().BeEquivalentTo(new[] { policyA, policyB, policyC },
                options => options
                    .WithStrictOrdering()
                    .Using<IsPolicy>(ctx => ctx.Subject.Should().BeSameAs(ctx.Expectation))
                    .WhenTypeIs<IsPolicy>());
        }

        [Fact]
        public void GetPoliciesTPolicy_should_return_single_policy_of_type_TPolicy()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.Handle<Exception>().Retry();
            ISyncPolicy policyC = Policy.NoOp();
            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies<RetryPolicy>().Should().BeEquivalentTo(new[] { policyB },
                options => options
                    .WithStrictOrdering()
                    .Using<IsPolicy>(ctx => ctx.Subject.Should().BeSameAs(ctx.Expectation))
                    .WhenTypeIs<IsPolicy>());
        }

        [Fact]
        public void GetPoliciesTPolicy_should_return_empty_enumerable_if_no_policy_of_type_TPolicy()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.Handle<Exception>().Retry();
            ISyncPolicy policyC = Policy.NoOp();
            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies<CircuitBreakerPolicy>().Should().BeEmpty();
        }

        [Fact]
        public void GetPoliciesTPolicy_should_return_multiple_policies_of_type_TPolicy()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.Handle<Exception>().Retry();
            ISyncPolicy policyC = Policy.NoOp();
            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies<NoOpPolicy>().Should().BeEquivalentTo(new[] { policyA, policyC },
                options => options
                    .WithStrictOrdering()
                    .Using<IsPolicy>(ctx => ctx.Subject.Should().BeSameAs(ctx.Expectation))
                    .WhenTypeIs<IsPolicy>());
        }

        [Fact]
        public void GetPoliciesTPolicy_should_return_policies_of_type_TPolicy_matching_predicate()
        {
            ISyncCircuitBreakerPolicy policyA = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);
            ISyncPolicy policyB = Policy.Handle<Exception>().Retry();
            ISyncCircuitBreakerPolicy policyC = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);

            policyA.Isolate();

            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies<CircuitBreakerPolicy>(p => p.CircuitState == CircuitState.Closed).Should().BeEquivalentTo(new[] { policyC },
                options => options
                    .WithStrictOrdering()
                    .Using<IsPolicy>(ctx => ctx.Subject.Should().BeSameAs(ctx.Expectation))
                    .WhenTypeIs<IsPolicy>());
        }

        [Fact]
        public void GetPoliciesTPolicy_should_return_empty_enumerable_if_none_match_predicate()
        {
            ISyncCircuitBreakerPolicy policyA = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);
            ISyncPolicy policyB = Policy.Handle<Exception>().Retry();
            ISyncCircuitBreakerPolicy policyC = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);

            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies<CircuitBreakerPolicy>(p => p.CircuitState == CircuitState.Open).Should().BeEmpty();
        }

        [Fact]
        public void GetPoliciesTPolicy_with_predicate_should_return_multiple_policies_of_type_TPolicy_if_multiple_match_predicate()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.Handle<Exception>().Retry();
            ISyncPolicy policyC = Policy.NoOp();
            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicies<NoOpPolicy>(_ => true).Should().BeEquivalentTo(new[] { policyA, policyC },
                options => options
                    .WithStrictOrdering()
                    .Using<IsPolicy>(ctx => ctx.Subject.Should().BeSameAs(ctx.Expectation))
                    .WhenTypeIs<IsPolicy>());
        }

        [Fact]
        public void GetPoliciesTPolicy_with_predicate_should_throw_if_predicate_is_null()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.NoOp();
            ISyncPolicyWrap wrap = policyA.Wrap(policyB);

            Action configure = () => wrap.GetPolicies<NoOpPolicy>(null);
            
            configure.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("filter");
        }

        [Fact]
        public void GetPolicyTPolicy_should_return_single_policy_of_type_TPolicy()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.Handle<Exception>().Retry();
            ISyncPolicy policyC = Policy.NoOp();
            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicy<RetryPolicy>().Should().BeSameAs(policyB);
        }

        [Fact]
        public void GetPolicyTPolicy_should_return_null_if_no_TPolicy()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.Handle<Exception>().Retry();
            ISyncPolicy policyC = Policy.NoOp();
            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicy<CircuitBreakerPolicy>().Should().BeNull();
        }

        [Fact]
        public void GetPolicyTPolicy_should_throw_if_multiple_TPolicy()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.Handle<Exception>().Retry();
            ISyncPolicy policyC = Policy.NoOp();
            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.Invoking(p => p.GetPolicy<NoOpPolicy>()).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetPolicyTPolicy_should_return_single_policy_of_type_TPolicy_matching_predicate()
        {
            ISyncCircuitBreakerPolicy policyA = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);
            ISyncPolicy policyB = Policy.Handle<Exception>().Retry();
            ISyncCircuitBreakerPolicy policyC = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);

            policyA.Isolate();

            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicy<CircuitBreakerPolicy>(p => p.CircuitState == CircuitState.Closed).Should().BeSameAs(policyC);
        }

        [Fact]
        public void GetPolicyTPolicy_should_return_null_if_none_match_predicate()
        {
            ISyncCircuitBreakerPolicy policyA = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);
            ISyncPolicy policyB = Policy.Handle<Exception>().Retry();
            ISyncCircuitBreakerPolicy policyC = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);

            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.GetPolicy<CircuitBreakerPolicy>(p => p.CircuitState == CircuitState.Open).Should().BeNull();
        }

        [Fact]
        public void GetPolicyTPolicy_with_predicate_should_throw_if_multiple_TPolicy_if_multiple_match_predicate()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.Handle<Exception>().Retry();
            ISyncPolicy policyC = Policy.NoOp();
            ISyncPolicyWrap wrap = policyA.Wrap(policyB.Wrap(policyC));

            wrap.Invoking(p => p.GetPolicy<NoOpPolicy>(_ => true)).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetPolicyTPolicy_with_predicate_should_throw_if_predicate_is_null()
        {
            ISyncPolicy policyA = Policy.NoOp();
            ISyncPolicy policyB = Policy.NoOp();
            ISyncPolicyWrap wrap = policyA.Wrap(policyB);

            Action configure = () => wrap.GetPolicy<NoOpPolicy>(null);

            configure.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("filter");
        }
    }
}
