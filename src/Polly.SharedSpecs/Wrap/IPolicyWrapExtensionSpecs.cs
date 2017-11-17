using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Polly.Wrap;
using System.Linq;
using FluentAssertions;

namespace Polly.Specs.Wrap
{
    public class IPolicyWrapExtensionSpecs
    {
        [Fact]
        public void Should_pass_outer_policy_and_then_inner_policy_from_PolicyWrap()
        {
            var outerPolicy = Policy.NoOp();
            var innerPolicy = Policy.NoOp();
            var policyWrap = Policy.Wrap(outerPolicy, innerPolicy);

            var policies = policyWrap.GetPolicies().ToList();
            policies[0].Should().Be(outerPolicy);
            policies[1].Should().Be(innerPolicy);
        }

        [Fact]
        public void Should_pass_no_policy_if_PolicyWrap_has_no_inner_or_outer_policy()
        {
            var policyWrap = new CustomWrap();

            var policies = policyWrap.GetPolicies().ToList();
            policies.Count.Should().Be(0);
        }

        [Fact]
        public void Should_pass_only_inner_policy_if_PolicyWrap_has_no_outer_policy()
        {
            var innerPolicy = Policy.NoOp();
            var policyWrap = new CustomWrap(null, innerPolicy);

            var policies = policyWrap.GetPolicies().ToList();
            policies.Count.Should().Be(1);
            policies[0].Should().Be(innerPolicy);
        }

        [Fact]
        public void Should_pass_only_outer_policy_if_PolicyWrap_has_no_inner_policy()
        {
            var outerPolicy = Policy.NoOp();
            var policyWrap = new CustomWrap(outerPolicy, null);

            var policies = policyWrap.GetPolicies().ToList();
            policies.Count.Should().Be(1);
            policies[0].Should().Be(outerPolicy);
        }

        [Fact]
        public void Should_pass_all_nested_policies_from_PolicyWrap_in_same_order_they_were_added()
        {
            var policy0 = Policy.NoOp();
            var policy1 = Policy.NoOp();
            var policy2 = Policy.NoOp();
            var policyWrap = Policy.Wrap(policy0, policy1, policy2);

            var policies = policyWrap.GetPolicies().ToList();
            policies.Count.Should().Be(3);
            policies[0].Should().Be(policy0);
            policies[1].Should().Be(policy1);
            policies[2].Should().Be(policy2);
        }

        [Fact]
        public void Should_pass_nested_policies_in_outer_inner_order()
        {
            var policy0 = Policy.NoOp();
            var policy1 = Policy.NoOp();
            var policy2 = Policy.NoOp();
            var policy3 = Policy.NoOp();
            var policyWrap = 
                new CustomWrap(
                    new CustomWrap(policy0, policy1),
                    new CustomWrap(
                        policy2,
                        new CustomWrap(null, policy3)));

            var policies = policyWrap.GetPolicies().ToList();
            policies.Count.Should().Be(4);
            policies[0].Should().Be(policy0);
            policies[1].Should().Be(policy1);
            policies[2].Should().Be(policy2);
            policies[3].Should().Be(policy3);
        }

        public class CustomWrap : IPolicyWrap
        {
            public IsPolicy Outer { get; }
            public IsPolicy Inner { get; }

            public string PolicyKey => throw new NotImplementedException();

            public CustomWrap()
            {
            }

            public CustomWrap(IsPolicy outer, IsPolicy inner)
            {
                Outer = outer;
                Inner = inner;
            }
        }
    }
}
