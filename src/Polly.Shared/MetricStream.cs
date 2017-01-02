using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Shared
{
    /// Find and return the current metrics for the system (in Hystrix format)
    public class MetricStream
    {
        /// an infinite stream of all metrics
        /// <param name="sleepFunc">The method will be called between each set of outputs to slow the stream down. We suggest "() => Thread.Sleep(1000)"</param>
        public static IEnumerable<object> All(Action sleepFunc)
        {
            while (true)
            {
                foreach (var policy in CollectedPolicies.All)
                {
                    yield return new HystrixCommand
                    {
                        rollingCountSuccess = policy.Value.HealthCount.Successes,
                        rollingCountFailure = policy.Value.HealthCount.Failures,
                        isCircuitBreakerOpen = (policy.Value.CircuitState != CircuitBreaker.CircuitState.Closed),
                        name = policy.Key,
                        group = "Group",
                        latencyExecute = new Dictionary<string, int>() { { "0", 0 }, { "25", 0 }, { "50", 0 }, { "75", 0 }, { "90", 0 }, { "95", 0 }, { "99", 0 }, { "99.5", 0 }, { "100", 0 } },
                        latencyTotal = new Dictionary<string, int>() { { "0", 0 }, { "25", 0 }, { "50", 0 }, { "75", 0 }, { "90", 0 }, { "95", 0 }, { "99", 0 }, { "99.5", 0 }, { "100", 0 } },
                        propertyValue_executionIsolationStrategy = "THREAD",
                        threadPool = "ThreadPool"
                    };
                }

                // yield return new HystrixThreadPool { type = "HystrixThreadPool", name = "Order" };

                sleepFunc.Invoke();
            }
        }
    }
}
