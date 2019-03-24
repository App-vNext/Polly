using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using Polly.RateLimit;

namespace Polly
{
    public partial class Policy
    {

        /* Maybe these commented out overloads with TimeSpan permitOneExecutionPer are not as intuitive as the overloads left uncommented.

                    /// <summary>
                /// Builds a RateLimit <see cref="AsyncPolicy{TResult}"/> that will rate-limit executions to one per the timespan given.
                /// A <see cref="RateLimitRejectedException"/> will be thrown to indicate rate-limiting.
                /// </summary>
                /// <param name="permitOneExecutionPer">How often one execution is permitted.</param>
                /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
                /// <returns>The policy instance.</returns>
                public static AsyncRateLimitPolicy<TResult> RateLimitAsync<TResult>(TimeSpan permitOneExecutionPer)
                {
                    return RateLimitAsync<TResult>(permitOneExecutionPer, null);
                }

                /// <summary>
                /// Builds a RateLimit <see cref="AsyncPolicy{TResult}"/> that will rate-limit executions to one per the timespan given,
                /// with a maximum burst size of <paramref name="maxBurst"/>.
                /// A <see cref="RateLimitRejectedException"/> will be thrown to indicate rate-limiting.
                /// </summary>
                /// <param name="permitOneExecutionPer">How often one execution is permitted.</param>
                /// <param name="maxBurst">The maximum number of executions that will be permitted in a single burst (for example if none have been executed for a while).
                /// This equates to the bucket-capacity of a token-bucket implementation.</param>
                /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
                /// <returns>The policy instance.</returns>
                public static AsyncRateLimitPolicy<TResult> RateLimitAsync<TResult>(TimeSpan permitOneExecutionPer, int maxBurst)
                {
                    return RateLimitAsync<TResult>(permitOneExecutionPer, maxBurst, null);
                }

                /// <summary>
                /// Builds a RateLimit <see cref="AsyncPolicy{TResult}"/> that will rate-limit executions to one per the timespan given.
                /// </summary>
                /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
                /// <param name="permitOneExecutionPer">How often one execution is permitted.</param>
                /// <param name="retryAfterFactory">An (optional) factory to use to express retry-after back to the caller, when an operation is rate-limited.
                /// <remarks>This parameter can be null. If null, a <see cref="RateLimitRejectedException"/> will be thrown to indicate rate-limiting.</remarks></param>
                /// <returns>The policy instance.</returns>
                public static AsyncRateLimitPolicy<TResult> RateLimitAsync<TResult>(
                    TimeSpan permitOneExecutionPer,
                    Func<TimeSpan, Context, TResult> retryAfterFactory)
                {
                    return RateLimitAsync(permitOneExecutionPer, 1, retryAfterFactory);
                }

                /// <summary>
                /// Builds a RateLimit <see cref="AsyncPolicy{TResult}"/> that will rate-limit executions to one per the timespan given,
                /// with a maximum burst size of <paramref name="maxBurst"/>
                /// </summary>
                /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
                /// <param name="permitOneExecutionPer">How often one execution is permitted.</param>
                /// <param name="maxBurst">The maximum number of executions that will be permitted in a single burst (for example if none have been executed for a while).
                /// This equates to the bucket-capacity of a token-bucket implementation.</param>
                /// <param name="retryAfterFactory">An (optional) factory to use to express retry-after back to the caller, when an operation is rate-limited.
                /// <remarks>This parameter can be null. If null, a <see cref="RateLimitRejectedException"/> will be thrown to indicate rate-limiting.</remarks></param>
                /// <returns>The policy instance.</returns>
                public static AsyncRateLimitPolicy<TResult> RateLimitAsync<TResult>(
                    TimeSpan permitOneExecutionPer,
                    int maxBurst,
                    Func<TimeSpan, Context, TResult> retryAfterFactory)
                {
                    return RateLimitAsync(new TokenBucketRateLimiter(permitOneExecutionPer, maxBurst), retryAfterFactory);
                }
                */

        private static readonly Func<TimeSpan, int, IRateLimiter> DefaultRateLimiterFactory = (onePer, bucketCapacity) => new LockFreeTokenBucketRateLimiter(onePer, bucketCapacity);
        // private readonly Func<TimeSpan, int, IRateLimiter> DefaultRateLimiterFactory = (onePer, bucketCapacity) => new LockBasedTokenBucketRateLimiter(onePer, bucketCapacity);

        /// <summary>
        /// Builds a RateLimit <see cref="AsyncPolicy{TResult}"/> that will rate-limit executions to <paramref name="numberOfExecutions"/> per the timespan given.
        /// </summary>
        /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
        /// <param name="numberOfExecutions">The number of executions (call it N) permitted per timespan.</param>
        /// <param name="perTimeSpan">How often N executions are permitted.</param>
        /// <returns>The policy instance.</returns>
        public static AsyncRateLimitPolicy<TResult> RateLimitAsync<TResult>(
            int numberOfExecutions,
            TimeSpan perTimeSpan)
        {
            return RateLimitAsync<TResult>(numberOfExecutions, perTimeSpan, null);
        }

        /// <summary>
        /// Builds a RateLimit <see cref="AsyncPolicy{TResult}"/> that will rate-limit executions to <paramref name="numberOfExecutions"/> per the timespan given.
        /// </summary>
        /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
        /// <param name="numberOfExecutions">The number of executions (call it N) permitted per timespan.</param>
        /// <param name="perTimeSpan">How often N executions are permitted.</param>
        /// <param name="retryAfterFactory">An (optional) factory to use to express retry-after back to the caller, when an operation is rate-limited.
        /// <remarks>This parameter can be null. If null, a <see cref="RateLimitRejectedException"/> will be thrown to indicate rate-limiting.</remarks></param>
        /// <returns>The policy instance.</returns>
        public static AsyncRateLimitPolicy<TResult> RateLimitAsync<TResult>(
            int numberOfExecutions,
            TimeSpan perTimeSpan,
            Func<TimeSpan, Context, TResult> retryAfterFactory)
        {
            return RateLimitAsync(numberOfExecutions, perTimeSpan, 1, retryAfterFactory);
        }

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
        /// <remarks>This parameter can be null. If null, a <see cref="RateLimitRejectedException"/> will be thrown to indicate rate-limiting.</remarks></param>
        /// <returns>The policy instance.</returns>
        public static AsyncRateLimitPolicy<TResult> RateLimitAsync<TResult>(
            int numberOfExecutions,
            TimeSpan perTimeSpan,
            int maxBurst,
            Func<TimeSpan, Context, TResult> retryAfterFactory)
        {
            return RateLimitAsync(DefaultRateLimiterFactory(TimeSpan.FromTicks(perTimeSpan.Ticks / numberOfExecutions), maxBurst), retryAfterFactory);
        }

        /// <summary>
        /// Builds a RateLimit <see cref="AsyncPolicy{TResult}"/> that will rate-limit executions with the provided <paramref name="rateLimiter"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
        /// <param name="rateLimiter">The rate-limiter to use to determine whether the execution is permitted.</param>
        /// <param name="retryAfterFactory">An (optional) factory to use to express retry-after back to the caller, when an operation is rate-limited.
        /// <remarks>This parameter can be null. If null, a <see cref="RateLimitRejectedException"/> will be thrown to indicate rate-limiting.</remarks></param>
        /// <returns>The policy instance.</returns>
        public static AsyncRateLimitPolicy<TResult> RateLimitAsync<TResult>(
            IRateLimiter rateLimiter,
            Func<TimeSpan, Context, TResult> retryAfterFactory)
        {
            if (rateLimiter == null) throw new NullReferenceException(nameof(rateLimiter));

            return new AsyncRateLimitPolicy<TResult>(rateLimiter, retryAfterFactory);
        }
    }
}
