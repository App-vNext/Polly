namespace Polly.Specs.RateLimit;

public abstract class RateLimitPolicyTResultSpecsBase : RateLimitPolicySpecsBase
{
    protected abstract IRateLimitPolicy<TResult> GetPolicyViaSyntax<TResult>(
        int numberOfExecutions,
        TimeSpan perTimeSpan,
        int maxBurst,
        Func<TimeSpan, Context, TResult> retryAfterFactory);

    protected abstract TResult TryExecuteThroughPolicy<TResult>(IRateLimitPolicy<TResult> policy, Context context, TResult resultIfExecutionPermitted);

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void Ratelimiter_specifies_correct_wait_until_next_execution_by_custom_factory_passing_correct_context(int onePerSeconds)
    {
        FixClock();

        // Arrange
        TimeSpan onePer = TimeSpan.FromSeconds(onePerSeconds);
        Context? contextPassedToRetryAfter = null;
        Func<TimeSpan, Context, ResultClassWithRetryAfter> retryAfterFactory = (t, ctx) =>
        {
            contextPassedToRetryAfter = ctx;
            return new ResultClassWithRetryAfter(t);
        };
        var rateLimiter = GetPolicyViaSyntax<ResultClassWithRetryAfter>(1, onePer, 1, retryAfterFactory);

        // Arrange - drain first permitted execution after initialising.
        ShouldPermitAnExecution(rateLimiter);

        // Arrange
        // (do nothing - time not advanced)

        // Act - try another execution.
        Context contextToPassIn = [];
        var resultExpectedBlocked = TryExecuteThroughPolicy(rateLimiter, contextToPassIn, new ResultClassWithRetryAfter(ResultPrimitive.Good));

        // Assert - should be blocked - time not advanced.
        resultExpectedBlocked.ResultCode.ShouldNotBe(ResultPrimitive.Good);

        // Result should be expressed per the retryAfterFactory.
        resultExpectedBlocked.RetryAfter.ShouldBe(onePer);

        // Context should have been passed to the retryAfterFactory.
        contextPassedToRetryAfter.ShouldNotBeNull();
        contextPassedToRetryAfter.ShouldBeSameAs(contextToPassIn);
    }
}
