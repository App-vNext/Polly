using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Polly.Benchmarks
{
    [Config(typeof(PollyConfig))]
    public class Retry
    {
        [Benchmark]
        public void Retry_Synchronous_Succeeds()
        {
            var policy = Policy.Handle<InvalidOperationException>().Retry();
            policy.Execute(() => Workloads.Action());
        }

        [Benchmark]
        public async Task Retry_Asynchronous_Succeeds()
        {
            var policy = Policy.Handle<InvalidOperationException>().RetryAsync();
            await policy.ExecuteAsync(() => Workloads.ActionAsync());
        }

        [Benchmark]
        public async Task Retry_Asynchronous_Succeeds_With_CancellationToken()
        {
            var cancellationToken = CancellationToken.None;
            var policy = Policy.Handle<InvalidOperationException>().RetryAsync();
            await policy.ExecuteAsync((token) => Workloads.ActionAsync(token), cancellationToken);
        }

        [Benchmark]
        public int Retry_Synchronous_With_Result_Succeeds()
        {
            var policy = Policy.Handle<InvalidOperationException>().Retry();
            return policy.Execute(() => Workloads.Func<int>());
        }

        [Benchmark]
        public async Task<int> Retry_Asynchronous_With_Result_Succeeds()
        {
            var policy = Policy.Handle<InvalidOperationException>().RetryAsync();
            return await policy.ExecuteAsync(() => Workloads.FuncAsync<int>());
        }

        [Benchmark]
        public async Task<int> Retry_Asynchronous_With_Result_Succeeds_With_CancellationToken()
        {
            var cancellationToken = CancellationToken.None;
            var policy = Policy.Handle<InvalidOperationException>().RetryAsync();
            return await policy.ExecuteAsync((token) => Workloads.FuncAsync<int>(token), cancellationToken);
        }

        [Benchmark]
        public void Retry_Synchronous_Throws_Then_Succeeds()
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
        public async Task Retry_Asynchronous_Throws_Then_Succeeds()
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
