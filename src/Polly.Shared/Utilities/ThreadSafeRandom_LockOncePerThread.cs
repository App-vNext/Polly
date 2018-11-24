using System;
using System.Threading;

// Note: Given the current .NET Core (2.1) and .NET Framework (4.7.2) implementations of Random, 
// this implementation intentionally sacrifices a (marginal) degree of randomness in favour of speed, by locking only once-per-thread.
// An implementation locking once-per-random-number-generated will generate a marginally more random sequence.
// This implementation intentionally favours execution speed over that marginal extra degree of randomness.
// We choose that trade-off where the random number generator is used to select executions for chaos/fault injection, where we want minimum extra drag per execution.
// In other scenarios such as randomising jitter, we may (with a different implementation) favour randomness at the expense of locking once-per-random-number-generated.

// References:
// - https://stackoverflow.com/a/25448166/
// - https://docs.microsoft.com/en-us/dotnet/api/system.random?view=netframework-4.7.2#the-systemrandom-class-and-thread-safety
// - https://stackoverflow.com/questions/25390301/
// - https://github.com/App-vNext/Polly/issues/530#issuecomment-439680613

namespace Polly.Utilities
{
    /// <summary>
    /// An implementation of a Random Number Generator that is thread-safe for generating random numbers concurrently on multiple threads.
    /// <remarks>Thread-safety without locking-per-random-number-generated is achieved simply by storing and using a ThreadStatic instance of <see cref="Random"/> per thread.</remarks>
    /// </summary>
    public class ThreadSafeRandom_LockOncePerThread
    {
        /// <summary>
        /// Initializes the static <see cref="ThreadSafeRandom_LockOncePerThread"/>, setting the <see cref="NextDouble"/> method to a thread-safe implementation which locks only once-per-thread.
        /// </summary>
        static ThreadSafeRandom_LockOncePerThread()
        {
            Reset();
        }

        private static readonly Random s_globalRandom = new Random();

        private static readonly ThreadLocal<Random> t_threadRandom = new ThreadLocal<Random>(InitializeThreadRandom);

        private static Random InitializeThreadRandom()
        {
            int seed;
            // We must lock minimally once-per-thread. If the instance s_globalRandom is accessed on multiple threads concurrently, 
            // the current .NET Framework and .NET Core implementations of Random have a liability to return sequences of zeros.
            // See the articles referenced at the head of this class.
            lock (s_globalRandom)
            {
                seed = s_globalRandom.Next();
            }

            return new Random(seed);
        }

        /// <summary>
        /// Returns a random floating point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        public static Func<double> NextDouble;

        /// <summary>
        /// Method to reset the random generator.
        /// </summary>
        public static void Reset()
        {
            NextDouble = () => t_threadRandom.Value.NextDouble();
        }
    }
}
