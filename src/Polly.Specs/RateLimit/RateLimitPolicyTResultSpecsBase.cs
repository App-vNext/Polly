using System;
using FluentAssertions;
using Polly.RateLimit;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.RateLimit;
using Xunit;

namespace Polly.Specs.RateLimit
{
    public abstract class RateLimitPolicyTResultSpecsBase : RateLimitPolicySpecsBase
    {
        protected abstract IRateLimitPolicy<TResult> GetPolicyViaSyntax<TResult>(
            int numberOfExecutions,
            TimeSpan perTimeSpan,
            int maxBurst,
            Func<TimeSpan, Context, TResult> retryAfterFactory);

        protected abstract TResult TryExecuteThroughPolicy<TResult>(IRateLimitPolicy<TResult> policy, TResult resultIfExecutionPermitted);

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        public void Ratelimiter_specifies_correct_wait_until_next_execution_by_custom_factory(int onePerSeconds)
        {
            FixClock();

            // Arrange
            TimeSpan onePer = TimeSpan.FromSeconds(onePerSeconds);
            Func<TimeSpan, Context, ResultClassWithRetryAfter> retryAfterFactory = (t, ctx) => new ResultClassWithRetryAfter(t);
            var rateLimiter = GetPolicyViaSyntax<ResultClassWithRetryAfter>(1, onePer, 1, retryAfterFactory);

            // Assert - first execution after initialising should always be permitted.
            ShouldPermitAnExecution(rateLimiter);

            // Arrange
            // (do nothing - time not advanced)

            // Act - try another execution.
            var resultExpectedBlocked = TryExecuteThroughPolicy(rateLimiter, new ResultClassWithRetryAfter(ResultPrimitive.Good));

            // Assert - should be blocked - time not advanced. Result should be expressed per the retryAfterFactory.
            resultExpectedBlocked.ResultCode.Should().NotBe(ResultPrimitive.Good);
            resultExpectedBlocked.RetryAfter.Should().Be(onePer);
        }
    }
}
