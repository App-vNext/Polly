using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Polly.Benchmarks
{
    [Config(typeof(PollyConfig))]
    public class Fallback
    {
        private static readonly Policy<int> SyncPolicy = Policy<int>.Handle<InvalidOperationException>().Fallback(0);
        private static readonly AsyncPolicy<int> AsyncPolicy = Policy<int>.Handle<InvalidOperationException>().FallbackAsync(0);

        [Benchmark]
        public int Fallback_Synchronous_Succeeds()
        {
            return SyncPolicy.Execute(() => Workloads.Func<int>());
        }

        [Benchmark]
        public async Task<int> Fallback_Asynchronous_Succeeds()
        {
            return await AsyncPolicy.ExecuteAsync(() => Workloads.FuncAsync<int>());
        }

        [Benchmark]
        public int Fallback_Synchronous_Throws()
        {
            return SyncPolicy.Execute(() => Workloads.FuncThrows<int, InvalidOperationException>());
        }

        [Benchmark]
        public async Task<int> Fallback_Asynchronous_Throws()
        {
            return await AsyncPolicy.ExecuteAsync(() => Workloads.FuncThrowsAsync<int, InvalidOperationException>());
        }
    }
}
