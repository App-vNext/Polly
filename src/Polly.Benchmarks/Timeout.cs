using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Polly.Benchmarks
{
    [Config(typeof(PollyConfig))]
    public class Timeout
    {
        private static readonly Policy SyncPolicy = Policy.Timeout(TimeSpan.FromMilliseconds(1));
        private static readonly AsyncPolicy AsyncPolicy = Policy.TimeoutAsync(TimeSpan.FromMilliseconds(1));

        [Benchmark]
        public void Timeout_Synchronous_Succeeds()
        {
            SyncPolicy.Execute(() => Workloads.Action());
        }

        [Benchmark]
        public async Task Timeout_Asynchronous_Succeeds()
        {
            await AsyncPolicy.ExecuteAsync(() => Workloads.ActionAsync());
        }

        [Benchmark]
        public async Task Timeout_Asynchronous_Succeeds_With_CancellationToken()
        {
            await AsyncPolicy.ExecuteAsync((token) => Workloads.ActionAsync(token), CancellationToken.None);
        }

        [Benchmark]
        public int Timeout_Synchronous_With_Result_Succeeds()
        {
            return SyncPolicy.Execute(() => Workloads.Func<int>());
        }

        [Benchmark]
        public async Task<int> Timeout_Asynchronous_With_Result_Succeeds()
        {
            return await AsyncPolicy.ExecuteAsync(() => Workloads.FuncAsync<int>());
        }

        [Benchmark]
        public async Task<int> Timeout_Asynchronous_With_Result_Succeeds_With_CancellationToken()
        {
            return await AsyncPolicy.ExecuteAsync((token) => Workloads.FuncAsync<int>(token), CancellationToken.None);
        }

        [Benchmark]
        public async Task Timeout_Asynchronous_Times_Out_Optimistic()
        {
            await AsyncPolicy.ExecuteAsync((token) => Workloads.ActionInfiniteAsync(token), CancellationToken.None);
        }
    }
}
