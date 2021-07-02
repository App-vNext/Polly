using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Polly.Benchmarks
{
    [Config(typeof(PollyConfig))]
    public class CircuitBreaker
    {
        private static readonly Policy SyncPolicy = Policy.Handle<InvalidOperationException>().CircuitBreaker(2, TimeSpan.FromMinutes(1));
        private static readonly AsyncPolicy AsyncPolicy = Policy.Handle<InvalidOperationException>().CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        [Benchmark]
        public void CircuitBreaker_Synchronous_Succeeds()
        {
            SyncPolicy.Execute(() => Workloads.Action());
        }

        [Benchmark]
        public async Task CircuitBreaker_Asynchronous_Succeeds()
        {
            await AsyncPolicy.ExecuteAsync((token) => Workloads.ActionAsync(token), CancellationToken.None);
        }

        [Benchmark]
        public int CircuitBreaker_Synchronous_With_Result_Succeeds()
        {
            return SyncPolicy.Execute(() => Workloads.Func<int>());
        }

        [Benchmark]
        public async Task<int> CircuitBreaker_Asynchronous_With_Result_Succeeds()
        {
            return await AsyncPolicy.ExecuteAsync((token) => Workloads.FuncAsync<int>(token), CancellationToken.None);
        }
    }
}
