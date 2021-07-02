using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Polly.Benchmarks
{
    [Config(typeof(PollyConfig))]
    public class Bulkhead
    {
        private static readonly Policy SyncPolicy = Policy.Bulkhead(2);
        private static readonly AsyncPolicy AsyncPolicy = Policy.BulkheadAsync(2);

        [Benchmark]
        public void Bulkhead_Synchronous()
        {
            SyncPolicy.Execute(() => Workloads.Action());
        }

        [Benchmark]
        public async Task Bulkhead_Asynchronous()
        {
            await AsyncPolicy.ExecuteAsync(() => Workloads.ActionAsync());
        }

        [Benchmark]
        public async Task Bulkhead_Asynchronous_With_CancellationToken()
        {
            await AsyncPolicy.ExecuteAsync((token) => Workloads.ActionAsync(token), CancellationToken.None);
        }

        [Benchmark]
        public int Bulkhead_Synchronous_With_Result()
        {
            return SyncPolicy.Execute(() => Workloads.Func<int>());
        }

        [Benchmark]
        public async Task<int> Bulkhead_Asynchronous_With_Result()
        {
            return await AsyncPolicy.ExecuteAsync(() => Workloads.FuncAsync<int>());
        }

        [Benchmark]
        public async Task<int> Bulkhead_Asynchronous_With_Result_With_CancellationToken()
        {
            return await AsyncPolicy.ExecuteAsync((token) => Workloads.FuncAsync<int>(token), CancellationToken.None);
        }
    }
}
