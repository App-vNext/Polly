#nullable enable

namespace Polly;

public partial class Policy
{
    /// <summary>
    /// Builds a RateLimit <see cref="AsyncPolicy{TResult}"/> that will rate-limit executions to <paramref name="numberOfExecutions"/> per the timespan given.
    /// </summary>
    /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
    /// <param name="numberOfExecutions">The number of executions (call it N) permitted per timespan.</param>
    /// <param name="perTimeSpan">How often N executions are permitted.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRateLimitPolicy<TResult> RateLimitAsync<TResult>(
        int numberOfExecutions,
        TimeSpan perTimeSpan) =>
        RateLimitAsync<TResult>(numberOfExecutions, perTimeSpan, null);

    /// <summary>
    /// Builds a RateLimit <see cref="AsyncPolicy{TResult}"/> that will rate-limit executions to <paramref name="numberOfExecutions"/> per the timespan given.
    /// </summary>
    /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
    /// <param name="numberOfExecutions">The number of executions (call it N) permitted per timespan.</param>
    /// <param name="perTimeSpan">How often N executions are permitted.</param>
    /// <param name="retryAfterFactory">An (optional) factory to express the recommended retry-after time back to the caller, when an operation is rate-limited.
    /// <remarks>If null, a <see cref="RateLimitRejectedException"/> with property <see cref="RateLimitRejectedException.RetryAfter"/> will be thrown to indicate rate-limiting.</remarks></param>
    /// <returns>The policy instance.</returns>
    public static AsyncRateLimitPolicy<TResult> RateLimitAsync<TResult>(
        int numberOfExecutions,
        TimeSpan perTimeSpan,
        Func<TimeSpan, Context, TResult>? retryAfterFactory) =>
        RateLimitAsync(numberOfExecutions, perTimeSpan, 1, retryAfterFactory);

    /// <summary>
    /// Builds a RateLimit <see cref="AsyncPolicy{TResult}"/> that will rate-limit executions to <paramref name="numberOfExecutions"/> per the timespan given.
    /// </summary>
    /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
    /// <param name="numberOfExecutions">The number of executions (call it N) permitted per timespan.</param>
    /// <param name="perTimeSpan">How often N executions are permitted.</param>
    /// <param name="maxBurst">The maximum number of executions that will be permitted in a single burst (for example if none have been executed for a while).
    /// This equates to the bucket-capacity of a token-bucket implementation.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRateLimitPolicy<TResult> RateLimitAsync<TResult>(
        int numberOfExecutions,
        TimeSpan perTimeSpan,
        int maxBurst) =>
        RateLimitAsync<TResult>(numberOfExecutions, perTimeSpan, maxBurst, null);

    /// <summary>
    /// Builds a RateLimit <see cref="AsyncPolicy{TResult}"/> that will rate-limit executions to <paramref name="numberOfExecutions"/> per the timespan given,
    /// with a maximum burst size of <paramref name="maxBurst"/>
    /// </summary>
    /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
    /// <param name="numberOfExecutions">The number of executions (call it N) permitted per timespan.</param>
    /// <param name="perTimeSpan">How often N executions are permitted.</param>
    /// <param name="maxBurst">The maximum number of executions that will be permitted in a single burst (for example if none have been executed for a while).
    /// This equates to the bucket-capacity of a token-bucket implementation.</param>
    /// <param name="retryAfterFactory">An (optional) factory to use to express retry-after back to the caller, when an operation is rate-limited.
    /// <remarks>If null, a <see cref="RateLimitRejectedException"/> with property <see cref="RateLimitRejectedException.RetryAfter"/> will be thrown to indicate rate-limiting.</remarks></param>
    /// <returns>The policy instance.</returns>
    public static AsyncRateLimitPolicy<TResult> RateLimitAsync<TResult>(
        int numberOfExecutions,
        TimeSpan perTimeSpan,
        int maxBurst,
        Func<TimeSpan, Context, TResult>? retryAfterFactory)
    {
        if (numberOfExecutions < 1)
            throw new ArgumentOutOfRangeException(nameof(numberOfExecutions), numberOfExecutions, $"{nameof(numberOfExecutions)} per timespan must be an integer greater than or equal to 1.");
        if (perTimeSpan <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(perTimeSpan), perTimeSpan, $"{nameof(perTimeSpan)} must be a positive timespan.");
        if (maxBurst < 1)
            throw new ArgumentOutOfRangeException(nameof(maxBurst), maxBurst, $"{nameof(maxBurst)} must be an integer greater than or equal to 1.");

        var onePer = TimeSpan.FromTicks(perTimeSpan.Ticks / numberOfExecutions);

        if (onePer <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(perTimeSpan), perTimeSpan, "The number of executions per timespan must be positive.");
        }

        IRateLimiter rateLimiter = RateLimiterFactory.Create(onePer, maxBurst);

        return new AsyncRateLimitPolicy<TResult>(rateLimiter, retryAfterFactory);
    }
}
