namespace Polly.Specs.Helpers;

/// <summary>
/// Constants supporting tests.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Denotes a test collection dependent on manipulating the abstracted <see cref="SystemClock"/>.
    /// </summary>
    /// <remarks>These tests are not parallelized.</remarks>
    public const string SystemClockDependentTestCollection = "SystemClockDependentTestCollection";

    /// <summary>
    /// Denotes a test collection making heavy use of parallel threads.
    /// </summary>
    /// <remarks>These tests are not run in parallel with each other, to reduce heavy use of threads in the build/CI environment.</remarks>
    public const string ParallelThreadDependentTestCollection = "ParallelThreadDependentTestCollection";
}
