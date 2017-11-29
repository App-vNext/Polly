namespace Polly.Specs.Helpers
{
    /// <summary>
    /// Constants supporting tests.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Denotes a test collection dependent on manipulating the abstracted <see cref="Polly.Utilities.SystemClock"/>.  <remarks>These tests are not parallelized.</remarks>
        /// </summary>
        public const string SystemClockDependentTestCollection = "SystemClockDependentTestCollection";
    }
}