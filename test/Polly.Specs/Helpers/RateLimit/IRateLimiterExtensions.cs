namespace Polly.Specs.Helpers.RateLimit;

internal static class IRateLimiterExtensions
{
    public static void ShouldPermitAnExecution(this IRateLimiter rateLimiter)
    {
        (bool PermitExecution, TimeSpan RetryAfter) canExecute = rateLimiter.PermitExecution();

        canExecute.PermitExecution.ShouldBeTrue();
        canExecute.RetryAfter.ShouldBe(TimeSpan.Zero);
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

        canExecute.PermitExecution.ShouldBeFalse();
        if (retryAfter == null)
        {
            canExecute.RetryAfter.ShouldBeGreaterThan(TimeSpan.Zero);
        }
        else
        {
            canExecute.RetryAfter.ShouldBe(retryAfter.Value);
        }
    }
}
