namespace Polly.CircuitBreaker
{
    /// <summary>
    /// Store and report health metrics
    /// </summary>
    public interface IHealthCount
    {
        /// <summary>
        /// Success count
        /// </summary>
        int Successes { get; }

        /// <summary>
        /// Failure count
        /// </summary>
        int Failures { get; }

        /// <summary>
        /// Total count (probably success + failure)
        /// </summary>
        int Total { get; }

        /// <summary>
        /// Start time for metric collection
        /// </summary>
        long StartedAt { get; }
    }
}
