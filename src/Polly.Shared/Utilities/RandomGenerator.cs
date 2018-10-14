using System;

namespace Polly.Utilities
{
    /// <summary>
    /// Methods to generate random number and improve the testability of the code.
    /// </summary>
    public static class RandomGenerator
    {
        /// <summary>
        /// internal random object
        /// </summary>
        private static Random rand = new Random();

        /// <summary>
        /// Delegate Method to return a random number between 0 and 1
        /// </summary>
        /// <returns>a random <see cref="Double"/> between [0,1]</returns>
        public static Func<Double> GetRandomNumber = () => rand.NextDouble();

        /// <summary>
        /// Method to reset the random generator
        /// </summary>
        public static void Reset()
        {
            GetRandomNumber = () => rand.NextDouble();
        }
    }
}
