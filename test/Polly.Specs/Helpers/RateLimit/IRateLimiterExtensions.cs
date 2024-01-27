namespace Polly.Specs.Helpers.RateLimit;

internal static class IRateLimiterExtensions
{
    public static void ShouldPermitAnExecution(this IRateLimiter rateLimiter)
    {
        (bool PermitExecution, TimeSpan RetryAfter) canExecute = rateLimiter.PermitExecution();

        canExecute.PermitExecution.Should().BeTrue();
        canExecute.RetryAfter.Should().Be(TimeSpan.Zero);
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
        (bool PermitExecution, TimeSpan RetryAfter) canExecute = rateLimiter.PermitExecution();

        canExecute.PermitExecution.Should().BeFalse();
        if (retryAfter == null)
        {
            canExecute.RetryAfter.Should().BeGreaterThan(TimeSpan.Zero);
        }
        else
        {
            canExecute.RetryAfter.Should().Be(retryAfter.Value);
        }
    }
}
