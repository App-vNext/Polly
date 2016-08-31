using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Shared
{
    /// <summary>
    /// Data class for output to be consumed by Hystrix Dashboard (https://github.com/Netflix/Hystrix/tree/master/hystrix-dashboard)
    /// </summary>
    public class HystrixCommand
    {
        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public HystrixCommand()
        {
            currentTime = (long)((DateTime.UtcNow - Jan1St1970).TotalMilliseconds);
            rollingCountTimeout = -1;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string type { get { return "HystrixCommand"; } }
        public string name { get; set; }
        public string group { get; set; }
        public long currentTime { get; set; }
        public bool isCircuitBreakerOpen { get; set; }
        public int errorPercentage { get; set; }
        public int errorCount { get; set; }
        public int requestCount { get; set; }
        public int rollingCountBadRequests { get; set; }
        public int rollingCountCollapsedRequests { get; set; }
        public int rollingCountEmit { get; set; }
        public int rollingCountExceptionsThrown { get; set; }
        public int rollingCountFailure { get; set; }
        public int rollingCountFallbackEmit { get; set; }
        public int rollingCountFallbackFailure { get; set; }
        public int rollingCountFallbackMissing { get; set; }
        public int rollingCountFallbackRejection { get; set; }
        public int rollingCountFallbackSuccess { get; set; }
        public int rollingCountResponsesFromCache { get; set; }
        public int rollingCountSemaphoreRejected { get; set; }
        public int rollingCountShortCircuited { get; set; }
        public int rollingCountSuccess { get; set; }
        public int rollingCountThreadPoolRejected { get; set; }
        public int rollingCountTimeout { get; set; }
        public int currentConcurrentExecutionCount { get; set; }
        public int rollingMaxConcurrentExecutionCount { get; set; }
        public int latencyExecute_mean { get; set; }
        public Dictionary<string, int> latencyExecute { get; set; }
        public int latencyTotal_mean { get; set; }
        public Dictionary<string, int> latencyTotal { get; set; }
        public int propertyValue_circuitBreakerRequestVolumeThreshold { get; set; }
        public int propertyValue_circuitBreakerSleepWindowInMilliseconds { get; set; }
        public int propertyValue_circuitBreakerErrorThresholdPercentage { get; set; }
        public bool propertyValue_circuitBreakerForceOpen { get; set; }
        public bool propertyValue_circuitBreakerForceClosed { get; set; }
        public bool propertyValue_circuitBreakerEnabled { get; set; }
        public string propertyValue_executionIsolationStrategy { get; set; }
        public int propertyValue_executionIsolationThreadTimeoutInMilliseconds { get; set; }
        public int propertyValue_executionTimeoutInMilliseconds { get; set; }
        public bool propertyValue_executionIsolationThreadInterruptOnTimeout { get; set; }
        public object propertyValue_executionIsolationThreadPoolKeyOverride { get; set; }
        public int propertyValue_executionIsolationSemaphoreMaxConcurrentRequests { get; set; }
        public int propertyValue_fallbackIsolationSemaphoreMaxConcurrentRequests { get; set; }
        public int propertyValue_metricsRollingStatisticalWindowInMilliseconds { get; set; }
        public bool propertyValue_requestCacheEnabled { get; set; }
        public bool propertyValue_requestLogEnabled { get; set; }
        public int reportingHosts { get; set; }
        public string threadPool { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
