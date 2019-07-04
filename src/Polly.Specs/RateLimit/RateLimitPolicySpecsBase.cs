using System;
using FluentAssertions;
using Polly.RateLimit;
using Xunit;

namespace Polly.Specs.RateLimit
{
    public abstract class RateLimitPolicySpecsBase
    {
        public abstract IRateLimitPolicy GetPolicyViaSyntax(
            int numberOfExecutions,
            TimeSpan perTimeSpan);

        public abstract IRateLimitPolicy GetPolicyViaSyntax(
            int numberOfExecutions,
            TimeSpan perTimeSpan,
            int maxBurst);

        [Fact]
        public void Syntax_should_throw_for_numberOfExecutions_negative()
        {
            Action invalidSyntax = () => GetPolicyViaSyntax(-1, TimeSpan.FromSeconds(1));

            invalidSyntax.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("numberOfExecutions");
        }

        [Fact]
        public void Syntax_should_throw_for_numberOfExecutions_zero()
        {
            Action invalidSyntax = () => GetPolicyViaSyntax(0, TimeSpan.FromSeconds(1));

            invalidSyntax.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("numberOfExecutions");
        }

        [Fact]
        public void Syntax_should_throw_for_perTimeSpan_negative()
        {
            Action invalidSyntax = () => GetPolicyViaSyntax(1, TimeSpan.FromTicks(-1));

            invalidSyntax.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("perTimeSpan");
        }

        [Fact]
        public void Syntax_should_throw_for_perTimeSpan_zero()
        {
            Action invalidSyntax = () => GetPolicyViaSyntax(1, TimeSpan.Zero);

            invalidSyntax.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("perTimeSpan");
        }

        [Fact]
        public void Syntax_should_throw_for_maxBurst_negative()
        {
            Action invalidSyntax = () => GetPolicyViaSyntax(1, TimeSpan.FromSeconds(1), -1);

            invalidSyntax.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("maxBurst");
        }

        [Fact]
        public void Syntax_should_throw_for_maxBurst_zero()
        {
            Action invalidSyntax = () => GetPolicyViaSyntax(1, TimeSpan.FromSeconds(1), 0);

            invalidSyntax.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("maxBurst");
        }
    }
}
