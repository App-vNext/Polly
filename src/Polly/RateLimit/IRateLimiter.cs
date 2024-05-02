#nullable enable

namespace Polly.RateLimit;

/// <summary>
/// Defines methods to be provided by a rate-limiter used in a Polly <see cref="IRateLimitPolicy"/>.
/// </summary>
internal interface IRateLimiter
{
    /// <summary>
    /// Returns whether the execution is permitted; if not, returns what <see cref="TimeSpan"/> should be waited before retrying.
    /// <remarks>Calling this method consumes an execution permit if one is available: a caller receiving a return value true should make an execution.</remarks>
    /// </summary>
    /// <returns>
    /// A tuple whose first element is a value indicating whether the execution is permitted,
    /// and whose second element is the <see cref="TimeSpan"/> should be waited before retrying.
    /// </returns>
    (bool PermitExecution, TimeSpan RetryAfter) PermitExecution();
}
