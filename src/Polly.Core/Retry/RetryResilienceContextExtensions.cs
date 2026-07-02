namespace Polly;

/// <summary>
///   Extension methods for <see cref="ResilienceContext"/> to store and retrieve properties related to the Retry strategy.
/// </summary>
public static class RetryResilienceContextExtensions
{
    internal static readonly ResiliencePropertyKey<int> AttemptNumberPropertyKey = new("Retry.AttemptNumber");

    /// <summary>
    /// Gets the current attempt number from the <see cref="ResilienceContext"/> (starting with <c>0</c> for the first attempt).
    /// </summary>
    /// <param name="context">The <see cref="ResilienceContext"/> instance.</param>
    /// <returns>The current attempt number.</returns>
    public static int GetRetryAttemptNumber(this ResilienceContext context)
    {
        Guard.NotNull(context);

        return context.Properties.TryGetValue(AttemptNumberPropertyKey, out var value)
            ? value
            : 0;
    }

    /// <summary>
    /// Sets the attempt number in the <see cref="ResilienceContext"/>.
    /// </summary>
    /// <param name="context">The <see cref="ResilienceContext"/> instance.</param>
    /// <param name="attemptNumber">The attempt number to set.</param>
    /// <remarks>
    /// Application code can use this method to set the initial attempt number when executing a retry strategy.
    /// The strategy will update the attempt number on each retry, and it can be retrieved using <see cref="GetRetryAttemptNumber"/>.
    /// The strategy will overwrite the attempt number on each retry, so any value set inside callbacks may be lost or cause inconsistent behavior.
    /// </remarks>
    public static void SetRetryAttemptNumber(this ResilienceContext context, int attemptNumber)
    {
        Guard.NotNull(context);

        if (attemptNumber < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptNumber), attemptNumber, "Attempt number must be non-negative.");
        }

        context.Properties.Set(AttemptNumberPropertyKey, attemptNumber);
    }
}
