using System;

namespace Polly.Utilities
{
    /// <summary>
    /// TODO: add methods for determinism in tests
    /// Methods to generate random number and improve the testability of the code.
    /// </summary>
    public static class RandomGenerator
    {
        /// <summary>
        /// internal random object
        /// </summary>
        private static Random rand = new Random();

        /// <summary>
        /// Method to return a random number between 0 and 1
        /// </summary>
        /// <returns>a random <see cref="Double"/> between [0,1]</returns>
        public static Double GetRandomNumber()
        {
            return rand.NextDouble();
        }
    }
}
