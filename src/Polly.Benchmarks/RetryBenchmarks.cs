using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Polly.Benchmarks
{
    [Config(typeof(PollyConfig))]
    public class RetryBenchmarks
    {
        [Benchmark]
        public void Retry_Synchronous()
        {
            var policy = Policy.Handle<InvalidOperationException>().Retry();

            int count = 0;

            policy.Execute(() =>
            {
                if (count++ % 2 == 0)
                {
                    throw new InvalidOperationException();
                }
            });
        }

        [Benchmark]
        public async Task Retry_Aynchronous()
        {
            var policy = Policy.Handle<InvalidOperationException>().RetryAsync();

            int count = 0;

            await policy.ExecuteAsync(() =>
            {
                if (count++ % 2 == 0)
                {
                    throw new InvalidOperationException();
                }

                return Task.CompletedTask;
            });
        }
    }
}
