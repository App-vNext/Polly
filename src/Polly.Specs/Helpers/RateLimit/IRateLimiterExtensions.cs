using System;
using FluentAssertions;
using Polly.RateLimit;

namespace Polly.Specs.Helpers.RateLimit
{
    internal static class IRateLimiterExtensions
    {
        public static void ShouldPermitAnExecution(this IRateLimiter rateLimiter)
        {
            (bool permitExecution, TimeSpan retryAfter) canExecute = rateLimiter.PermitExecution();

            canExecute.permitExecution.Should().BeTrue();
            canExecute.retryAfter.Should().Be(TimeSpan.Zero);
        }

        public static void ShouldPermitNExecutions(this IRateLimiter rateLimiter, long numberOfExecutions)
        {
            for (int execution = 0; execution < numberOfExecutions; execution++)
            {
                rateLimiter.ShouldPermitAnExecution();
            }
        }

        public static void ShouldNotPermitAnExecution(this IRateLimiter rateLimiter, TimeSpan? retryAfter = null)
        {
            (bool permitExecution, TimeSpan retryAfter) canExecute = rateLimiter.PermitExecution();

            canExecute.permitExecution.Should().BeFalse();
            if (retryAfter == null)
            {
                canExecute.retryAfter.Should().BeGreaterThan(TimeSpan.Zero);
            }
            else
            {
                canExecute.retryAfter.Should().Be(retryAfter.Value);
            }
        }
    }
}