#nullable enable
namespace Polly;

public partial class Policy
{
    /// <summary>
    /// Builds a RateLimit <see cref="AsyncPolicy"/> that will rate-limit executions to <paramref name="numberOfExecutions"/> per the timespan given.
    /// </summary>
    /// <param name="numberOfExecutions">The number of executions (call it N) permitted per timespan.</param>
    /// <param name="perTimeSpan">How often N executions are permitted.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRateLimitPolicy RateLimitAsync(
        int numberOfExecutions,
        TimeSpan perTimeSpan) =>
        RateLimitAsync(numberOfExecutions, perTimeSpan, 1);

    /// <summary>
    /// Builds a RateLimit <see cref="AsyncPolicy"/> that will rate-limit executions to <paramref name="numberOfExecutions"/> per the timespan given.
    /// </summary>
    /// <param name="numberOfExecutions">The number of executions (call it N) permitted per timespan.</param>
    /// <param name="perTimeSpan">How often N executions are permitted.</param>
    /// <param name="maxBurst">The maximum number of executions that will be permitted in a single burst (for example if none have been executed for a while).
    /// This equates to the bucket-capacity of a token-bucket implementation.</param>
    /// <returns>The policy instance.</returns>
    public static AsyncRateLimitPolicy RateLimitAsync(
        int numberOfExecutions,
        TimeSpan perTimeSpan,
        int maxBurst)
    {
        if (numberOfExecutions < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfExecutions), numberOfExecutions, $"{nameof(numberOfExecutions)} per timespan must be an integer greater than or equal to 1.");
        }

        if (perTimeSpan <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(perTimeSpan), perTimeSpan, $"{nameof(perTimeSpan)} must be a positive timespan.");
        }

        if (maxBurst < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBurst), maxBurst, $"{nameof(maxBurst)} must be an integer greater than or equal to 1.");
        }

        var onePer = TimeSpan.FromTicks(perTimeSpan.Ticks / numberOfExecutions);

        if (onePer <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(perTimeSpan), perTimeSpan, "The number of executions per timespan must be positive.");
        }

        IRateLimiter rateLimiter = RateLimiterFactory.Create(onePer, maxBurst);

        return new AsyncRateLimitPolicy(rateLimiter);
    }
}
